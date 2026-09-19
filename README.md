# Tale of Immortal — Partner Affinity Protection

**Status: experimental prototype. The user confirmed that it compiles on Windows (September 19, 2026); packaging, loading and actual affinity protection have NOT been verified.** Back up your save before testing.

Goal: prevent affinity losses in either direction between the player and their current spouse or cultivation partner (道侣), while allowing gains and leaving unrelated NPC relationships unaffected.

## What is implemented

The user's assembly inventory identifies `DataUnit.RelationData.AddIntim(string,float,int,string,bool)` and `SetIntim(string,float)` as candidate writers. [`ModCode/ModMain/ModMain.cs`](ModCode/ModMain/ModMain.cs) contains two narrowly scoped Harmony prefixes for those overloads, with current `Married`/`Lover` checks and startup/failure logs. This is an in-game Tale of Immortal mod project using `ModMain.Init()` and `Destroy()`, **not** a standalone MelonMod.

**Limitations:** compilation does not establish which paths the game executes or whether Harmony hooks install. The direct-set guard reads affinity with an integer-returning getter, so sub-integer decreases may slip through. `ClearIntim` remains unpatched until breakup behavior is understood. Other affinity writers may bypass the hooks.

## Build and package on Windows

Configure `ModCode/Local.props` *once* by copying its `.example` file and setting `GameDir` to the game directory containing `MelonLoader`. If you already configured it, **do not overwrite it**. From PowerShell at the repository root:

```powershell
git pull origin main
dotnet build .\ModCode\ModMain\ModMain.csproj -c Release
toi mod pack . -o "$env:USERPROFILE\Desktop\AffinityProtection-test" --clean --glob '.git/'
```

The first packaging attempt failed because the tool tried to copy `.git/config` into a destination `.git` folder that had not been created. `.gitignore` now excludes `.git/` and `--glob '.git/'` explicitly asks the packer to ignore it as well. **This corrected command still needs a user-side test.** The tool formats the resulting Desktop folder as `Mod_AffinityProtection_AffinityProtection-test`, not just `AffinityProtection-test`.

See [`docs/build-and-test.md`](docs/build-and-test.md) for exact package checks and backup-save runtime testing. If packaging fails, share the console output before installing. Do not copy the raw DLL into MelonLoader's generic Mods folder.

## Project resources

- [`docs/build-and-test.md`](docs/build-and-test.md): packaging, log and safe test procedure.
- [`docs/inventory-results.md`](docs/inventory-results.md): signatures/enum values extracted from the assembly inventory.
- [`docs/assembly-findings.md`](docs/assembly-findings.md): first-pass candidate names.
- [`tools/AffinityInspector/`](tools/AffinityInspector/): offline metadata inventory helper.
- [`docs/verification.md`](docs/verification.md): behavior acceptance checks.

Never commit proprietary game DLLs, saves, local configuration paths or other private data to this public repository.
