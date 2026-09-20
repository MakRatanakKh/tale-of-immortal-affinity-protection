# Gameplay test: unrelated NPC hostility is not frozen

Date: September 20, 2026. Source: screenshots supplied privately in chat. Do not upload screenshots, private saves, or logs to this public repository.

## Setup and action

- Standalone Partner Affinity Protection mod installed; prior game session demonstrated Harmony affinity patch installation and interception for a partner.
- Selected **Wan Deye**, whose profile initially identified him as **Stranger**; not an identified spouse or cultivation partner.
- Attacked him in-game on the user's test session.

## Observations supported by screenshots

- The attack sequence displayed **Wan Deye Hatred ↑↑↑**; this is a *hatred* notification, not an affinity notification.
- A following screen showed **Wan Deye Reputation -500**.
- His relationship display changed from **Stranger** to multiple hostile flame icons; his profile afterward showed hostile flames. These are visual evidence that the unrelated NPC's hostility/relation state could change while the standalone protection was installed.
- No numeric before/after affinity values were shown, and no affinity-specific decrease notification or MelonLoader log for this attack was supplied. Thus this test **does not establish that the unrelated NPC's affinity numerically decreased** or that every unrelated affinity writer works.

## Result

**Observed:** unrelated NPC hatred and hostility changed normally in this example; the mod did not freeze all relationship systems.

**Not verified:** numerical unrelated-NPC *affinity decrease*, absence of Harmony intercepts for this NPC, both directions of partner affinity, seasonal/yearly decay, breakups, fractional setter changes, and persistence after save/reload.

Next useful check: on an expendable save, capture an unambiguous affinity decrease for a nonpartner (not only hatred), or inspect game logs and corresponding raw affinity values. Avoid treating the Marked screen's eye-icon Focus costs as affinity numbers.
