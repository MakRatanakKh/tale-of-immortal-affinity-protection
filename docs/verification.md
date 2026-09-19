# Verification checklist (before coding the patch)

## Why the discovery step matters

The game stores affinity in **two directions**: NPC → player and player → NPC. A fix that patches only one direction, only the UI, or only January's yearly decay is incomplete. The exact names/signatures of the mutation method(s) and the spouse/partner relationship flags **have not been verified** for game build `v1.2.113.259`.

The previous artifact-durability experiment compiled but its expected runtime patch log was missing and durability continued to decrease. We will require runtime evidence this time before calling a build working.

## Local discovery, no game DLL redistribution

In your game installation, inspect `MelonLoader/Il2CppAssemblies/Assembly-CSharp.dll` with a .NET assembly browser (for example ILSpy) or your existing tooling. Do not upload proprietary DLLs to this public repository.

Find and report the **declaring type, full method signature and relevant member types** for:

1. NPC ↔ NPC affinity read and write, including methods for changing an existing affinity by a delta.
2. The player character's ID and the NPC's ID, so we can recognize either direction.
3. Current spouse and cultivation-partner relationship checks. Verify that former partners are excluded.
4. The January/yearly decay path and the ordinary interaction path; determine whether each calls the same affinity writer.
5. Relationship breakup and death handling (particularly zero resets).

Look for likely strings in English and Chinese such as `favor`, `affinity`, `goodwill`, `relation`, `intimacy`, `好感`, `关系`, `道侣`; these are **search hints, not verified API names**. A decompiler may show generated IL2CPP wrappers rather than original method bodies; runtime logs can confirm actual invocation.

Share the *text* of the relevant decompiled declarations or screenshots of signatures and types. Include no game DLL binaries and no save files with private information.

## Implementation acceptance criteria

- Patch installation logs the exact resolved method(s), full signature and patch count; missing method means an explicit error, not a silent success.
- In each direction, **negative affinity deltas** are suppressed only for active spouse/partner relations; zero and positive changes pass through.
- Unrelated NPC changes are unmodified.
- Normal breakup/removal must continue; its reset behavior must be considered explicitly, rather than accidentally making relationships unbreakable.
- Logs do not spam on every NPC every frame.
- Build and packaging steps are reproducible and do not include game proprietary binaries.

## In-game testing (backup save first)

Record exact affinity values or hearts in *both directions* before and after: a month/year rollover; an event that normally decreases affinity; a gift that increases affinity; an unrelated NPC negative interaction; a breakup; save/reload. A change in hearts alone may hide small changes, so prefer exact numerical displays if available. Keep the raw `Player.log` patch-installation lines for troubleshooting.
