# Build and test the experimental mod (Windows)

**Status:** The user confirmed that the source compiles successfully on Windows (`net472`, `MOD_AffinityProtection.dll`). Packaging and in-game behavior are **not yet verified**. Back up your save before attempting to use the mod.

## 1. Configure once and compile

From PowerShell in the cloned repository, if you have already configured `ModCode/Local.props`, **keep it**; do not overwrite it with the example:

```powershell
git pull origin main
dotnet build .\ModCode\ModMain\ModMain.csproj -c Release
```

On first setup only, copy `ModCode/Local.props.example` to `ModCode/Local.props`, and set `GameDir` to the folder containing your game's `MelonLoader` directory. `Local.props` and proprietary DLLs must not be committed.

## 2. Package without Git metadata

TaleOfImmortalTool 0.6.1 walked into `.git/config` when packing straight from the repository, then failed with `DirectoryNotFoundException` because the output `.git` directory was not created. The project `.gitignore` now excludes `.git/` for the tool's ignore walker. Pass the `--glob` override explicitly too, to prevent Git history/config from being included even if ignore-file behavior changes:

```powershell
toi mod pack . -o "$env:USERPROFILE\Desktop\AffinityProtection-test" --clean --glob '.git/'
```

**Important:** `-o` is an output *base* path by default: the tool formats the actual folder as `Mod_{{SoleId}}_{{FolderName}}`. For this project, the expected actual folder is:

```text
%USERPROFILE%\Desktop\Mod_AffinityProtection_AffinityProtection-test
```

The packer prints its actual `Output Folder:` path. Read that rather than assuming the `-o` argument is the final directory. Its `--clean` option deletes that generated output directory if it already exists; only point it at a disposable test destination.

To inspect the package in PowerShell:

```powershell
$package = Join-Path $env:USERPROFILE 'Desktop\Mod_AffinityProtection_AffinityProtection-test'
Get-ChildItem -LiteralPath $package -Recurse | Select-Object -ExpandProperty FullName
Test-Path -LiteralPath (Join-Path $package 'ModExportData.cache')
Test-Path -LiteralPath (Join-Path $package 'ModCode\dll\MOD_AffinityProtection.dll')
Test-Path -LiteralPath (Join-Path $package '.git')
```

The first two `Test-Path` results should be `True` and the `.git` result should be `False`. If the output or checks differ, stop and send the output before installing. A packer success message by itself does not prove the code DLL was included.

## 3. Verify the hook was actually installed (backup save only)

After verifying the package, install the test output via the game's supported mod import/loader flow. Do **not** copy the raw DLL into MelonLoader's generic `Mods` folder; this project uses the game's in-game `ModMain.Init()` entry point. Start the game with the mod enabled and inspect `Player.log` or the MelonLoader log for `AffinityProtection:`. Both exact-overload `patched` messages must appear. If they do not, assume the mod is not active and send the relevant startup log lines.

## 4. Test safely on a backup save

1. Record affinity values in both directions between the player and a current lover/spouse, if accessible.
2. Advance through a month/year rollover that normally reduces affinity and inspect both directions.
3. Trigger a negative interaction with that partner, then a gift that increases affinity.
4. Trigger an unrelated NPC's negative interaction: it must still decrease normally.
5. Test relationship breakup/removal: `ClearIntim` is intentionally not hooked; breakup semantics are unverified.
6. Save/reload and check for unexpected changes.

Known limitations: the `SetIntim` prefix compares a `float` against `GetIntim()` (which returns `int`), so sub-integer losses might evade it. Other direct affinity writes and `ClearIntim` may bypass the patch. Relationship breakup can happen for other reasons. Record game version, before/after values, and log messages when reporting issues.
