# Assembly findings — first pass

Source: privately provided `Assembly-CSharp.dll`, inspected locally as a PE32 Mono/.NET assembly. **Do not commit or redistribute the DLL.** The presence of generated `NativeMethodInfoPtr_...` strings suggests an IL2CPP interop assembly; identifiers below are verified *as strings in the supplied binary*, but declaring types, original bodies, and runtime behavior are not yet verified.

## Affinity candidate methods found in binary

```text
NativeMethodInfoPtr_AddIntim_Public_Static_Boolean_WorldUnitBase_WorldUnitBase_Int32_0
NativeMethodInfoPtr_AddIntim_Public_Static_Void_WorldUnitBase_WorldUnitBase_Single_0
NativeMethodInfoPtr_AddIntim_Public_Void_String_Single_Int32_String_Boolean_0
NativeMethodInfoPtr_AddIntim_Public_Void_Int32_Int32_0
NativeMethodInfoPtr_AddIntim_Public_Void_Int32_0
NativeMethodInfoPtr_SetIntim_Public_Void_String_Single_0
NativeMethodInfoPtr_GetIntim_Public_Int32_String_0
NativeMethodInfoPtr_GetIntim_Public_Int32_WorldUnitBase_0
NativeMethodInfoPtr_GetRelation_Public_UnitRelationType_WorldUnitBase_0
NativeMethodInfoPtr_GetRelationType_Public_UnitBothRelationType_WorldUnitBase_0
NativeMethodInfoPtr_SetRelationUnit_Public_Static_Void_WorldUnitBase_WorldUnitBase_UnitRelationType_Int32_0
NativeMethodInfoPtr_UpdateAllUnitRelation_Public_Void_0
```

`AddIntim` is the primary **candidate** for delta modification, but not every overload necessarily changes NPC-player affinity. `SetIntim` might bypass any `AddIntim` patch; confirm call paths before coding. `UnitRelationType` / `UnitBothRelationType` may provide the required active spouse/partner filter, but enum values and directions need inspection.

## Next steps

Use a metadata-aware decompiler (ILSpy/dnSpy) on the local DLL and identify the declaring types and bodies of `AddIntim`, `SetIntim`, `GetIntim`, `GetRelation`, `GetRelationType`, and any `UpdateAllUnitRelation` call sites. Verify the game loader/runtime and method hooks against the actual installed build. No production patch or successful in-game test exists yet.
