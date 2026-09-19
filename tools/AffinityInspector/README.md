# AffinityInspector — metadata-only game API discovery

This is an investigation tool, **not an installed game mod**. It reads the provided DLL without loading/executing game code, then prints candidate owner types, all members on relevant types, method tokens, raw ECMA-335 signature bytes, parameter names and enum constant bytes. It cannot prove which method the game calls at runtime.

Requires the .NET 10 SDK. Run from the repository root in PowerShell (change the game path if your Steam library is elsewhere):

```powershell
$gameDll = 'C:\Program Files (x86)\Steam\steamapps\common\鬼谷八荒\MelonLoader\Il2CppAssemblies\Assembly-CSharp.dll'
dotnet run --project .\tools\AffinityInspector\AffinityInspector.csproj -- "$gameDll" > "$env:USERPROFILE\Desktop\affinity-inventory.txt"
```

If the file lives elsewhere, substitute the actual DLL path. If your loader has *no* `MelonLoader\Il2CppAssemblies`, use the location of the `Assembly-CSharp.dll` you already provided. For errors, rerun without `> ...` to see the complete message.

Open the generated `affinity-inventory.txt` and send it in chat, **not to this public repository**. The report contains only assembly metadata and a SHA-256 checksum, but it may be large; sharing the report privately makes signature verification easier. Do not upload the DLL itself to GitHub. For source bodies/callers, use ILSpy locally as a follow-up after owner types are known.

We will not claim affinity protection works until a patch-installation log and an actual in-game loss/gain test confirm it.
