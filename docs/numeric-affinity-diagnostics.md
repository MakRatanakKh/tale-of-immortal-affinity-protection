# Numeric affinity diagnostics (experimental, 0.2.1-test)

The game's UI uses hearts and does not show a reliable numerical before/after readout. The standalone mod can now log an integer value returned by `DataUnit.RelationData.GetIntim(unitID)` for **player-related affinity writes**. This is not the raw `float` in storage: fractional changes can be invisible. The previously reported 380 in the Marked screen is a focus cost, **not** an affinity measurement.

## What the log shows

The shared patch logs `AffinityProtection: NUMERIC` entries to MelonLoader's console, `MelonLoader/Latest.log`, and the existing `%TEMP%/AffinityProtection-diagnostic.log`. Each line includes the writer (`AddIntim` or `SetIntim`), BEFORE or AFTER, the direction (`player->NPC` or `NPC->player`), the NPC's `unitID` (not a display name), and `GetIntim(int)`. BEFORE entries show the requested delta or absolute value. A BLOCKED or CLAMPED entry appears where applicable. After entries read the game's integer getter again; entries may be nested if AddIntim internally invokes SetIntim. The counter is capped at 120 numerical lines per session to avoid log spam.

The game code and MelonLoader APIs have not been compiled or exercised for **this change**; the previous standalone 0.2.0-test was confirmed to compile and install two Harmony patches. Test the new build with a backup save and do not treat diagnostic output as complete protection from every possible affinity change. This change does not adjust the preexisting protection conditions or fix its fractional `SetIntim` comparison.

## Updating ONLY the standalone build (Windows PowerShell)

Close Tale of Immortal. From your local repository:

```powershell
git pull origin main
dotnet build .\tools\AffinityStandalone\AffinityStandalone.csproj -c Release
```

Proceed only if the build succeeds. The game's experimental Local Mods version must remain removed/disabled; install only the standalone DLL into MelonLoader's `Mods` directory:

```powershell
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
$source = '.\tools\AffinityStandalone\bin\Release\net6.0\AffinityProtectionStandalone.dll'
$destination = Join-Path $gameDir 'Mods\AffinityProtectionStandalone.dll'
Copy-Item -LiteralPath $source -Destination $destination -Force
```

Start the game, load a **backup save**, perform a single affinity-changing interaction, then inspect the file:

```powershell
Select-String -Path (Join-Path $gameDir 'MelonLoader\Latest.log') -Pattern 'AffinityProtection: NUMERIC|AffinityProtection: ERROR|patch installation' -Context 0,0
```

To watch the log update live in a second PowerShell window while the game is open:

```powershell
Get-Content (Join-Path $gameDir 'MelonLoader\Latest.log') -Tail 20 -Wait |
    Select-String 'AffinityProtection: NUMERIC|AffinityProtection: ERROR'
```

`Get-Content -Wait` is a continuous watch; press Ctrl+C to stop. If the MelonLoader console is hidden, `Latest.log` can still be examined afterward.

## Interpretation and limits

- `GetIntim(int)=...` is the numeric result of the game's **integer getter** for that **specific direction**. It cannot show fractions or automatically list every partner; it logs only when `AddIntim` or `SetIntim` is called for a player-related pair.
- A paired BEFORE/AFTER readout is stronger evidence than a heart-only screenshot, but other code can modify relations via writers not patched here, and some events use hate rather than affinity.
- The log identifies an NPC with `npcId`, not a name. Match the interaction timing and ID to your in-game test; do not publish raw saves or log files in this public repository.
- If the new build fails, do not replace the known-working DLL. If the game behaves unexpectedly, close it and restore the previous standalone DLL from your own backup.
