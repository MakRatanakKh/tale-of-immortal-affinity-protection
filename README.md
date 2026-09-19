# Tale of Immortal — Partner Affinity Protection

**Status (September 19, 2026): experimental; affinity protection NOT yet verified.** The game's Local Mods loader reads both our original and six-character-ID packages, but neither produced the expected C# `Init` log. The user installed a separate `net6.0` MelonLoader probe and confirmed `AffinityLoaderProbe: OnInitializeMelon reached` in MelonLoader 0.7.3's `Latest.log`. Thus, the standalone MelonLoader entry point is verified in this environment, whereas the game's Local Mods C# entry point remains unverified. This does not yet prove Harmony patches or affinity behavior work.

Goal: stop decreases in affinity in either direction between the player and their current spouse or cultivation partners (道侣), while permitting increases and leaving unrelated NPC relations unchanged.

## Current next experiment: standalone MelonMod

A new project at [`tools/AffinityStandalone/`](tools/AffinityStandalone/) reuses the exact affinity-patch source from [`ModCode/ModMain/ModMain.cs`](ModCode/ModMain/ModMain.cs) but exposes the `MelonMod.OnInitializeMelon` callback that the separate loader probe successfully exercised. It targets `net6.0` like that probe, and installs into `<GameDir>/Mods` **only after a successful Windows build**. Unlike the game's Local Mods package, this standalone DLL contains an assembly-level `MelonInfo` registration. **This new standalone project has NOT been compiled or tested on the user's machine yet.**

Follow the safety checks, build, isolated installation and rollback instructions in [`docs/standalone-test.md`](docs/standalone-test.md). Disable and move only the experimental Partner Affinity Protection Local Mods package out of `ModExportData` before installing the standalone DLL, so both implementations cannot hook the same methods at once. Do not downgrade MelonLoader or disturb unrelated mods.

## Previous experiments

- `ModCode/ModMain/` builds as `MOD_Rk7Qp2.dll` for the game's Local Mods loader; it compiles and packs, and the package-loading line appears in `Player.log`, but `Init` does not appear in logs or `%TEMP%/AffinityProtection-diagnostic.log`.
- `tools/LoaderProbe/` builds as `AffinityLoaderProbe.dll`; the user confirmed it loads in MelonLoader `Mods` and calls `OnInitializeMelon`.

The shared code attempts to patch `DataUnit.RelationData.AddIntim(string,float,int,string,bool)` and `SetIntim(string,float)` for current `Married`/`Lover` player relationships. The setter comparison uses integer `GetIntim`, potentially missing fractional changes. `ClearIntim`, other writers and breakup behavior remain unverified. Even a `patch installation complete` log is not proof that affinity is protected: test actual losses and gains only on a backup save.

Other resources: [`docs/inventory-results.md`](docs/inventory-results.md) lists candidate game signatures; [`docs/verification.md`](docs/verification.md) lists game-behavior tests; [`docs/six-character-id-test.md`](docs/six-character-id-test.md) documents the previous Local Mods ID experiment.

Never commit game DLLs, saves, local configuration paths or private logs to this public repository.
