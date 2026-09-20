# Tale of Immortal — Partner Affinity Protection

**Status (September 20, 2026): first gameplay affinity-loss interception observed; still experimental.** The standalone MelonLoader mod compiles, loads, installs both Harmony patches, and logs affinity-loss interception. During a session in which the user attacked their partner Ning Yue, they reported no visible drop in affinity. This is strong evidence the patch works for this scenario, **not** proof that every affinity change, both affinity directions, or every partner/event is protected. Only the on-screen heart display was reported; exact numeric before/after values and individual NPC IDs were not recorded in the interception log.

Goal: prevent decreases in affinity in either direction between the player and their current spouse or cultivation partners (道侣), while permitting increases and leaving unrelated NPC relations unchanged.

## Confirmed standalone runtime and first gameplay test

The user successfully built `tools/AffinityStandalone/AffinityStandalone.csproj` (net6.0, five nullable-reference warnings), installed `AffinityProtectionStandalone.dll` into `<GameDir>/Mods`, and confirmed it loads through MelonLoader 0.7.3. The old game's Local Mods package is removed/disabled. The loader probe also successfully executed its own `OnInitializeMelon` callback.

The September 20 test session's MelonLoader log showed the following sequence (condensed from the user's screenshot):

```text
AffinityProtectionStandalone: OnInitializeMelon ENTERED; initializing shared affinity patches.
AffinityProtection: Init ENTERED (six-character-ID diagnostic build 0.1.3)
AffinityProtection: patched Il2Cpp.DataUnit+RelationData.AddIntim(String, Single, Int32, String, Boolean)
AffinityProtection: patched Il2Cpp.DataUnit+RelationData.SetIntim(String, Single)
AffinityProtection: startup patch installation complete; confirm behavior using a backup save.
AffinityProtection: blocked negative AddIntim (total 1)
AffinityProtection: clamped decreasing SetIntim (total 1)
AffinityProtection: clamped decreasing SetIntim (total 2)
AffinityProtection: removed patches.
AffinityProtectionStandalone: deinitialized.
```

User-reported gameplay context: attacked current partner **Ning Yue** and did not see an affinity drop. The log demonstrates a blocked negative delta and two clamped setter writes **during that session**, but does not include character IDs or numerical affinity values to tie each specific message to Ning Yue or show which direction of affinity was affected. This is the first meaningful gameplay confirmation, not full regression validation.

The `Init` log message says `0.1.3` because the standalone assembly (`0.2.0-test`) reuses the prior shared patch implementation. No rebuild is required to change that diagnostic wording.

## Further tests on a BACKUP save

Leave the earlier experimental Partner Affinity Protection *Local Mod* disabled/removed; keep only `AffinityProtectionStandalone.dll` as the affinity patch provider. The separate loader probe is harmless and can be removed later. Avoid overwriting the primary save while deliberately triggering negative events.

1. Record numerical affinity before and after for **both directions**: `Your Affinity` and `Target's Affinity` in the relationship UI, if a numerical display is available. Hearts may conceal small changes.
2. Repeat with a second partner and a different negative event, especially time/season/year rollover, and check the log for new `blocked negative AddIntim`/`clamped decreasing SetIntim` entries. The current log does not name NPC IDs.
3. Check that affinity **gains** from a friendly interaction or gift still work.
4. Check that an **unrelated NPC's** affinity can still decrease (on a disposable save).
5. Test relationship removal/breakup only on an expendable backup copy; `ClearIntim` is intentionally not patched. Verify save/reload if the above tests pass.

Useful log command:

```powershell
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
Select-String -Path (Join-Path $gameDir 'MelonLoader\Latest.log') -Pattern 'AffinityProtection:|AffinityProtectionStandalone:|Exception|Error' -Context 1,1
```

Report exact values or screenshots and the relevant log lines in private chat, **not this public repository**.

## Structure and known limitations

- [`tools/AffinityStandalone/`](tools/AffinityStandalone/): standalone net6.0 MelonMod for `<GameDir>/Mods`. [Installation/test guide](docs/standalone-test.md).
- [`ModCode/ModMain/ModMain.cs`](ModCode/ModMain/ModMain.cs): shared Harmony implementation targeting `DataUnit.RelationData.AddIntim(string,float,int,string,bool)` and `SetIntim(string,float)` for current `Married`/`Lover` player relationships.
- `ModCode/ModMain/`: earlier game's Local Mods package; recognized by game but C# `Init` unverified. Do not load alongside standalone.
- [`tools/LoaderProbe/`](tools/LoaderProbe/): minimal probe confirmed to run via MelonLoader.

**Known limitations:** `SetIntim` compares a float against integer `GetIntim`, so fractional decreases may slip through. Other writers, including `ClearIntim`, and breakup behavior have not been validated. The log messages do not identify NPCs. This is an experimental mod, not a guarantee against every possible affinity decrease. [Assembly findings](docs/inventory-results.md) and [verification checklist](docs/verification.md) explain the underlying discovery work.

Do not commit game DLLs, saves, local configuration paths or private logs to this public repository.
