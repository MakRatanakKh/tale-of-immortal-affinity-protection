# Tale of Immortal — Partner Affinity Protection

**Status (September 20, 2026): v0.2.4, tested for personal gameplay.** The standalone MelonLoader mod has passed 13 cap-policy checks and several user gameplay scenarios: loading and patch installation, blocking partner affinity losses, allowing gains, leaving unrelated NPC affinity changes intact in an observed test, respecting the 300 affinity cap, and saving/reloading after partner interactions and month skipping. This does not prove all gameplay events or both affinity directions are covered, nor that the earlier reported crashes were caused by the previous over-cap bug. Keep save backups.

Goal: prevent decreases in affinity in either direction between the player and their current spouse or cultivation partners (道侣), while permitting increases and leaving unrelated NPC relations unchanged.

## Installation and build

Use the **standalone MelonLoader** mod, not the earlier in-game Local Mods package. The old Local Mods affinity package should remain disabled/removed so the Harmony patches are not installed twice.

1. Copy `ModCode/Local.props.example` to `ModCode/Local.props` and configure `GameDir` for your installation (this local file is not committed).
2. Close the game and back up your saves and currently installed mod DLL.
3. Run `dotnet run --project ./tools/AffinityCapTests/AffinityCapTests.csproj -c Release` and `dotnet build ./tools/AffinityStandalone/AffinityStandalone.csproj -c Release`.
4. Copy `tools/AffinityStandalone/bin/Release/net6.0/AffinityProtectionStandalone.dll` into `<GameDir>/Mods/` and restart the game.

The optional `AffinityLoaderProbe.dll` was used to diagnose MelonLoader startup; it is not required for gameplay and can be removed from the `Mods` folder while the game is closed.

## What v0.2.4 does

- Harmony-patches `DataUnit.RelationData.AddIntim` and `SetIntim` for current player `Married`/`Lover` relationships.
- Blocks negative deltas for protected relationships, and prevents below-current absolute writes using raw floating-point affinity when a plausible validated read is available (integer fallback otherwise).
- Limits protected affinity to 300 and allows existing above-cap affinity to normalize to 300. Preserves fractional values below the cap when validated raw affinity is available.
- Leaves unrelated NPC relationships and `ClearIntim` unpatched so the game's relationship-removal behavior remains game-controlled.
- Logs `DECAY BLOCKED`, `CAP NORMALIZED`, and `CAP LIMITED` in MelonLoader's `Latest.log` for diagnosis.

## Verification and limitations

- The user's v0.2.4 tests confirmed raw affinity normalizing from 302 to 300, attempted decreases to 296.25 and 299 being blocked at 300, and additional above-cap requests being limited to 300 in the **NPC → player** direction. A session including attacking a partner, gifting a manual, skipping several months, closing the game, and reloading the save completed without a reported crash.
- The opposite **player → NPC** direction, all NPC/event combinations, and longer-term stability have not been exhaustively tested. The raw-field plausibility check is not a guarantee for every game version, and the fallback getter may lose fractional precision. A previous game crash was reported during max-affinity interactions, but its root cause is unconfirmed.
- If the game crashes, preserve `MelonLoader/Latest.log` and the game's `Player.log` before restarting. Do not post private saves or logs to this public repository. Keep backup saves when updating the game or the mod.

Additional records: [300-cap investigation](docs/affinity-300-cap-investigation.md), [fractional precision test](docs/fractional-precision-test.md), and [assembly findings](docs/assembly-findings.md).

Do not commit game DLLs, private saves, local configuration paths, or private logs.
