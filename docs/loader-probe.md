# Isolated loader probe (diagnostic only)

**Why:** The game logs `Mod_Rk7Qp2_AffinityProtection-test/ModExportData.cache` as loaded but `%TEMP%/AffinityProtection-diagnostic.log` is absent. Matching DLL hashes and recent logs already ruled out a stale deployed DLL on the prior test; the six-character ID change did not establish `ModMain.Init()` execution. MelonLoader `Latest.log` previously identified version **0.7.3 Open-Beta**. Historically some Tale of Immortal code/Harmony mods required older versions; this does not prove the current failure or justify downgrading all of the user's existing mods.

This probe uses the standalone MelonLoader `MelonMod` entry point, independently of Tale of Immortal's Local Mods loader. It logs one startup marker and writes one text file to TEMP. It **does not patch the game or modify affinity/saves.** It is an experiment, not a working affinity mod.

## Build from the repository root

The game must be **closed** before installing a DLL. Ensure `ModCode/Local.props` still points at the user's game folder. Run:

```powershell
git pull origin main
dotnet build .\tools\LoaderProbe\LoaderProbe.csproj -c Release
```

The probe targets `net6.0`, matching the .NET 6 runtime reported by the current MelonLoader log. If your SDK cannot resolve a .NET 6 targeting pack, **stop and share the build error** rather than changing target frameworks or downgrading MelonLoader. Expected build output: `tools/LoaderProbe/bin/Release/net6.0/AffinityLoaderProbe.dll`.

## Run one controlled test

With the game closed:

```powershell
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
$probe = '.\tools\LoaderProbe\bin\Release\net6.0\AffinityLoaderProbe.dll'
$modsDir = Join-Path $gameDir 'Mods'
New-Item -ItemType Directory -Path $modsDir -Force | Out-Null
Copy-Item -LiteralPath $probe -Destination (Join-Path $modsDir 'AffinityLoaderProbe.dll') -Force
Remove-Item (Join-Path $env:TEMP 'AffinityLoaderProbe-diagnostic.log') -ErrorAction SilentlyContinue
```

Start the game and enter the main menu. A save is *not* required for this probe. Exit the game and run:

```powershell
Select-String -Path (Join-Path $gameDir 'MelonLoader\Latest.log') -Pattern 'AffinityLoaderProbe|Mods loaded|Mod Loaded' -Context 1,1
Get-Content (Join-Path $env:TEMP 'AffinityLoaderProbe-diagnostic.log') -ErrorAction SilentlyContinue
```

If `AffinityLoaderProbe: OnInitializeMelon reached` appears, native MelonLoader execution works on this setup; investigate/replace only the game's Local Mods code-loading route. If it doesn't appear, inspect the *current launch's* `Latest.log` for rejection/exception and share the result. Absence of the TEMP marker alone may be a path/permissions issue; the MelonLoader log is the primary check. Neither outcome proves affinity behavior.

## Cleanup

Close the game and run:

```powershell
Remove-Item -LiteralPath (Join-Path $modsDir 'AffinityLoaderProbe.dll') -ErrorAction SilentlyContinue
```

Do not remove or replace unrelated game mods, `version.dll`, or MelonLoader itself for this test. Do not upload private logs, game DLLs, or save data to the public repository.
