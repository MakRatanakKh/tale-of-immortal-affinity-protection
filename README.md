# Tale of Immortal — Partner Affinity Protection

**Status (September 19, 2026): experimental prototype. Windows build and `toi mod pack` succeeded; the user imported the package and showed `Partner Affinity Protection` in the game's Local Mods list with `Load into the game?` checked. Runtime entry-point execution, Harmony patch installation and affinity behavior are NOT yet verified.** Back up your save before testing.

Goal: prevent affinity losses in either direction between the player and their current spouse or cultivation partner (道侣), while allowing gains and leaving unrelated NPC relationships unaffected.

## What is implemented

The user's assembly inventory identifies `DataUnit.RelationData.AddIntim(string,float,int,string,bool)` and `SetIntim(string,float)` as candidate writers. [`ModCode/ModMain/ModMain.cs`](ModCode/ModMain/ModMain.cs) contains two narrowly scoped Harmony prefixes for those overloads, with current `Married`/`Lover` checks and startup/failure logs. This is an in-game Tale of Immortal mod project using `ModMain.Init()` and `Destroy()`, **not** a standalone MelonMod.

**Limitations:** successfully building, packaging and appearing checked in Local Mods do not prove that `Init` ran or that Harmony hooks installed. The direct-set guard reads affinity with an integer-returning getter, so sub-integer decreases may slip through. `ClearIntim` remains unpatched until breakup behavior is understood. Other affinity writers may bypass the hooks.

## Build and package on Windows (confirmed)

Configure `ModCode/Local.props` *once* by copying its `.example` file and setting `GameDir` to the game directory containing `MelonLoader`. If already configured, do not overwrite it. From PowerShell at the repository root:

```powershell
git pull origin main
dotnet build .\ModCode\ModMain\ModMain.csproj -c Release
toi mod pack . -o "$env:USERPROFILE\Desktop\AffinityProtection-test" --clean --glob '.git/'
```

The corrected pack command succeeds on the user's installation. The output directory is `%USERPROFILE%\Desktop\Mod_AffinityProtection_AffinityProtection-test` because the packer adds its `Mod_...` prefix. The package includes `ModExportData.cache` and `ModCode\dll\MOD_AffinityProtection.dll`; it does not contain Git metadata. Some development docs/tools are also included, and can be cleaned up in a future packaging revision.

## Next: check patch logs, then test a backup save

The mod is visible and checked in the Local Mods menu. Fully exit the game and relaunch with it enabled; load a **backup save** if needed to trigger the in-game `ModMain.Init` callback. Inspect recent `Player.log`, the MelonLoader log, or other game logs for `AffinityProtection:`. We need **both** `patched ... AddIntim(...)` and `patched ... SetIntim(...)` startup lines and no `patch installation FAILED` / `relation lookup failed` errors before assuming hooks are active. If neither startup line appears, confirm the log is from the current launch and inspect surrounding mod-loader errors. Only then test affinity changes using a backup save.

You can search for recent log files in the game installation and `%USERPROFILE%\AppData\LocalLow` with PowerShell. Do not commit logs, private local paths, proprietary game DLLs, or saves to this public repository.

See [`docs/build-and-test.md`](docs/build-and-test.md) for packaging, installation, logging and safe in-game test steps. Do not put the DLL in the generic `MelonLoader/Mods` directory.

## Project resources

- [`docs/build-and-test.md`](docs/build-and-test.md): packaging, installation, logging and test procedure.
- [`docs/inventory-results.md`](docs/inventory-results.md): signatures/enum values extracted from the assembly inventory.
- [`docs/assembly-findings.md`](docs/assembly-findings.md): first-pass candidate names.
- [`tools/AffinityInspector/`](tools/AffinityInspector/): offline metadata inventory helper.
- [`docs/verification.md`](docs/verification.md): behavior acceptance checks.

Never commit proprietary game DLLs, saves, local configuration paths or other private data to this public repository.
