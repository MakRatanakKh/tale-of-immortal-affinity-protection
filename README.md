# Tale of Immortal — Partner Affinity Protection

**Status (September 20, 2026): standalone initialization and Harmony patch installation CONFIRMED; gameplay behavior NOT yet verified.** The user built `tools/AffinityStandalone/AffinityStandalone.csproj` successfully (`net6.0`, five nullable-reference warnings), installed `AffinityProtectionStandalone.dll` in `<GameDir>/Mods`, launched the game, and provided `MelonLoader/Latest.log` showing both affinity method patches installed and removed cleanly at shutdown. This verifies that the current standalone MelonMod loads and Harmony reports installation; it does **not** prove that affinity is protected under actual game events.

Goal: stop decreases in affinity in either direction between the player and their current spouse or cultivation partners (道侣), while permitting increases and leaving unrelated NPC relations unchanged.

## Confirmed standalone runtime milestone

The September 20 log displayed:

```text
AffinityProtectionStandalone: OnInitializeMelon ENTERED; initializing shared affinity patches.
AffinityProtection: Init ENTERED (six-character-ID diagnostic build 0.1.3)
AffinityProtection: patched Il2Cpp.DataUnit+RelationData.AddIntim(String, Single, Int32, String, Boolean)
AffinityProtection: patched Il2Cpp.DataUnit+RelationData.SetIntim(String, Single)
AffinityProtection: startup patch installation complete; confirm behavior using a backup save.
AffinityProtection: removed patches.
AffinityProtectionStandalone: deinitialized.
```

The `Init` message still says `0.1.3` because the standalone project reuses the older shared source; the standalone assembly identifies itself as `0.2.0-test`. No need to rebuild merely to change that diagnostic wording. The separate loader probe also successfully reached `OnInitializeMelon` on MelonLoader 0.7.3.

## Next: behavior testing on a BACKUP save

Keep the game's previous experimental Partner Affinity Protection *Local Mod* disabled/removed, so only `AffinityProtectionStandalone.dll` installs affinity patches. Do not disturb unrelated mods or downgrade MelonLoader. The loader probe can remain installed; it does not patch affinity.

1. Before triggering changes, record both directions of affinity for a current spouse or cultivation partner, ideally using numerical values rather than hearts.
2. Trigger a specific game event that normally decreases that partner's affinity (for example the relevant seasonal/yearly transition, only if reproducible). Record both directions afterward.
3. Check `MelonLoader/Latest.log` for `AffinityProtection: blocked negative AddIntim`, `AffinityProtection: clamped decreasing SetIntim`, and `relation lookup failed` messages. A blocked-event line plus unchanged values is stronger evidence than either alone; absence of a blocked-event line can mean the tested event did not use these writers.
4. Separately verify positive affinity gains and unrelated NPC decreases still work. Check breakup/reset behavior only with an expendable backup copy, not a primary save. Avoid overwriting the primary save during tests.
5. Report exact before/after values or screenshots and relevant log lines in chat, **not in this public repository**.

Useful log command from PowerShell (after launching/loading a backup save, triggering an event, and exiting):

```powershell
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
Select-String -Path (Join-Path $gameDir 'MelonLoader\Latest.log') -Pattern 'AffinityProtection:|AffinityProtectionStandalone:|Exception|Error' -Context 1,1
```

## Structure and known limitations

- [`tools/AffinityStandalone/`](tools/AffinityStandalone/): confirmed-to-initialize `net6.0` MelonMod, installed in `<GameDir>/Mods`. [Standalone test guide](docs/standalone-test.md).
- [`ModCode/ModMain/ModMain.cs`](ModCode/ModMain/ModMain.cs): shared Harmony code targeting `DataUnit.RelationData.AddIntim(string,float,int,string,bool)` and `SetIntim(string,float)` for current `Married`/`Lover` player relationships.
- `ModCode/ModMain/`: original game's Local Mods packaging, compiled and recognized but C# `Init` unverified; do not enable concurrently with standalone.
- [`tools/LoaderProbe/`](tools/LoaderProbe/): minimal standalone MelonLoader probe, verified to run.

**Known limitations:** `SetIntim` compares a float with integer `GetIntim`, so fractional affinity losses might slip through; additional writers (including `ClearIntim`) and breakup handling are not fully tested. Even though the two Harmony patches report installation, the affinity filter and gameplay behavior are **still experimental**. [Assembly findings](docs/inventory-results.md) and [verification checklist](docs/verification.md) contain further details.

Never commit game DLLs, saves, local configuration paths or private logs to this public repository.
