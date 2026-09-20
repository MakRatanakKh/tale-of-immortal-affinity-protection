# Gameplay test: positive affinity remains available

Date: September 20, 2026. Source: user's screenshots and report in chat (do not publish private saves or screenshots).

## Setup

- Standalone MelonLoader affinity mod installed; previous test confirmed `AddIntim` and `SetIntim` Harmony patches initialized.
- Ning Yue was already at maximum affinity, so she was unsuitable for detecting a gain. User switched to Yin'er.
- The Marked screen showed Yin'er with a visible value of **380** before the gift test. Note: this value appears under the eye icon; do not assume it is the exact raw bidirectional affinity field without further verification.

## Action and observation

The user offered Yin'er **Star's Powerful Mindbender Fist*1**. The subsequent screenshot displayed a green **"Yin'er Affinity ↑"** notification. This is positive evidence that an increase-producing interaction still fires while the protection mod is installed; it is consistent with the intended pass-through of positive deltas.

**Limit:** No numerical before/after reading of the *same affinity field* is available, and the heart display can hide small changes. Record as **UI positive-affinity event observed**, not as proof of an exact numerical increase or both-direction behavior. No new log of this specific gift was supplied.

## Remaining checks

1. Record an unambiguous numerical before/after value for a partner's affinity if possible; verify positive gains are actually persisted across a save/reload.
2. Using only an expendable backup save, trigger a negative interaction with an **unrelated NPC** and confirm its affinity still falls normally. Other mods can interfere; avoid changing the main save.
3. Test normal month/year transitions for an existing spouse and cultivation partner, in both affinity directions where observable.
4. Verify relationship removal/breakups are not blocked, and note the known fractional `SetIntim` limitation (`GetIntim` returns `int` while the setter stores `float`).

Current status: the earlier Ning Yue attack produced one blocked negative `AddIntim`, two clamped decreasing `SetIntim`, and no *visible* reduction in her displayed affinity. The Yin'er gift subsequently produced a positive-affinity UI indicator. These are promising targeted tests, not comprehensive verification.
