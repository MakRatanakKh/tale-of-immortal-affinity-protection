# Initialization diagnostic (experimental build 0.1.2)

**Status:** The user reports the updated code compiles successfully, and the installed DLL copy command completes, but `%TEMP%\AffinityProtection-diagnostic.log` does not exist after their test. This does **not** by itself identify the cause: the installed DLL may not be the one executed, the game may not have relaunched/entered a world after the copy, `ModMain.Init()` may never be called, or the best-effort file writer may fail/use a different temp directory. Do not claim patches work or change metadata speculatively.

Revision 0.1.1 introduced `UnityEngine.Debug` and failed to compile due to missing UnityEngine.CoreModule references. Revision 0.1.2 removes Unity Debug and logs best-effort both to MelonLoader and to `Path.Combine(Path.GetTempPath(), "AffinityProtection-diagnostic.log")`, using an immediate `Init ENTERED` message. Compile status is confirmed; runtime behavior is not.

## Next investigation: verify exact installed code and timestamps

With the game **closed**, from the repository root in PowerShell:

```powershell
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
$built = (Resolve-Path '.\ModCode\ModMain\bin\Release\MOD_AffinityProtection.dll').Path
$installed = Join-Path $gameDir 'ModExportData\Mod_AffinityProtection_AffinityProtection-test\ModCode\dll\MOD_AffinityProtection.dll'
Get-FileHash -LiteralPath $built, $installed -Algorithm SHA256 | Select-Object Path, Hash
Get-Item -LiteralPath $built, $installed | Select-Object FullName, LastWriteTime, Length
$playerLog = Join-Path $env:USERPROFILE 'AppData\LocalLow\guigugame\guigubahuang\Player.log'
$melonLog = Join-Path $gameDir 'MelonLoader\Latest.log'
Get-Item -LiteralPath $playerLog, $melonLog | Select-Object FullName, LastWriteTime, Length
Select-String -Path $playerLog, $melonLog -Pattern 'AffinityProtection|MOD_AffinityProtection|Exception|Failed' | Select-Object -Last 40
$diagnosticCandidates = @((Join-Path $env:TEMP 'AffinityProtection-diagnostic.log'), (Join-Path $env:WINDIR 'Temp\AffinityProtection-diagnostic.log')) | Select-Object -Unique
foreach ($p in $diagnosticCandidates) { if (Test-Path -LiteralPath $p) { Get-Item -LiteralPath $p; Get-Content -LiteralPath $p } }
```

The SHA-256 hashes **must match** to establish that the installed file is identical to the new compiled DLL. Compare log modification times to the copy and the **latest complete game restart and world load**. Missing/newer timestamps can reveal a stale log. The file writer intentionally swallows I/O exceptions, so a missing file alone is inconclusive. Do not publish the logs or local paths to this public repository.

If hashes differ, exit the game and copy the new DLL again. If hashes match but logs are stale, launch the game, enter a world on a backup save, exit, and rerun log checks. If hashes match, logs are fresh, and neither sink logs `Init ENTERED`, investigate the game's C# mod code loader/namespace and whether `ModMain.Init()` is actually invoked. `MelonLoader: 0 Mods loaded` describes the separate MelonLoader Mods folder, not the game's built-in Local Mods, so do not interpret it as definitive evidence of failure.

Potential follow-up *hypothesis*, **not a verified cause**: the project currently uses `soleID=AffinityProtection` and namespace/assembly `MOD_AffinityProtection`, whereas the `toi` project generator normally chooses a six-character ID. Check loader restrictions against the user's specific game build before renaming IDs, since changing identity could disrupt existing mod installs.

## Earlier verification and acceptance

1. Rebuild with `git pull origin main; dotnet build .\ModCode\ModMain\ModMain.csproj -c Release` and only on success copy the built DLL to the installed packed folder's `ModCode\dll\MOD_AffinityProtection.dll` with the game closed. Do not put it in `MelonLoader\Mods`.
2. Exit and restart the game with the checkbox enabled, enter a world on a backup save and exit.
3. Look for `Init ENTERED (diagnostic build 0.1.2)` in temp diagnostics and `MelonLoader\Latest.log`. If present, read subsequent success/error lines.
4. Both `patched ... AddIntim` and `patched ... SetIntim` plus `startup patch installation complete` imply Harmony installation reported success, **not** that affinity protection has been verified; behavioral tests are still necessary.

Never commit proprietary game DLLs, saves, sensitive logs or personal paths to this public repository.
