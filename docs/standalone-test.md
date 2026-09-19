# Standalone MelonLoader affinity test (experimental, NOT yet built or game-tested)

The user's MelonLoader 0.7.3 successfully loaded `AffinityLoaderProbe.dll` from `<GameDir>/Mods` and logged `AffinityLoaderProbe: OnInitializeMelon reached`. Both six-character-ID and earlier in-game Local Mod packages were visible in `Player.log`, but neither produced `AffinityProtection: Init ENTERED`. Therefore test a **separate** MelonMod entry point rather than continuing to change Local Mod IDs. This is a loader-path experiment, **not proof the affinity patch works**.

The standalone project `tools/AffinityStandalone/AffinityStandalone.csproj` targets `net6.0`, references local game/MelonLoader/Harmony/IL2CPP dependencies, and links the same original `ModCode/ModMain/ModMain.cs` so the patch logic is not duplicated. The wrapper logs `AffinityProtectionStandalone: OnInitializeMelon ENTERED`, calls the shared `Init`, and removes patches on deinitialize. Do not install the old Local Mod and the standalone mod together; they share a Harmony ID and patch targets.

## 1. Build **before changing the installed game**

Close the game. In PowerShell at repository root:

```powershell
git pull origin main
dotnet build .\tools\AffinityStandalone\AffinityStandalone.csproj -c Release
```

**STOP and share errors if the build fails.** This net6 project has not yet been built on the user's Windows setup. It does not use `toi mod pack`. Expected output, only after successful build: `tools\AffinityStandalone\bin\Release\net6.0\AffinityProtectionStandalone.dll`. Verify with `Test-Path` before copying. Do not copy the original `MOD_Rk7Qp2.dll` to MelonLoader/Mods; it does not carry a `MelonInfo` entry point.

## 2. Isolate and install the new test

Back up a save before any gameplay test. In game's Local Mods menu, uncheck `Partner Affinity Protection`, exit fully, and move the **single** folder `<GameDir>/ModExportData/Mod_Rk7Qp2_AffinityProtection-test` out of `ModExportData` to a safe backup directory (rather than deleting it). Leave unrelated mods untouched. A disabled in-game mod alone is not sufficient assurance that the assembly will not be loaded by all code paths; moving the one folder makes the comparison clearer.

If the earlier `Mod_AffinityProtection_AffinityProtection-test` folder is still installed, also move that one out. Do not modify any unrelated files. The standalone `AffinityLoaderProbe.dll` can remain because it only logs, although you may move it out of `<GameDir>/Mods` to simplify the logs.

With the game closed, after a successful build:

```powershell
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
$built = '.\tools\AffinityStandalone\bin\Release\net6.0\AffinityProtectionStandalone.dll'
if (-not (Test-Path -LiteralPath $built)) { throw "Standalone build output missing: $built" }
$mods = Join-Path $gameDir 'Mods'
New-Item -ItemType Directory -Path $mods -Force | Out-Null
Copy-Item -LiteralPath $built -Destination (Join-Path $mods 'AffinityProtectionStandalone.dll') -Force
```

## 3. Runtime test (do NOT assume success merely because a MelonMod loads)

Start the game and reach the main menu. Exit, then:

```powershell
Select-String -Path (Join-Path $gameDir 'MelonLoader\Latest.log') -Pattern 'AffinityProtectionStandalone|AffinityProtection:|Exception|ERROR' -Context 1,2
```

Look for `AffinityProtectionStandalone: OnInitializeMelon ENTERED`, then `AffinityProtection: Init ENTERED`, and either **both** `patched ... AddIntim` and `patched ... SetIntim` with `startup patch installation complete`, or a specific failure message/stack trace. If startup is successful, only then load a **backup save** and test an actual affinity decrease plus a positive gain and a decrease for unrelated NPCs. Any message saying a patch was installed is not proof of functional protection.

Known limitation: `SetIntim` currently reads the integer `GetIntim()` so fractional losses can evade it. `ClearIntim` is not patched to avoid breaking relationship removal. If a patch throws or the game behaves incorrectly, exit and remove/move **only** `<GameDir>/Mods/AffinityProtectionStandalone.dll` to roll back; leave other mods installed.

The standalone test's logging may also appear in `%TEMP%/AffinityProtection-diagnostic.log`, but `MelonLoader/Latest.log` is the primary evidence.
