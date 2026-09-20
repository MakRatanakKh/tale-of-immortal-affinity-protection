# Fractional affinity precision — 0.2.3 experimental test

**Status as of 2026-09-20:** User built the standalone project successfully with five nullable-reference warnings and no errors, backed up the prior 0.2.2 DLL, copied the new standalone DLL, and supplied a post-launch MelonLoader log. **NPC → player fractional-read and clamp behavior is observed working in this session** for NPC ID `UdlliE`. This is not full validation of every relationship direction or game mechanic.

## What changed

- Negative `AddIntim` cancellation and current `Married`/`Lover` filter are unchanged.
- A protected `SetIntim` write tries to read the pre-write float from `RelationData.intimToPlayerUnit` for NPC → player, or `RelationData.intimToUnit[otherId]` for player → NPC.
- A candidate must be finite and within 1.01 of the integer `GetIntim` getter. This is a **plausibility check, not definitive proof of field semantics**. The fallback uses the previous integer guard and cannot guarantee fractional protection.
- `NUMERIC PROTECTED` includes the raw value and its source. `ClearIntim` is still not patched.

## User-observed build and runtime result

- `dotnet build .\tools\AffinityStandalone\AffinityStandalone.csproj -c Release` succeeded with 5 nullable-reference warnings, 0 errors, and emitted `AffinityProtectionStandalone.dll`. User copied it to the game `Mods` folder.
- Log `[11:43:39.753]`: fractional guard enabled.
- For protected NPC ID `UdlliE`, `NPC->player`, at `[11:45:11]` the readout starts at `GetIntim(int)=302; raw=302; rawSource=intimToPlayerUnit`. An allowed `AddIntim` increase causes `SetIntim requestedAbsolute=303.375`; after it, integer getter is `303` and raw display reads `303.38` (log formatting; do not claim displayed decimal digits are the exact binary float).
- Subsequent `SetIntim` attempts request `300`, `299.25`, and `302`. Each is logged `CLAMPED`, passing the previous raw affinity displayed as `303.38` with `guard=intimToPlayerUnit`. Each following `AFTER` readout remains `GetIntim(int)=303; raw=303.38`. **This demonstrates that these intercepted decreases did not drop the raw affinity in the observed session.**
- The supplied filtered output contains no `Exception` or `Error` matches; this does not exclude unrelated errors elsewhere in the complete log.

## Important caveats / unverified

- The ID `UdlliE` is a relationship the mod classified as protected, but the log does not independently associate it with a character name.
- Only the **NPC → player** raw-store direction (`intimToPlayerUnit`) was exercised in the supplied log. `player → NPC` (`intimToUnit`) remains untested for fractional values, including fallback behavior.
- The requested absolute `300` might be a game cap/normalization, not just relationship decay. Preventing all decreases may also prevent intentional clamping to a normal cap; inspect the game's intended semantics before calling that universally correct.
- The 1.01-point plausibility threshold does not by itself prove field identity or all edge cases. Breakups, alternative write methods, save/reload persistence, game updates and all month/year transitions remain unverified.
- Keep a save backup and the 0.2.2 DLL backup; do not commit private screenshots/logs/saves or game DLLs to the public repository. No further build is required just to document this result.

## Recheck if needed

With a disposable backup save, test the other direction (`player->NPC`) for a fractional raw value and a decreasing request. Verify `rawSource=intimToUnit`, `SetIntim CLAMPED`, and identical raw before/after. If an error, mismatch or unexpected gameplay occurs, fully close the game and restore `AffinityProtectionStandalone-0.2.2-working.dll` to the game `Mods/AffinityProtectionStandalone.dll`, then share the relevant logs privately.
