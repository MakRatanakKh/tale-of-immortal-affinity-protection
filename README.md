# Tale of Immortal — Partner Affinity Protection

**Status (September 19, 2026): experimental prototype.** Windows build and `toi mod pack` succeeded. The game shows `Partner Affinity Protection` in Local Mods with its checkbox checked. The installed DLL exists and `Player.log` shows the package's `ModExportData.cache` loading. **C# entry-point execution, Harmony patch installation and actual affinity protection have NOT been verified.** Back up your save before any gameplay testing.

Goal: prevent affinity losses in either direction between the player and their current spouse or cultivation partner (道侣), while allowing gains and leaving unrelated NPC relationships unaffected.

## Implementation and current blocker

The user's assembly inventory identified `DataUnit.RelationData.AddIntim(string,float,int,string,bool)` and `SetIntim(string,float)` as candidate affinity writers. [`ModCode/ModMain/ModMain.cs`](ModCode/ModMain/ModMain.cs) contains two narrowly scoped Harmony prefixes for those overloads and `Married`/`Lover` checks. This is an **in-game Tale of Immortal mod**, not a standalone MelonMod.

The full `Latest.log` shows `0 Mods loaded` under standalone MelonLoader, which does **not** establish that the game's separate local-mod code loader failed. `Player.log` contains the package-loading line but neither diagnostic messages nor clear exceptions from this mod. The original C# code emitted its first success log only *after* reflection and patching, leaving early failures invisible. A newly committed diagnostic revision logs `Init ENTERED` to both Unity and MelonLoader and catches initialization exceptions around the whole process. **This revision still needs compiling, deploying, and testing on the user's machine.**

Follow the exact **diagnostic DLL deployment and log commands** in [`docs/build-and-test.md`](docs/build-and-test.md). They update only the DLL inside the *existing in-game local mod folder*, never the generic `MelonLoader/Mods` directory. If the `Init ENTERED` line appears, the next logs show whether patch resolution/installation succeeds; if it never appears, investigate how the game loads and invokes mod C# rather than assuming the Harmony method name is wrong.

**Known limitations:** the `SetIntim` guard compares a `float` with integer `GetIntim()`, so sub-integer decreases may evade it. `ClearIntim` and other writers might bypass the two hooks; breakups can happen independently of affinity. No in-game behavior claims until tested on a backup save.

## Project resources

- [`docs/build-and-test.md`](docs/build-and-test.md): compilation, packaging, local-mod deployment, exact log locations and regression test checklist.
- [`docs/inventory-results.md`](docs/inventory-results.md): candidate method signatures and enum values from the game assembly inventory.
- [`docs/assembly-findings.md`](docs/assembly-findings.md): first-pass method discovery.
- [`tools/AffinityInspector/`](tools/AffinityInspector/): offline metadata inventory helper.
- [`docs/verification.md`](docs/verification.md): acceptance criteria.

Do not commit proprietary game DLLs, saves, local configuration paths or other private data to this public repository.
