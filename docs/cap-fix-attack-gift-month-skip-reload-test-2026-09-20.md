# Cap-aware affinity gameplay and reload test (2026-09-20)

Status: user-reported gameplay test of the installed cap-aware v0.2.4 build; this documents observations, not proof that the earlier crashes were caused by the prior over-cap issue.

## Scenario

User reports attacking a partner, gifting her a manual, advancing several months, closing the game, and successfully loading the save again with no crash in this test. Exact month count and whether every relevant write occurred in the selected log excerpt are unknown. The log excerpt below was captured after closing the game and before reloading the save; thus it does not independently document the reload session.

## Evidence from selected MelonLoader log

All captured protected writes shown are NPC -> player for NPC ID `UdlliE`. The log does not independently map the ID to an NPC name.

- 12:32:52: validated raw store `intimToPlayerUnit`; previous raw 302. Requested 303.625 was changed to 300 (`CAP NORMALIZED`); subsequent `AFTER` read reported raw 300.
- 12:33:41: requested 296.25 from current raw 300; `DECAY BLOCKED`, passed 300; subsequent raw 300.
- 12:33:41: requested 299 from current raw 300; `DECAY BLOCKED`, passed 300; subsequent raw 300.
- 12:35:57 and 12:36:12: positive requests 303.75 and 301 were `CAP LIMITED` to 300; subsequent reads reported raw 300.
- The user's `Select-String` filter searched for `Exception|Error` along with cap/read events; no matching exception/error line appeared in the supplied excerpt. This is not a comprehensive crash-log inspection.

## Interpretation and remaining checks

The cap policy behaves as intended for these NPC -> player operations: an existing value above 300 returned to 300, subsequent increases were bounded, and two attempted decreases were rejected. The user reports that the attack/gift/month-skip/reload test finished without crashing. Earlier crashes while interacting with max-affinity partners are still not causally linked to the affinity cap; no pre-crash stack traces are available here.

The opposite player -> NPC direction, exact float behavior when raw reads fail, and longer-term stability are not fully verified. Keep backup DLL and saves until ordinary usage establishes stability. If another crash occurs, preserve `MelonLoader/Latest.log`, Unity `Player.log` and any crash dump before another launch (logs can be overwritten), along with time and interaction; do not commit private saves or raw logs to this public repository.

No source-code changes in this documentation commit.