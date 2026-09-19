# Build and test the experimental mod (Windows)

**Status:** This is a source prototype. It has NOT been compiled, packaged, loaded or verified in-game. Back up your save before attempting to use it.

## 1. Update your local repository

In PowerShell in the cloned repository:

```powershell
git pull origin main
Copy-Item .\ModCode\Local.props.example .\ModCode\Local.props
notepad .\ModCode\Local.props
```

Replace `C:\path\to\TaleOfImmortal` in that local XML file with your own game directory (the folder containing `MelonLoader`). Save and close Notepad. `Local.props` is ignored by Git. Do not commit game DLLs or any saves.

## 2. Compile

```powershell
dotnet build .\ModCode\ModMain\ModMain.csproj -c Release
```

The project looks for `Assembly-CSharp.dll` under `MelonLoader/Managed` first and then `MelonLoader/Il2CppAssemblies`; it references loader libraries from `MelonLoader` and `MelonLoader/Managed`. If there are C# compile errors, paste the complete text here before proceeding. A successful compile alone does not prove Harmony can hook the game.

## 3. Package using your installed Tale of Immortal tool

Only after compilation succeeds, from the repo root, use:

```powershell
toi mod pack . -o "$env:USERPROFILE\Desktop\AffinityProtection-test" --clean
```

The project layout and manifest follow existing community examples, but this command and the new mod ID have **not** been tested on this particular installation. If the tool reports a manifest or packaging error, send that output. Verify the output is a game mod package before installing it via the game's mod manager. Do not copy the raw DLL to MelonLoader's generic Mods folder: this is an in-game mod project with a `ModMain.Init()` entry point, not a standard MelonMod class.

## 4. Verify the hook was actually installed

Launch the game with the mod enabled, then inspect `Player.log` / the MelonLoader log. Search for `AffinityProtection:`. Both exact-overload `patched` messages must appear. If either is absent, treat the mod as NOT active. A successful `dotnet build` or a packaging success message is not sufficient. The logs deliberately do not claim that game behavior was tested.

## 5. Test safely on a backup save

1. Write down affinity numbers for both player -> current lover/spouse and lover/spouse -> player, if accessible.
2. Advance through a month/year rollover that normally reduces affinity and inspect both directions.
3. Trigger a normal negative interaction with that partner, then a gift that increases affinity.
4. Trigger an unrelated NPC's negative interaction: it must still decrease normally.
5. Test a normal relationship breakup/removal on the backup save: `ClearIntim` is deliberately not hooked; breakup semantics are unverified.
6. Save/reload and confirm the protection does not corrupt the save.

Known limitations: the `SetIntim` prefix compares a new `float` value to `GetIntim()` which returns `int`, so decreases smaller than one point may evade it. Other direct affinity storage writes or `ClearIntim` may bypass the two patched methods. A relationship may also break for reasons other than a change in affinity. If testing reveals any of these cases, report the game version, action, before/after values and relevant log messages so we can refine the next revision.
