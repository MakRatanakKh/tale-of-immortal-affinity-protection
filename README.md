# Tale of Immortal — Partner Affinity Protection

**Status: experimental source prototype; compiled successfully on the user's Windows PC (September 19, 2026). NOT yet packaged, loaded or tested in-game.** Back up your save before trying it.

Goal: prevent affinity losses in either direction between the player and their current spouse or cultivation partner (道侣), while allowing gains and leaving unrelated NPC relationships unaffected.

## What is implemented

The user's assembly metadata identifies `DataUnit.RelationData.AddIntim(string,float,int,string,bool)` and `SetIntim(string,float)` as candidate writers. [`ModCode/ModMain/ModMain.cs`](ModCode/ModMain/ModMain.cs) contains two narrowly scoped Harmony prefixes for those overloads, with current `Married`/`Lover` relationship checks, explicit startup patch logs, and failure logging. This is an in-game Tale of Immortal mod project using `ModMain.Init()` and `Destroy()`, not a standalone MelonMod.

**Limitations:** compilation does not establish which paths the game actually executes. The direct-set guard reads affinity using an integer-returning getter, so sub-integer decreases may slip through. `ClearIntim` is untouched until breakup semantics are verified; relationship changes or affinity writes may bypass the current patches. No claim of working in-game behavior yet.

## Next: package and inspect output

The Windows build completed successfully with:

```powershell
dotnet build .\ModCode\ModMain\ModMain.csproj -c Release
```

From the repository root, run:

```powershell
toi mod pack . -o "$env:USERPROFILE\Desktop\AffinityProtection-test" --clean
```

**This packaging command has not yet been verified on the user's installation.** Send its console output and package folder structure before installing. Follow [`docs/build-and-test.md`](docs/build-and-test.md) for the remaining instructions and backup-save runtime checks. Do not copy the DLL directly into MelonLoader's generic Mods folder.

## Project resources

- [`docs/build-and-test.md`](docs/build-and-test.md): packaging, log and test procedure.
- [`docs/inventory-results.md`](docs/inventory-results.md): signatures/enum values extracted from the user's assembly inventory.
- [`docs/assembly-findings.md`](docs/assembly-findings.md): first-pass candidate names.
- [`tools/AffinityInspector/`](tools/AffinityInspector/): offline metadata inventory helper.
- [`docs/verification.md`](docs/verification.md): behavior acceptance checks.

No proprietary game DLLs, saves, local paths or other private data should be committed to this public repository.
