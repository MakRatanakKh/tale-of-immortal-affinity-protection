# Focused protected-partner month test (experimental 0.2.2)

## Result: build and first runtime test completed, September 20, 2026

User pulled main, backed up the previous standalone DLL, successfully built `tools/AffinityStandalone/AffinityStandalone.csproj -c Release` with five nullable warnings and no errors, copied the DLL to `<GameDir>/Mods`, and confirmed the installed path exists. They advanced approximately three or four months on another save; the precise number is unknown. They supplied two relationship UI screenshots for Na Zhen, one showing `Your Affinity` and the other `Target's Affinity` (these are different directions, NOT a before/after pair), with no obvious loss reported.

The user-supplied filtered MelonLoader log recorded the following sequence for the code-identified **protected** NPC ID `UdlliE`, direction **NPC → player**, at 11:26:36:

```text
NUMERIC PROTECTED AddIntim BEFORE; requestedDelta=24; direction=NPC->player; npcId=UdlliE; GetIntim(int)=300
NUMERIC PROTECTED SetIntim BEFORE; requestedAbsolute=301.5; direction=NPC->player; npcId=UdlliE; GetIntim(int)=300
NUMERIC PROTECTED SetIntim AFTER; direction=NPC->player; npcId=UdlliE; GetIntim(int)=302
NUMERIC PROTECTED SetIntim BEFORE; requestedAbsolute=300; direction=NPC->player; npcId=UdlliE; GetIntim(int)=302
NUMERIC PROTECTED SetIntim CLAMPED; requestedAbsolute=300; passedAbsolute=302; direction=NPC->player; npcId=UdlliE; GetIntim(int)=302
AffinityProtection: clamped decreasing SetIntim (total 1)
NUMERIC PROTECTED SetIntim AFTER; direction=NPC->player; npcId=UdlliE; GetIntim(int)=302
NUMERIC PROTECTED AddIntim AFTER; direction=NPC->player; npcId=UdlliE; GetIntim(int)=302
```

**Interpretation:** This is an observed decrease interception during a session that included month skips: the integer getter advanced from 300 to 302; a later requested absolute value of 300 was clamped to 302; the subsequent getter remained 302. A positive affinity write was also permitted. The log confirms the code classified this pair as protected but does not associate `UdlliE` with the name Na Zhen. It does NOT independently establish that the clamped write specifically originated from the monthly tick, that every monthly decrease was caught, or that the stored fractional float is unchanged. No further numerical lines or errors from this session were provided; absence in the filtered output is not a full-log error audit. The different-direction screenshots cannot be compared as before/after values.

## Procedure for any later confirmation (backup save)

1. Fully exit Tale of Immortal. Keep the earlier in-game Local Mod disabled/removed; never run two affinity patch providers together. Back up the DLL and save.
2. Pull main, build the standalone project, and replace the DLL only after a successful build with the game closed.
3. Capture a partner's baseline hearts in **both** `Your Affinity` and `Target's Affinity` views, and capture the SAME direction after a precisely noted month transition.
4. Check `MelonLoader/Latest.log` for `AffinityProtection: NUMERIC PROTECTED`, `blocked negative AddIntim`, `clamped decreasing SetIntim`, `relation lookup failed`, `numeric affinity readout failed`, `trace limit reached`, `Exception` and `Error`.
5. Keep private logs/screenshots/saves in chat; do not commit them or private paths to this public repo.

## Unresolved items

- `GetIntim(int)` is not the exact underlying float; the 301.5-to-302 readout highlights why float precision matters. Current `SetIntim` compares/clamps using an integer getter, potentially affecting fractions.
- The diagnostic identifies a relationship as protected using `IsProtectedPair`, but names are not mapped to IDs. Do not assert `UdlliE` is Na Zhen without verifying.
- Logs are event-driven writer traces, not complete relationship snapshots at month boundaries; there may be other write paths not patched.
- Test independent monthly/yearly changes in BOTH directions and save/reload before claiming comprehensive protection; preserve normal relationship breakup/removal.
- The focused logger limits detailed numeric output to 240 lines/session; protection still runs after logging reaches the limit.
