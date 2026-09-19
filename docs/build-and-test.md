# Build, package and test the experimental mod (Windows)

**Status (September 19, 2026):** Windows compilation and `toi mod pack` succeeded. The game shows `Partner Affinity Protection` in Local Mods with its checkbox enabled; `Player.log` shows it loading the mod's `ModExportData.cache` at line 301, and the installed `ModCode/dll/MOD_AffinityProtection.dll` exists. **Neither the C# entry point nor Harmony patches have been confirmed. Do not claim the mod is functional.** Back up your save before gameplay testing.

The user's `MelonLoader/Latest.log` (MelonLoader v0.7.3) reports `0 Mods loaded.` at startup. That refers to **standalone MelonLoader mods**, not necessarily the game's separate `ModExportData` mod system. The Player.log package-loading line confirms data loading, not C# execution. Both logs contain no `AffinityProtection:` patch messages and no explicit error identifying why the entry point has not been observed.

## 1. Build (previously passed)

From PowerShell in the cloned repository, retain `ModCode/Local.props`:

```powershell
git pull origin main
dotnet build .\ModCode\ModMain\ModMain.csproj -c Release
```

For a new machine only, copy `ModCode/Local.props.example` to `ModCode/Local.props`, set `GameDir` to the game directory containing `MelonLoader`, and do not commit local properties or game binaries.

## 2. Package (previously passed)

```powershell
toi mod pack . -o "$env:USERPROFILE\Desktop\AffinityProtection-test" --clean --glob '.git/'
```

The actual packed directory is `%USERPROFILE%\Desktop\Mod_AffinityProtection_AffinityProtection-test`. The `.git/` ignore fixes a TaleOfImmortalTool 0.6.1 copy failure. Packing copies the built DLL to `ModCode/dll/MOD_AffinityProtection.dll` and writes `ModExportData.cache`. Extras (`docs/`, `tools/`, `README.md`) are harmless development files. Do not use the repository root as the installed mod.

## 3. Install in the game's Local Mods system

With the game **closed**, import the packed folder through `Mod > Local Mods > Import mod data` or place the entire folder in `<GameDir>/ModExportData/`. Enable `Partner Affinity Protection`. Do **not** copy this DLL to MelonLoader's generic `Mods` directory; `0 Mods loaded` there is not a valid pass/fail indicator for this in-game mod.

## 4. Deploy and test the startup diagnostic revision

Commit `a6e6c748` adds `AffinityProtection: Init ENTERED (diagnostic build 0.1.1)` as the **first operation in `ModMain.Init()`**, logs independently to Unity (`Player.log`) and MelonLoader, and catches initialization exceptions around the entire patch-setup routine. It does not claim to fix the actual activation problem; it makes it observable.

After `git pull` and a successful build, close the game before changing files. For this diagnostic-only code change, the installed package's metadata is unchanged, so you can copy *only the compiled DLL to the game's existing in-game package DLL location* (NOT to generic MelonLoader Mods):

```powershell
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
$builtDll = (Resolve-Path .\ModCode\ModMain\bin\Release\MOD_AffinityProtection.dll).Path
$installedDll = Join-Path $gameDir 'ModExportData\Mod_AffinityProtection_AffinityProtection-test\ModCode\dll\MOD_AffinityProtection.dll'
if (!(Test-Path -LiteralPath $installedDll)) { throw "Installed DLL not found: $installedDll" }
Copy-Item -LiteralPath $builtDll -Destination $installedDll -Force
Get-FileHash -Algorithm SHA256 $builtDll, $installedDll | Select-Object Path, Hash
```

Both hashes should match. If you prefer to rebuild the entire package instead, rerun `toi mod pack` and replace the installed *test package* folder, without touching other mods.

Restart the game, enable the mod if necessary, and inspect both actual log paths (confirmed from user's machine):

```powershell
$playerLog = Join-Path $env:USERPROFILE 'AppData\LocalLow\guigugame\guigubahuang\Player.log'
$melonLog  = Join-Path $gameDir 'MelonLoader\Latest.log'
Select-String -Path $playerLog, $melonLog -Pattern 'AffinityProtection:' -Context 1,2
```

Interpretation:
- `Init ENTERED`, then `patched ...AddIntim(...)`, `patched ...SetIntim(...)`, and `startup patch installation complete`: entry point and patch installation were reached, but gameplay behavior **still must be tested**.
- `Init ENTERED` followed by a diagnostic error: report the exact exception; the mod is not ready.
- Package-loading line but no `Init ENTERED` in either log: do not assume method resolution is the cause; investigate the game's **code-loading/entry-point path** and whether the installed binary is the freshly rebuilt version.
- Missing package-loading line: revisit Local Mods enable/import and restart.

## 5. Only after both patch-install logs: test a backup save

1. Record player-to-partner and partner-to-player affinity if available.
2. Advance through a month/year rollover that normally lowers affinity; inspect both directions.
3. Trigger a negative interaction, then an affinity-increasing gift.
4. Confirm unrelated NPCs can still lose affinity.
5. Test breakup/removal: `ClearIntim` remains untouched and breakup behavior is unverified.
6. Save/reload and check for unexpected changes.

Known limitations: direct `SetIntim` guard compares a `float` against integer `GetIntim()`, so sub-integer losses might evade it; other writers and `ClearIntim` can bypass current hooks. The game may end a relationship independently of affinity. Report the game version, before/after values and log messages.
