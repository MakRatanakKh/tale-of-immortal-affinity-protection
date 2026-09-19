# Build troubleshooting: missing HarmonyLib / MelonLoader

If `dotnet build` reports `CS0246` for `HarmonyLib`, `MelonLoader`, or `Harmony`, the compiler cannot resolve the loader libraries even if it can resolve `Assembly-CSharp.dll`. The two are separate dependencies. The project now looks recursively under `GameDir/MelonLoader` for `MelonLoader.dll`, `0Harmony.dll`, `HarmonyLib.dll`, and `HarmonyX.dll`. Do not assume that a directory exists just because the game DLL does.

In PowerShell **from the repository root**:

```powershell
# Use the game location already configured in your local file.
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
"GameDir: $gameDir"
Get-ChildItem -LiteralPath $gameDir -Recurse -File -Filter '*.dll' -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match '^(MelonLoader|0Harmony|HarmonyLib|HarmonyX)\.dll$' } |
    Select-Object -ExpandProperty FullName
```

If no MelonLoader/Harmony DLLs appear, the locally installed loader/toolchain is incomplete or libraries are outside GameDir. **Do not download a random DLL**: use the exact compatible libraries that came with the existing game's modding toolchain. Share the paths printed above (not the DLL files) so the project's reference locations can be adjusted accurately. Also check what exists under `GameDir/MelonLoader`.

After pulling changes, retry:

```powershell
git pull origin main
dotnet build .\ModCode\ModMain\ModMain.csproj -c Release
```

This is a prototype; compilation, runtime patch installation, and actual affinity behavior are separate tests. Do not use it on your only save.
