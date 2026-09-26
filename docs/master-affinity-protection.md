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

## Verified gameplay test — 2026-09-26

The first runtime Master-filter test passed.

Test context:

- player attacked their current Master twice;
- no visible affinity-loss notification was observed;
- the Master's heart display remained unchanged;
- MelonLoader identified the relation as the current Master and logged protected writes.

Observed raw affinity:

```text
MASTER->player current raw = 279.81
requested decrease        = 276.25
passed value              = 279.81
post-write raw            = 279.81

MASTER->player current raw = 279.81
requested decrease        = 279
passed value              = 279.81
post-write raw            = 279.81
```

The same two decrease attempts were observed again after the second attack and were blocked identically.

This confirms:

- `UnitRelationType.Master` is recognized correctly at runtime for this save;
- Master -> player affinity decreases are blocked;
- the raw fractional value is preserved rather than rounded to the integer getter;
- the standalone DLL can coexist with Partner Affinity Protection during this test.

The reverse direction, player -> Master, has not yet been directly exercised in gameplay.

## Useful log command

```powershell
$log = Join-Path $gameDir 'MelonLoader\Latest.log'
Select-String -Path $log `
  -Pattern 'MasterAffinityProtection|NUMERIC MASTER|DECAY BLOCKED|CAP NORMALIZED|CAP LIMITED|Exception|Error' `
  -Context 0,1
```

## Status

`0.1.0-test`: Master -> player affinity protection is gameplay-verified. Reverse-direction behavior remains to be tested before removing the test suffix.
