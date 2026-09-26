# Master Affinity Protection

Standalone MelonLoader mod for Tale of Immortal.

## Goal

Prevent affinity decreases in either direction between the player and the player's current **Master**, while:

- allowing affinity increases;
- keeping the normal +300 affinity cap;
- preserving fractional affinity where the raw float store can be validated;
- leaving unrelated NPCs and Students/disciples unchanged;
- leaving `ClearIntim` unpatched so relationship removal remains game-controlled.

The inspected game assembly defines `UnitRelationType.Master = 10` and `UnitRelationType.Student = 11`. This mod intentionally checks only `Master`.

## Build

From the repository root:

```powershell
dotnet build .\tools\MasterAffinityStandalone\MasterAffinityStandalone.csproj -c Release
```

Expected DLL:

```text
tools\MasterAffinityStandalone\bin\Release\net6.0\MasterAffinityProtection.dll
```

## Install for test

Close Tale of Immortal first.

```powershell
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
Copy-Item `
  '.\tools\MasterAffinityStandalone\bin\Release\net6.0\MasterAffinityProtection.dll' `
  (Join-Path $gameDir 'Mods\MasterAffinityProtection.dll') -Force
```

Partner Affinity Protection can remain installed; the mods use separate Harmony IDs and relationship filters.

## First test

Use a backup save.

1. Start the game and load a save where the player currently has a Master.
2. Perform an action that normally lowers the Master's affinity toward the player, or the player's affinity toward the Master.
3. Exit the game.
4. Inspect:

```powershell
$log = Join-Path $gameDir 'MelonLoader\Latest.log'
Select-String -Path $log `
  -Pattern 'MasterAffinityProtection|NUMERIC MASTER|DECAY BLOCKED|CAP NORMALIZED|CAP LIMITED|Exception|Error' `
  -Context 0,1
```

A successful protection event should show `AddIntim BLOCKED` or `SetIntim DECAY BLOCKED` and an unchanged post-write raw value.

## Status

`0.1.0-test`: implementation prepared from the gameplay-tested Partner Affinity Protection logic. The Master relationship filter itself still requires in-game verification.