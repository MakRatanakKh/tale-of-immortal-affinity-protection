# Initialization diagnostic (experimental build 0.1.2)

The previous diagnostic revision introduced `UnityEngine.Debug.Log` and `LogError`, causing CS0234 and CS0012 on the user's setup because the project did not reference `UnityEngine.CoreModule`. Revision 0.1.2 drops the Unity Debug calls and logs best-effort to MelonLoader **and** `%TEMP%\AffinityProtection-diagnostic.log` with timestamps. It does not claim affinity protection works yet.

From PowerShell in the repository, run `git pull origin main`, then `dotnet build .\ModCode\ModMain\ModMain.csproj -c Release`. **Only if the build succeeds:** close the game, copy the newly built `ModCode\ModMain\bin\Release\MOD_AffinityProtection.dll` over the installed packed mod's `ModCode\dll\MOD_AffinityProtection.dll` (not `MelonLoader\Mods`). Keep the installed package's `ModExportData.cache` unchanged for this code-only update.

With the game closed, optional: `Remove-Item (Join-Path $env:TEMP 'AffinityProtection-diagnostic.log') -ErrorAction SilentlyContinue` to remove old diagnostic output. Restart the game with Partner Affinity Protection enabled, enter the world using a **backup save**, exit the game, then `Get-Content (Join-Path $env:TEMP 'AffinityProtection-diagnostic.log') -ErrorAction SilentlyContinue`. Check `MelonLoader\Latest.log` for `AffinityProtection:` as well.

Interpretation:

- `Init ENTERED (diagnostic build 0.1.2)` appears: the game's C# entry point ran; read following success/error lines.
- Both `patched ... AddIntim` and `patched ... SetIntim` and `startup patch installation complete`: Harmony installation reported success; **not** proof affinity is protected, so in-game tests are still required.
- No diagnostic file, no `Init ENTERED` in MelonLoader logs: the entry point may not have run, **or** both log sinks failed. Reconfirm the installed DLL timestamp and enabled Local Mods checkbox before modifying the patch.

Never upload local diagnostic logs or game DLLs into the public repository. Share the diagnostic log in the private chat instead, optionally redacting paths.
