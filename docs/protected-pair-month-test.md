# Focused protected-partner month test (experimental 0.2.2)

The prior month-skip test filled the 120-line numeric trace with many unrelated NPC writes. The numeric logger now only writes `NUMERIC PROTECTED` lines for relationships that `IsProtectedPair` identifies as current spouse or cultivation partner. The limit is 240 numeric lines per session. This is a diagnostic-only change; the AddIntim/SetIntim protection decisions remain as before.

## Procedure (backup save)

1. Fully exit Tale of Immortal. Keep the earlier in-game Local Mod disabled/removed; do not run two affinity patch providers together. Back up your currently working `Mods/AffinityProtectionStandalone.dll` and your save.
2. Pull `main` and build `tools/AffinityStandalone/AffinityStandalone.csproj -c Release`. Confirm a successful build before copying the new DLL into `<GameDir>/Mods` while the game is shut down.
3. Start the game and load the backup save. Capture the baseline hearts for a partner, in **both** `Your Affinity` and `Target's Affinity` views if possible.
4. Advance one month at a time, noting the months. Capture the relationship UI again and exit the game.
5. Filter the latest MelonLoader log for `AffinityProtection: NUMERIC PROTECTED`, `blocked negative AddIntim`, `clamped decreasing SetIntim`, `relation lookup failed`, `numeric affinity readout failed`, `trace limit reached`, `Exception` and `Error`.
6. Report the relevant log lines and whether either direction's hearts changed in private chat; do not commit private logs, screenshots, or saves to this public repository.

Example PowerShell in the repository root:

```powershell
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
$log = Join-Path $gameDir 'MelonLoader\Latest.log'
Select-String -Path $log -Pattern 'AffinityProtection: NUMERIC PROTECTED|blocked negative AddIntim|clamped decreasing SetIntim|relation lookup failed|numerical affinity readout failed|trace limit reached|Exception|Error'
```

## Interpretation and limitations

- A `NUMERIC PROTECTED ... AddIntim BLOCKED` or `SetIntim CLAMPED` line demonstrates interception of a writer the code classified as an active partner, not proof of every possible affinity mutation.
- `GetIntim(int)` is only the integer getter, **not the exact stored float**; fractional changes and other write methods remain unverified.
- Logs contain NPC **IDs**, not verified NPC names. Match names only after establishing an ID/name mapping. Do not guess that a particular ID belongs to the partner visible in a screenshot.
- These are event-driven writer logs, not complete read-only snapshots of all relationships on each month boundary. No log line for a month does not establish that no change occurred.
- When the quota is exhausted, the numeric lines stop, but the existing protection behavior and occasional block counters continue.
- The new build has been committed but not yet compiled/tested on the user's game. If its build fails, keep the backed-up working DLL and send the compiler output.
