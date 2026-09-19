# Tale of Immortal — Partner Affinity Protection

**Status: experimental source prototype committed; NOT compiled, packaged, or tested in-game.** Do not use on your only save.

Goal: prevent affinity losses in either direction between the player and their current spouse or cultivation partner (道侣), while allowing gains and leaving unrelated NPC relationships unaffected.

## What is implemented

The uploaded assembly metadata identifies `DataUnit.RelationData.AddIntim(string,float,int,string,bool)` and `SetIntim(string,float)` as candidate writers. [`ModCode/ModMain/ModMain.cs`](ModCode/ModMain/ModMain.cs) contains two narrowly scoped Harmony prefixes for those exact overloads, with current `Married`/`Lover` relationship checks, explicit startup patch logs, and failure logging. No generic `MelonMod` loader entry point is used: this is an in-game Tale of Immortal mod project with `ModMain.Init()` and `Destroy()`.

**Limitations:** metadata alone cannot prove which paths the game actually executes. The current direct-set guard reads affinity via an integer-returning getter, so sub-integer losses may slip through. `ClearIntim` is intentionally untouched until breakup semantics are understood; some losses or relationship changes may bypass the current patches. No claim of working game behavior yet.

## Your next step: build on Windows

From PowerShell in your existing local clone:

```powershell
git pull origin main
Copy-Item .\ModCode\Local.props.example .\ModCode\Local.props
notepad .\ModCode\Local.props
```

In Notepad, set `GameDir` to your Tale of Immortal installation directory (the folder containing `MelonLoader`) and save. Then run:

```powershell
dotnet build .\ModCode\ModMain\ModMain.csproj -c Release
```

If build fails, paste its error output into chat. If it succeeds, follow [`docs/build-and-test.md`](docs/build-and-test.md) for packaging, logs and testing using a backup save. The packaging instructions have not yet been verified on this machine.

## Project resources

- [`docs/inventory-results.md`](docs/inventory-results.md): signatures/enum values extracted from the user's own assembly inventory.
- [`docs/assembly-findings.md`](docs/assembly-findings.md): first-pass candidate names.
- [`tools/AffinityInspector/`](tools/AffinityInspector/): offline metadata inventory helper, already used for this iteration.
- [`docs/verification.md`](docs/verification.md): behavior acceptance checks.

No proprietary game DLLs, saves, local paths or other private data should be committed to this public repository.
