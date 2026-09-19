# Tale of Immortal — Partner Affinity Protection

**Status: research / scaffold; no functional affinity patch has been implemented or tested yet.**

Goal: prevent decreases in *both directions* of affinity between the player and their current spouse or cultivation partners (道侣), while allowing gains normally and leaving unrelated NPC relationships untouched. Removing a relationship should not be blocked; we need to confirm how the game represents that operation before deciding how to handle its affinity reset.

## Compatibility target

- Tale of Immortal / 鬼谷八荒, user's Steam build `v1.2.113.259` (verify after any game update).
- MelonLoader-based modding; project must use the actual dependencies and mod packaging expected by this game build. Not a BepInEx mod.
- Existing local tools: .NET SDK 10.0.401, `just` 1.58.0, Tale of Immortal Tool 0.6.1.

## Development plan

1. Inspect the exact IL2CPP generated `Assembly-CSharp.dll` for the relationship type, player identity, and affinity mutation methods. Check all overloads and the annual-decay path.
2. Implement a narrowly scoped Harmony patch with positive-gain pass-through, current spouse/partner filtering, and explicit logs for patch success/failure. Avoid patching by an unverified guessed method name.
3. Build and package the mod using the game's actual mod loader format; confirm the patch-installed log in `Player.log`.
4. Test on a **backup save**: annual decay, negative interaction, positive gift, unrelated NPC decrease, relationship removal, and reload.

See [`docs/verification.md`](docs/verification.md) for the discovery information needed before implementing a reliable patch.

No copyrighted game DLLs, save data, or personal local paths should be committed to this public repository.
