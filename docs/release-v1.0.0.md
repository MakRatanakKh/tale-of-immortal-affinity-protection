# Affinity Protection Mods v1.0.0

First combined release of the Tale of Immortal affinity-protection mods.

## Included mods

### Partner Affinity Protection v0.2.4

`AffinityProtectionStandalone.dll`

Protects affinity between the player and their current spouse/cultivation partners.

Verified behavior includes:

- blocking tested affinity-loss writes;
- allowing affinity gains;
- preserving validated fractional affinity;
- respecting the normal 300 affinity cap;
- allowing over-cap values to normalize to 300;
- leaving unrelated NPC affinity changes intact in observed tests;
- surviving tested partner interactions, month skipping, save close, and reload.

### Master Affinity Protection v0.1.0

`MasterAffinityProtection.dll`

Protects affinity between the player and their current Master.

Verified behavior includes:

- recognizing `UnitRelationType.Master` at runtime;
- blocking tested Master -> player affinity-loss writes;
- preserving raw fractional affinity (`279.81` remained `279.81` during tested attacks);
- coexisting with Partner Affinity Protection.

The reverse player -> Master direction uses the same protection strategy but has not yet been directly exercised in gameplay.

## Tested environment

- Tale of Immortal / 鬼谷八荒 — Steam App ID `1468810`
- Steam public branch Build ID `21758240`
- `guigubahuang.exe` Product/File Version `2020.3.9.15689012`
- MelonLoader `0.7.3 Open-Beta`
- Windows

## Installation

1. Install MelonLoader.
2. Close Tale of Immortal.
3. Back up your save.
4. Place either or both DLLs in the game's `Mods` folder.
5. Start the game.

The two mods can be used together.

Do not install the old diagnostic `AffinityLoaderProbe.dll` for normal gameplay. If an older Local Mods affinity package was created during development, leave it disabled or remove it so the protection logic is not installed twice.

## Notes

These mods patch internal game relationship methods. Future Tale of Immortal updates may require compatibility changes. Keep save backups, especially after game or mod updates.
