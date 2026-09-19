# Build troubleshooting: missing game and loader references

The game, Harmony, MelonLoader, and generated IL2CPP assemblies are separate compile-time dependencies. The build searches under the `GameDir` configured in your untracked `ModCode/Local.props`. Do not commit proprietary DLLs or download random replacements.

## CS0246: HarmonyLib / MelonLoader

If `dotnet build` reports `CS0246` for `HarmonyLib`, `MelonLoader`, or `Harmony`, the compiler cannot resolve the loader libraries even if it resolves `Assembly-CSharp.dll`. The project searches recursively under `GameDir/MelonLoader` for `MelonLoader.dll`, `0Harmony.dll`, `HarmonyLib.dll`, and `HarmonyX.dll`.

## CS0118: Harmony is a namespace but is used like a type

The mod source must use `HarmonyLib.Harmony` for its Harmony instance and constructor to disambiguate the class from the game's namespace. This does **not** mean renaming `0Harmony.dll`.

## CS0012: Object is defined in Il2Cppmscorlib

Generated `Assembly-CSharp.dll` uses types whose core assembly is `Il2Cppmscorlib.dll`, which is not interchangeable with .NET Framework `mscorlib.dll`. The project explicitly references that DLL, preferring the copy beside `Assembly-CSharp.dll` and otherwise searching recursively under `GameDir/MelonLoader`. If it is missing, the build now emits a specific path error.

In PowerShell **from the repository root**, find dependency DLL locations using your configured game path:

```powershell
$gameDir = ([xml](Get-Content .\ModCode\Local.props -Raw)).Project.PropertyGroup.GameDir
"GameDir: $gameDir"
Get-ChildItem -LiteralPath $gameDir -Recurse -File -Filter '*.dll' -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -match '^(MelonLoader|0Harmony|HarmonyLib|HarmonyX|Il2Cppmscorlib|Assembly-CSharp)\.dll$' } |
    Select-Object -ExpandProperty FullName
```

If a required DLL is not found, share only the printed paths so we can match the references to your actual installation. Do not upload game or loader binaries to the public repository.

After pulling changes, retry:

```powershell
git pull origin main
dotnet build .\ModCode\ModMain\ModMain.csproj -c Release
```

Compiling, loading the patch, and confirming affinity changes in-game are separate tests. Use a backup save.
