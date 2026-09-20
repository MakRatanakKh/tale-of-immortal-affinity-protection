# Month-skip gameplay test — 2026-09-20

Source: user-provided MelonLoader terminal excerpt and two screenshots in chat. Do not publish the screenshots, user saves, local paths or full private logs.

## Setup and observation

- User switched to another save, advanced several months, and reported no *noticeable* drop in their cultivation partner's affinity.
- Screenshots show the relationship/family view for Han Li and partner Na Zhen, with `Target's Affinity` checked and five visible hearts for Na Zhen in both images. Hearts are coarse; no exact affinity numeric before/after comparison or ID-to-name mapping is established.
- `NUMERIC` traces in this build only show player-related writes and integer `GetIntim`, capped at 120 diagnostic lines per session. The cap was reached at `11:04:30`; later events are **not covered by numerical lines**, but the cap does not disable the affinity patches themselves.

## Concrete logged evidence

- At `11:02:10`, `npcId=UdlliE`, `direction=NPC->player`: `AddIntim requestedDelta=-39` with `GetIntim(int)=300` was logged `BLOCKED`, and its `AFTER` getter was still `300`. This establishes that a negative operation was intercepted during the test. The log does **not** resolve `UdlliE` to Na Zhen or any named character.
- At `11:02:10` and `11:02:11`, several other NPC IDs show allowed rises or falls, indicating that affinity processing for player-related, non-protected records continues. Example `npcId=eRrRrA`: `player->NPC` getter changed `153 -> 149` and `NPC->player` getter changed `298 -> 296` in logged operations. **Do not claim this NPC was or was not Na Zhen** without an ID match; if it is an active partner, investigate relationship filtering.
- At `11:04:11`, `npcId=tL8dbo` in both directions had an allowed positive `AddIntim requestedDelta=36`, intermediate getter `72`, then an absolute `SetIntim requestedAbsolute=60` with getter `72 -> 60`; net visible sequence in this excerpt starts at `36` and ends at `60`. A lower intermediate value does not alone prove net affinity decay, but the relation identity and reason for absolute writes deserve investigation if this is an active partner.
- Other examples include `npcId=LN05Ck` `NPC->player 25 -> 15` from a negative delta and `npcId=Fef5Qa` `NPC->player 146 -> 137`; the test lacks reliable mapping from these IDs to names/relationships.

## Interpretation and next steps

This is encouraging evidence for a **specific negative write being blocked** and no visible change on the displayed partner screen, but it is not comprehensive proof of protection across month/year changes or both directions. No evidence currently identifies which NPC ID is Na Zhen; avoid asserting `UdlliE` is hers or that the observed allowed decreases represent a bug. `GetIntim` is integer-only, while `SetIntim` accepts float; fractional changes remain a known precision limitation.

For a more decisive follow-up on a *backup save*, consider a focused diagnostic iteration that resolves partner IDs safely, takes both-direction snapshots of current partner integer affinity at well-defined points before and after a time skip, and prioritizes protected-pair and negative events within a revised bounded log budget. Keep logging read-only and avoid changing patch behavior until the data is clear. If an allowed drop maps to a current partner, inspect why `IsProtectedPair` returned false or whether an alternative writer was involved. Test save/reload separately.
