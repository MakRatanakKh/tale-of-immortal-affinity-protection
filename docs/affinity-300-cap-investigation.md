# Investigation: 300 affinity cap vs. protection clamp (2026-09-20)

## Findings

**High-confidence game-design cap, not yet a bytecode-confirmed implementation detail:** Public Tale of Immortal guides describe NPC affinity as ranging from -300 to +300, with five hearts representing +300. A December 2024 TapTap guide independently describes 300 as maximum. These sources discuss normal gameplay; they do not establish precisely how every method in the user's 2026 build enforces the cap.

Sources:
- https://www.gamersky.com/handbook/202103/1368709.shtml (2021 guide explicitly says range [-300,300]).
- https://www.taptap.cn/moment/619323840203327612 (2024-12-20 guide says 5 hearts = 300 maximum).
- https://tale-of-immortal.fandom.com/wiki/NPC (wiki says 5 hearts = 300 maximum).
- https://tale-of-immortal.fandom.com/wiki/Affinity_and_Gift (wiki describes gifts reaching 300 maximum).

**Observed in user-supplied 0.2.3-test runtime log, NPC -> player, ID UdlliE:**
1. `SetIntim BEFORE; requestedAbsolute=303.375; ... raw=302` followed by `AFTER ... raw=303.38` (logger formats output; the requested raw float is 303.375).
2. Immediately afterwards: `SetIntim BEFORE; requestedAbsolute=300; ... raw=303.38`.
3. Our prefix: `SetIntim CLAMPED; requestedAbsolute=300; passedAbsolute=303.38; guard=intimToPlayerUnit`.
4. `AFTER ... raw=303.38`. Later requested 299.25 and 302 were also blocked, with raw remaining around 303.38.

**Confirmed code issue:** `ModCode/ModMain/ModMain.cs` current `SetIntimPrefix` clamps *every* requested value below the current validated raw float, including an attempted return from above 300 down to 300. The earlier 0.2.2 integer-based guard shared the issue whenever the integer getter exceeded 300. This prevents a game-issued write to normalize an over-cap value. It is plausible repeated positive events can keep an out-of-range affinity above 300. The exact call stack and reasons for the game-requested 300 have NOT been established by decompilation; 300 may be a clamp/normalization or another game mechanic.

**What is not established:** No evidence so far of a crash, save corruption, infinite value growth, or a precise limit in this build's `SetIntim` code. Do not claim these outcomes. The fact that +300 is the normal-gameplay range does not prove every out-of-range write is unsafe.

## Proposed cap-aware behavior — NOT IMPLEMENTED

On both current player -> NPC and NPC -> player protected relations, handle cap-aware `SetIntim` *only after* a validated raw read (otherwise clearly log fallback/uncertainty):
- Reject/prevent positive writes above the normal +300 limit, rather than temporarily allowing over-cap increases to become the effective value.
- If current raw affinity is <=300, preserve it against a lower request using the exact float; still permit normal positive changes up to 300.
- If current raw affinity is >300, allow normalization down to 300. For a request below 300, use 300 rather than allowing the value to fall below the cap. Keep logs distinguishing CAP NORMALIZED, CAP LIMITED, and DECAY BLOCKED.
- Never touch unrelated NPCs, `ClearIntim`, or the negative bound. Preserve the `Married`/`Lover` filter.
- Be careful about temporary intermediate writes inside `AddIntim`: the current runtime log shows a >300 `SetIntim` followed immediately by another `SetIntim(300)`. Confirm the cap-aware intervention does not change other expected side effects.

Potential edge case: if a particular game event intentionally writes >300, this change would alter it; normal-gameplay sources make that less likely but cannot exclude it. Do not silently ship the change as verified.

## Safe validation plan

1. Save copies of the last working DLL and a disposable game save; close the game before installation.
2. Build and install the proposed cap-aware change only after implementation and successful compilation.
3. On a partner with affinity just below 300, verify fractional gains still occur, but the raw value does not finish over 300 after the game interaction.
4. If a backup save already has affinity above 300, trigger a benign interaction and check `SetIntim` ends at 300; verify an attempted loss from below 300 is still blocked.
5. Repeat in both directions and verify unrelated NPC loss still works; inspect MelonLoader for errors and perform save/reload test.
6. Until validated, the existing 0.2.3 DLL remains unchanged in the repo; for cautious play avoid repeatedly increasing an already maxed relationship, and maintain backups. Removing the mod is the way to let the game resume its ordinary affinity writes if over-cap accumulation is a concern.

**Status:** Investigation documented; no code or gameplay fix has been released.