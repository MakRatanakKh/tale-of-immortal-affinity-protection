# Fractional affinity precision — 0.2.3 experimental test

**Status:** Source update committed; not compiled or runtime-tested on the user's machine. Keep the 0.2.2 DLL backup until verification completes.

## What changed

- The existing negative `AddIntim` cancellation and active `Married`/`Lover` filter are unchanged.
- A protected `SetIntim` write attempts to read the pre-write floating-point affinity from `RelationData.intimToPlayerUnit` for NPC → player and `RelationData.intimToUnit[otherId]` for player → NPC.
- The candidate float must be finite and within 1.01 points of the game's integer `GetIntim(otherId)` result before it may be used to clamp. This is an **initial plausibility check, not definitive validation** of the storage mapping.
- On absent, inconsistent, or failing raw reads, the mod logs `RAW READ`/`fractional guard unavailable` and uses the earlier integer clamp for that relation. A fallback means fractional precision is **not** protected reliably for that relation.
- `NUMERIC PROTECTED` lines now include `raw=<float>` and `rawSource=<source>` (or `raw=unverified`). Log volume remains limited to 240 numerical lines/session.
- `ClearIntim` remains unpatched. Breakups, alternative writers, save/reload, and every time transition remain unverified.

## Build + install (game CLOSED; use a backup save)

From the repository root in PowerShell:

```powershell
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
$installed = Join-Path $gameDir 'Mods\AffinityProtectionStandalone.dll'
Copy-Item $installed "$env:USERPROFILE\Desktop\AffinityProtectionStandalone-0.2.2-working.dll" -Force
git pull origin main
dotnet build .\tools\AffinityStandalone\AffinityStandalone.csproj -c Release
```

**Only if compilation succeeds**, with the game fully closed:

```powershell
Copy-Item '.\tools\AffinityStandalone\bin\Release\net6.0\AffinityProtectionStandalone.dll' $installed -Force
```

Start the game on a **disposable backup save** and trigger a protected positive interaction followed by a negative one, if possible. Exit. Inspect:

```powershell
$log = Join-Path $gameDir 'MelonLoader\Latest.log'
Select-String -Path $log -Pattern 'fractional guard|RAW READ|NUMERIC PROTECTED|blocked negative AddIntim|clamped decreasing SetIntim|Exception|Error'
```

## What to look for

- `RAW READ validated:` at least once plus `rawSource=intimToPlayerUnit` (NPC → player) or `rawSource=intimToUnit` (player → NPC) in related trace lines.
- For an attempted decrease when the exact stored raw value is fractional (e.g., 300.5), `SetIntim CLAMPED ... passedAbsolute=300.5; guard=intimToPlayerUnit` followed by `AFTER ... raw=300.5`. Numeric getter may still round this to 300 or 301; that alone is not failure.
- A normal increase should pass, even a fractional one. Unrelated NPCs must still be able to lose affinity.
- If `RAW READ` fails, the raw value differs significantly from `GetIntim(int)`, patches fail to install, or the game crashes, stop the test. Restore the desktop backup DLL with the game closed, then provide build output / logs. Do not publish game binaries, private saves, or raw logs in the public repository.

**Do not label this fix verified just because it compiles or because integer hearts remain stable.** Confirm that the attempted fractional decrease was clamped to the same raw value and that the subsequent raw value did not decline. The mapped storage fields' runtime semantics are still provisional.
