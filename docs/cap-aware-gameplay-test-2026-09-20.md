# Cap-aware affinity test — 2026-09-20

Status: **Initial NPC -> player cap normalization and limiting verified in one disposable-save session. Not a stability or comprehensive protection certification.**

## User-reported test

- Pulled the v0.2.4 cap-aware implementation. Pure `AffinityCapTests` run printed `All 13 affinity cap policy checks passed.`; framework printed a NETSDK1138 out-of-support `net6.0` warning.
- Standalone `dotnet build ... -c Release` succeeded with five pre-existing C# nullable-reference warnings; user copied its DLL to the game's MelonLoader `Mods` directory.
- On a backup/test save, gave two manuals to a partner with full displayed affinity. No crash was reported *during this particular session*, but user had reported multiple crashes during earlier v0.2.3 testing. Crash root cause not identified and no crash logs reviewed.
- At 12:20:26 for NPC ID `UdlliE`, NPC -> player, raw `intimToPlayerUnit` was `302`, integer getter `302`. Requested `303.3125` was changed by the mod to `300` (`CAP NORMALIZED`); the next `AFTER` reading was raw `300`.
- At 12:21:43 the game requested `303.75` and `301` from raw `300`; both were capped to `300` (`CAP LIMITED`), and `AFTER` raw remained `300`. Another pair of `303.75`/`301` requests at 12:22:08 were likewise capped; `AFTER` readings stayed `300`.
- This test independently confirms the deployed cap-aware code was invoked and capped *this direction*, rather than only passing pure unit checks. Logs identify only NPC ID; they do not establish character name.

## Remaining questions and limits

- Not yet verified in this build: blocking normal negative deltas and direct decrements **below 300**, player -> NPC cap/read mapping, unrelated NPC behavior, save/reload persistence, month rollovers and extended stability.
- The underlying game code path issuing exact 300 and earlier crashes remain unexamined. The fix addresses observed above-cap persistence but is **not proven to fix crashes**.
- Pure policy tests cover numeric transformations, not MelonLoader, Harmony, actual game state transitions or other installed mods.

## Next safe checks

1. Keep a separate backup of the save and pre-fix DLL. Use a disposable save; avoid saving over the primary file.
2. On a protected partner whose raw affinity is **below 300**, trigger a small negative interaction and check `DECAY BLOCKED` plus identical `AFTER raw`; also try a positive interaction below cap.
3. If safe, verify the reverse (player -> NPC) direction and one month transition, then save/reload a disposable save to confirm neither direction exceeds 300.
4. If the game crashes, stop testing. Capture `MelonLoader/Latest.log`, the game's `Player.log`, relevant timestamps and action; compare a controlled run with our mod disabled. Do not assert causality from the cap behavior alone.
