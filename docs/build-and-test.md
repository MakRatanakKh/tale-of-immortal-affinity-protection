# Build, package and test the experimental mod (Windows)

**Status (September 19, 2026):** The user confirmed a successful Windows `net472` build and a successful `toi mod pack` run. The resulting folder listing contains both `ModExportData.cache` and `ModCode/dll/MOD_AffinityProtection.dll`, and no `.git` folder. **The game has not yet loaded the mod, the Harmony patches have not been confirmed in logs, and actual affinity protection has not been tested.** Back up your save first.

## 1. Build (already passed)

From PowerShell in the cloned repository, keep your existing local `ModCode/Local.props` and run:

```powershell
git pull origin main
dotnet build .\ModCode\ModMain\ModMain.csproj -c Release
```

If setting up a new machine only: copy `ModCode/Local.props.example` to `ModCode/Local.props` and set `GameDir` to the game folder containing `MelonLoader`. Do not commit the local properties file or any game DLLs.

## 2. Package (already passed)

```powershell
toi mod pack . -o "$env:USERPROFILE\Desktop\AffinityProtection-test" --clean --glob '.git/'
```

TaleOfImmortalTool 0.6.1 previously tried to copy `.git/config`, failing with `DirectoryNotFoundException`. The `.git/` ignore pattern fixes this. The tool formats the output name, so the **actual packed folder** is:

```text
%USERPROFILE%\Desktop\Mod_AffinityProtection_AffinityProtection-test
```

Successful console output included `modNamespace: MOD_AffinityProtection`, copying the built DLL to `ModCode/dll/MOD_AffinityProtection.dll`, writing `ModExportData.cache`, and `Successfully packed to:`. The user's directory listing confirms those files. The packer also includes `docs/`, `tools/`, `README.md`, and `.gitignore` — development extras not needed for playing; they do not establish that the mod loads. Do not copy the repository root into the game: copy only the **packed** folder.

For a quick package check:

```powershell
$package = Join-Path $env:USERPROFILE 'Desktop\Mod_AffinityProtection_AffinityProtection-test'
Test-Path -LiteralPath (Join-Path $package 'ModExportData.cache')
Test-Path -LiteralPath (Join-Path $package 'ModCode\dll\MOD_AffinityProtection.dll')
Test-Path -LiteralPath (Join-Path $package '.git')
```

Expected: `True`, `True`, `False`.

## 3. Install and enable as a local in-game mod (NOT a standalone MelonMod)

**Close Tale of Immortal before installing and back up the save you intend to test.** Use the game's built-in `Mod` > `Local Mods` > `Import mod data` control and select the **packed folder** above, OR copy that whole packed folder (not just the DLL, and not the repository root) into `<GameDir>/ModExportData/`. If a previous test folder with the same name is already installed, remove or rename that *specific previous test folder* before copying so you do not nest directories or leave stale files; do not delete unrelated mods.

Open the game, go to `Mod` > `Local Mods`, locate `Partner Affinity Protection`, enable/check it, and restart the game if requested. The game's mod menu labels may differ by language/version. Do **not** copy the DLL to generic `MelonLoader/Mods`: the source uses the game's `ModMain.Init()` entry point.

## 4. Check runtime logs before testing affinity

With the mod enabled, start the game and inspect `Player.log` (typically under the game's `*_Data` folder) and/or the MelonLoader log (under the game's `MelonLoader/Logs`). Search for `AffinityProtection:`. The source should log the exact `patched ... AddIntim(...)` and `patched ... SetIntim(...)` messages plus `startup patch installation complete`. If those lines do not appear, or exceptions appear, assume the patch is **not active** and share relevant log lines. `MelonLogger` output might be routed differently by the game's mod host; absence in one log is not proof of nonexecution, so check both where available.

Example PowerShell to find relevant lines from existing logs (change `$gameDir` if needed):

```powershell
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
Get-ChildItem -LiteralPath $gameDir -File -Recurse -Include Player.log,Latest.log,MelonLoader*.log -ErrorAction SilentlyContinue |
    Select-String -Pattern 'AffinityProtection|MOD_AffinityProtection' -Context 1,2 |
    Select-Object Path,LineNumber,Line
```

If the script finds no logs, search the game's install folder for `Player.log` or `Latest.log` manually and share a relevant excerpt, not an entire save file.

## 5. Test on a backup save only

1. Record affinity values in **both directions** between the player and a current lover/spouse, if accessible.
2. Advance through a month/year rollover that normally reduces affinity; inspect both directions.
3. Trigger a negative interaction with that partner, then a gift that increases affinity.
4. Trigger an unrelated NPC's negative interaction: it must still decrease normally.
5. Test breakup/removal: `ClearIntim` is intentionally untouched; breakup semantics are unverified.
6. Save/reload and inspect for unexpected changes.

Known limitations: the `SetIntim` prefix compares a `float` with `GetIntim()` (an `int`), so sub-integer decreases might evade it. Direct affinity writes or `ClearIntim` may bypass the patch, and breakups may happen independently of affinity. Report the game version, before/after values, and relevant log lines if behavior differs.
