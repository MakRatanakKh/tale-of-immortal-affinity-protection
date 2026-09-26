# Tale of Immortal — Affinity Protection Mods

MelonLoader mods for **Tale of Immortal / 鬼谷八荒** that prevent affinity loss for selected player relationships while still allowing affinity gains and respecting the game's normal affinity cap.

## Included mods

### Partner Affinity Protection v0.2.4

File:

```text
PartnerAffinityProtection.dll
```

Protects affinity between the player and current:

- spouse (`Married`);
- cultivation partners (`Lover` / 道侣).

### Master Affinity Protection v0.1.0

File:

```text
MasterAffinityProtection.dll
```

Protects affinity between the player and their current:

- Master (`UnitRelationType.Master`).

The two mods are independent and can be installed together.

## Tested environment

These versions were tested on:

- **Game:** Tale of Immortal / 鬼谷八荒, Steam App ID `1468810`
- **Steam public branch Build ID:** `21758240`
- **Executable:** `guigubahuang.exe`
- **Executable Product/File Version:** `2020.3.9.15689012` (Unity player/file version; not the Steam build number)
- **MelonLoader:** `0.7.3 Open-Beta`
- **Platform:** Windows

Other game or MelonLoader versions may work, but they have not been verified by this project.

## Installation

1. Install MelonLoader for Tale of Immortal.
2. Close the game.
3. Back up your save files before installing or updating mods.
4. Copy the DLL(s) you want into:

```text
<Tale of Immortal>\Mods\
```

For Partner protection:

```text
PartnerAffinityProtection.dll
```

For Master protection:

```text
MasterAffinityProtection.dll
```

You may install both at the same time.

The old in-game Local Mods affinity package, if you created or imported one during development, should remain disabled or removed so the same protection logic is not installed twice.

The old diagnostic `AffinityLoaderProbe.dll` is not required and should not be installed for normal gameplay.

## What the mods do

Both mods use the same protection strategy:

- Harmony-patch `DataUnit.RelationData.AddIntim` and `SetIntim`.
- Block negative affinity changes for the relationship type handled by that mod.
- Allow positive affinity changes.
- Preserve raw fractional affinity when the game's float value can be safely validated.
- Respect the normal maximum affinity of `300`.
- Allow existing above-cap values to normalize back to `300`.
- Leave unrelated NPC relationships unchanged.
- Leave `ClearIntim` unpatched so actual relationship removal remains game-controlled.
- Write diagnostic information to `MelonLoader/Latest.log`.

## Gameplay verification

### Partner Affinity Protection v0.2.4

Gameplay testing confirmed:

- negative partner-affinity writes being blocked;
- positive affinity changes still occurring;
- unrelated NPC affinity changes remaining possible in observed tests;
- fractional affinity preservation;
- affinity values being capped/normalized to `300`;
- month-skipping affinity-loss events being intercepted;
- a save being closed and reloaded successfully after partner interactions and month skipping.

The tested numerical path was primarily **NPC → player**. Not every possible game event or both directions have been exhaustively tested.

### Master Affinity Protection v0.1.0

Gameplay testing confirmed the player's current Master being recognized at runtime.

During two attacks on the Master, the game attempted to reduce **Master → player** affinity from a raw value of `279.81` to `276.25` and `279`. Both writes were blocked, and the post-write raw affinity remained `279.81`.

The reverse **player → Master** direction has not yet been directly exercised in gameplay, although it uses the same shared protection strategy.

## Building from source

Create:

```text
ModCode\Local.props
```

from:

```text
ModCode\Local.props.example
```

and set `GameDir` to your Tale of Immortal installation.

### Partner Affinity Protection

Run the cap-policy checks:

```powershell
dotnet run --project .\tools\AffinityCapTests\AffinityCapTests.csproj -c Release
```

Build:

```powershell
dotnet build .\tools\AffinityStandalone\AffinityStandalone.csproj -c Release
```

Output:

```text
tools\AffinityStandalone\bin\Release\net6.0\PartnerAffinityProtection.dll
```

### Master Affinity Protection

Build:

```powershell
dotnet build .\tools\MasterAffinityStandalone\MasterAffinityStandalone.csproj -c Release
```

Output:

```text
tools\MasterAffinityStandalone\bin\Release\net6.0\MasterAffinityProtection.dll
```

## Troubleshooting

If the game crashes or behaves unexpectedly:

1. close the game;
2. preserve `MelonLoader/Latest.log` and the game's `Player.log` before restarting;
3. temporarily remove the affinity-protection DLLs from `Mods`;
4. test with a backup save.

Game updates can change internal methods or data layouts, so re-check the logs after major Tale of Immortal updates.

## Development notes

Additional investigation and test records are available in `docs/`, including:

- [300-cap investigation](docs/affinity-300-cap-investigation.md)
- [fractional precision test](docs/fractional-precision-test.md)
- [assembly findings](docs/assembly-findings.md)
- [Master Affinity Protection verification](docs/master-affinity-protection.md)

Do not commit or redistribute game DLLs, private saves, local configuration paths, or private logs.
