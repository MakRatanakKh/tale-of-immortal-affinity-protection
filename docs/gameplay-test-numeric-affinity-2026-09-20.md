# Numeric affinity diagnostic test — September 20, 2026

Source: user-provided terminal transcript in chat. Do not publish their raw logs, save files, or screenshots. This document summarizes the observations without associating opaque NPC IDs with character names as a proven fact.

## Setup and actions

The user installed experimental standalone numeric logging, then attacked an unrelated stranger, fought and released him; subsequently attacked and humiliated current partner Ning Yue. The session's logger printed `NUMERIC` records from the patched `RelationData.SetIntim` method. Diagnostics log player-related writes, rather than every NPC-to-NPC interaction. `GetIntim` returns **int**, while `SetIntim` accepts **float**.

## Observations

- `npcId=iR0yik`, direction `NPC->player`: `SetIntim BEFORE; requestedAbsolute=-60; GetIntim(int)=0` followed by `SetIntim AFTER; GetIntim(int)=-60`. This is direct evidence that a player-related affinity decrease was permitted for this ID. Given the order of user actions it is consistent with the unrelated stranger, but the log does not map ID to the NPC's displayed name.
- `npcId=MCJkqx`, direction `NPC->player`: starting integer getter `300`; attempted `SetIntim` absolute values included `296.25` and `299`. Each shown attempt was `CLAMPED` to `passedAbsolute=300` and its corresponding `AFTER` getter remained `300`. This directly demonstrates protection of the **NPC->player direction** for this ID. Given the action order, consistent with Ning Yue, but no name mapping is logged.
- At approximately 10:50:04, repeated calls attempted the same decreasing absolute values in rapid succession. The diagnostic logger has a 120-numeric-line per-session cap. Repeated lines could reflect repeat game writes; they are not proof of distinct story events.

## Interpretation and limitations

This is stronger evidence for both *unprotected NPC->player decreases* and *protected NPC->player clamping* than the previous hearts/hatred observations. It does **not** verify the other direction (`player->NPC`) or seasonal/yearly decay, and it does not give exact raw float affinity. The getter reading `300` could conceal a stored fractional value.

**Important potential precision issue:** current `SetIntimPrefix` obtains `int current = relation.GetIntim(otherId)` and writes `value = current` when clamping. If the underlying stored float were e.g. 300.5, writing back 300 could itself lose the 0.5. Conversely, a fractional decrease that remains above the integer getter could evade clamping. Preserve the working DLL and do not claim fractional protection until verified. Future work: identify and safely read the underlying float field (`intimToUnit` or `intimToPlayerUnit` as appropriate) by direction, inspect runtime types, then correct clamp and log the actual raw value without altering unrelated game state; test with a backup save. Do not assume the integer getter equals the exact float.

## Next checks

1. Identify NPC IDs via a safe name lookup or narrowly isolated interaction before attaching names in diagnostics.
2. Test `player->NPC` direction explicitly, and season/year rollover with a backup save.
3. Determine raw float storage safely before changing the clamp. Verify positive increases, unrelated decreases, fractional transitions, and relationship removal after any change.

No gameplay code was modified as part of recording this result.
