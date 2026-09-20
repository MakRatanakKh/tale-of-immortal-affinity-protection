# Gameplay test: positive-affinity notification observed

Date: September 20, 2026. Source: user's screenshots and corrections in chat (do not publish private saves or screenshots).

## Setup and correction

- Standalone MelonLoader affinity mod installed; a previous test confirmed `AddIntim` and `SetIntim` Harmony patches initialized.
- Ning Yue was already at maximum affinity, so she was unsuitable for detecting an increase. User switched to Yin'er.
- **Correction:** The `380` shown beneath Yin'er's portrait on the game's **Marked** screen is a **Focus cost for marking/tracking the character's location**, according to the user. It is **not an affinity reading**. Our earlier note and chat message that treated 380 as starting affinity were incorrect. The screen also has a `Max Focus` meter. Do not use that number as an affinity baseline or compare it against affinity values.

## Action and observation

The user offered Yin'er **Star's Powerful Mindbender Fist ×1**. A subsequent screenshot displayed a green **"Yin'er Affinity ↑"** notification. This supports that the positive-affinity interaction was triggered while the protection mod was installed; it is consistent with the intended pass-through of positive changes.

**Limit:** Neither a valid numerical affinity baseline nor a numerical after-value was supplied. A notification alone does not establish the exact amount added, that a gain persisted after save/reload, or that both affinity directions increased. Record this as **positive-affinity UI notification observed**, not as a numeric gain confirmed.

## Remaining checks

1. If the game or an inspection tool provides an unambiguous numeric affinity field, compare the same field before and after a positive interaction, and check persistence after save/reload on a backup save. Do not use the Marked screen's Focus costs.
2. Using only an expendable backup save, trigger a negative interaction with an **unrelated NPC** and check whether their affinity still falls normally; other mods may interfere.
3. Test normal month/year transitions for a current spouse and cultivation partner, in both directions where values can be observed.
4. Verify that relationship removal/breakups are not blocked, and address the known fractional `SetIntim` limitation (`GetIntim` returns `int` but the setter receives `float`).

Current status: an earlier Ning Yue attack coincided with one blocked negative `AddIntim`, two clamped decreasing `SetIntim`, and no *visible* reduction in her displayed affinity. The Yin'er gift subsequently produced a positive-affinity UI indicator. These are targeted but incomplete tests; no exact numerical affinity increase has been verified.
