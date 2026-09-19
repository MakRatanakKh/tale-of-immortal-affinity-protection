# Tale of Immortal — Partner Affinity Protection

**Status (September 19, 2026): experimental prototype; the user confirmed a successful Windows build and `toi mod pack`, with `ModExportData.cache` and `ModCode/dll/MOD_AffinityProtection.dll` present. NOT yet loaded or tested in-game.** Back up your save before testing.

Goal: prevent affinity losses in either direction between the player and their current spouse or cultivation partner (道侣), while allowing gains and leaving unrelated NPC relationships unaffected.

## What is implemented

The user's assembly inventory identifies `DataUnit.RelationData.AddIntim(string,float,int,string,bool)` and `SetIntim(string,float)` as candidate writers. [`ModCode/ModMain/ModMain.cs`](ModCode/ModMain/ModMain.cs) contains two narrowly scoped Harmony prefixes for those overloads, with current `Married`/`Lover` checks and startup/failure logs. This is an in-game Tale of Immortal mod project using `ModMain.Init()` and `Destroy()`, **not** a standalone MelonMod.

**Limitations:** successful build/pack does not show that the game executes these methods or Harmony hooks install. The direct-set guard reads affinity with an integer-returning getter, so sub-integer decreases may slip through. `ClearIntim` remains unpatched until breakup behavior is understood. Other affinity writers may bypass the hooks.

## Build and package on Windows (confirmed)

Configure `ModCode/Local.props` *once* by copying its `.example` file and setting `GameDir` to the game directory containing `MelonLoader`. If already configured, do not overwrite it. From PowerShell at the repository root:

```powershell
git pull origin main
dotnet build .\ModCode\ModMain\ModMain.csproj -c Release
toi mod pack . -o "$env:USERPROFILE\Desktop\AffinityProtection-test" --clean --glob '.git/'
```

The corrected pack command succeeds on the user's installation. The output directory is `%USERPROFILE%\Desktop\Mod_AffinityProtection_AffinityProtection-test` because the packer adds its `Mod_...` prefix. The package includes `ModExportData.cache` and `ModCode\dll\MOD_AffinityProtection.dll`; it does not contain Git metadata. Some development docs/tools are also included, and can be cleaned up in a future packaging revision.

## Next: install and check the logs

Copy **the entire packed folder**, not the repository root or an individual DLL, into the game's `<GameDir>/ModExportData/` directory, or use the game's `Mod > Local Mods > Import mod data` command and select the packed folder. Enable `Partner Affinity Protection` in the in-game Local Mods menu. Check startup logs for `AffinityProtection:` patch-installed lines **before** testing affinity on a backed-up save. Do not put the DLL in the generic `MelonLoader/Mods` directory.

See [`docs/build-and-test.md`](docs/build-and-test.md) for install instructions, package checks, log discovery, and safe in-game test steps.

## Project resources

- [`docs/build-and-test.md`](docs/build-and-test.md): packaging, installation, logging and test procedure.
- [`docs/inventory-results.md`](docs/inventory-results.md): signatures/enum values extracted from the assembly inventory.
- [`docs/assembly-findings.md`](docs/assembly-findings.md): first-pass candidate names.
- [`tools/AffinityInspector/`](tools/AffinityInspector/): offline metadata inventory helper.
- [`docs/verification.md`](docs/verification.md): behavior acceptance checks.

Never commit proprietary game DLLs, saves, local configuration paths or other private data to this public repository.
