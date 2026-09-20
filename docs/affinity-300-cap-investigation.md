# Affinity maximum 300: investigation and experimental fix (2026-09-20)

## Evidence and limits

Public gameplay guides describe the ordinary affinity range as -300 to +300, with 300 corresponding to five hearts:
- https://www.gamersky.com/handbook/202103/1368709.shtml
- https://www.taptap.cn/moment/619323840203327612
- https://tale-of-immortal.fandom.com/wiki/NPC

In the user's 0.2.3 runtime log, a protected NPC -> player relationship rose from 302 to a raw 303.375. The game immediately requested SetIntim(300), but our old prefix substituted 303.375 and left the relationship above the ordinary maximum. The specific reason the game requested 300 has not been confirmed by decompiling its native code; cap normalization is a plausible explanation. The user subsequently reported several crashes during their testing. **We do not know whether the mod, the out-of-range value, another mod, or the game caused those crashes.** We must not claim crash prevention or save safety from this patch alone.

## Experimental 0.2.4 code now committed; NOT YET COMPILED OR RUNTIME-TESTED

Shared pure rule: `ModCode/ModMain/AffinityCapPolicy.cs`, with dependency-free test harness in `tools/AffinityCapTests/`. Both entry points build against the same policy. `ModMain.cs` applies it only after the existing current Married/Lover filter, using the validated raw float when possible and the existing integer fallback otherwise. The negative AddIntim cancellation remains unchanged, as do unrelated NPC writes and ClearIntim.

For a current value at or below 300: a requested decrease is held at the exact current value, while gains are allowed up to 300. For an already-overcap current value: the next protected SetIntim write normalizes it to 300, regardless of whether that write requested an increase or a decrease. A new request above 300 is limited to 300. There is no background sweep, save-file migration, or guarantee that every alternative affinity writer uses SetIntim. A blocked negative AddIntim by itself does not normalize an overcap value. An invalid raw read retains the prior integer fallback (fractional protection then remains uncertain).

The logger distinguishes `CAP NORMALIZED`, `CAP LIMITED`, and `DECAY BLOCKED`, with protected before/after raw readings. No claim of successful compilation, test execution, in-game verification, crash repair, or complete protection is made by these commits.

## First run: safeguard, compile and unit-test

With Tale of Immortal completely closed, first copy your current `Mods/AffinityProtectionStandalone.dll` somewhere safe, and separately back up your game saves. **Do not overwrite your primary save with this experimental revision.** In the repository root:

```powershell
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
$installed = Join-Path $gameDir 'Mods\AffinityProtectionStandalone.dll'
Copy-Item $installed "$env:USERPROFILE\Desktop\AffinityProtectionStandalone-pre-cap-fix.dll" -Force
git pull origin main
dotnet run --project .\tools\AffinityCapTests\AffinityCapTests.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Policy tests failed; do not install.' }
dotnet build .\tools\AffinityStandalone\AffinityStandalone.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Standalone build failed; do not install.' }
```

Only with passing tests and build, with the game closed:

```powershell
Copy-Item '.\tools\AffinityStandalone\bin\Release\net6.0\AffinityProtectionStandalone.dll' $installed -Force
```

## Minimal disposable-save gameplay test

1. Keep the old in-game Local Mod disabled/removed, and do not install duplicate affinity patch providers. On a copy of a save, start the game, enter the world, and confirm the log contains `cap-aware fractional guard build 0.2.4-test`, `patched ... AddIntim`, and `patched ... SetIntim`.
2. With an existing protected relationship that has a raw value over 300 on the copy, trigger a benign affinity interaction. Expect `CAP NORMALIZED` and `SetIntim AFTER ... raw=300`; **do not** assume loading alone normalizes stored data.
3. Test a relationship below 300 with a fractional attempted loss: expect `DECAY BLOCKED` and unchanged raw affinity. A gain that would exceed 300 should produce `CAP LIMITED` and end at 300.
4. Test both directions if practical, plus unrelated NPC affinity loss, and save/reload only the disposable copy to check persistence. A normal visual five-heart display is not enough to confirm exact values.
5. Check errors and raw values:

```powershell
$log = Join-Path $gameDir 'MelonLoader\Latest.log'
Select-String -Path $log -Pattern '0.2.4-test|CAP NORMALIZED|CAP LIMITED|DECAY BLOCKED|RAW READ|NUMERIC PROTECTED|Exception|Error' -Context 0,1
```

If there is another crash, stop testing; do not overwrite the main save. Keep the most recent MelonLoader logs and the game's Player.log, note which action preceded the crash and whether it also happens with the mod removed. With the game closed, restore the backed-up DLL if desired, or move the DLL out of `Mods` to disable it. Reverting the DLL does **not** automatically repair out-of-range values already written into a save. Avoid committing private game logs, saves, or proprietary game binaries to this public repository.
