# Confirmed assembly metadata (2026-09-19)

Input: user's `affinity-inventory.txt`, produced from `Assembly-CSharp.dll` (42,405,376 bytes), SHA-256 `52EC0CEFDB268002FCEF167B5173D4835A4B74BEC83FB6BA6EAEF29494E60136`.

**What this proves:** declarations and signatures in this particular generated assembly. **What it does not prove:** whether the running game executes the associated wrappers, which changes bypass them, or whether the patch is safe during breakups.

## Primary affinity writer (metadata token 0x06026933)

`RelationData` (nested public type; external reversing references identify its enclosing type as `DataUnit`, which still needs runtime confirmation):

```csharp
void AddIntim(string unitID, float value, int clampNewIntim, string uiTip, bool isTip);
void AddHate(string unitID, float value, int clampNewIntim, string uiTip, bool isTip);
void ClearIntim(string unitID);
void SetIntim(string unitID, float value);
int GetIntim(string unitID);
bool IsRelation(string toUnitID, UnitRelationType type);
string unitID { get; set; }
float intimToPlayerUnit { get; set; }
Il2CppSystem.Collections.Generic.Dictionary<string, float> intimToUnit { get; set; }
string married { get; set; }
Il2CppSystem.Collections.Generic.List<string> lover { get; set; }
```

`AddIntim` token 0x06026933 and `SetIntim` token 0x06026936, `GetIntim(string)` token 0x06026938, `IsRelation(string, UnitRelationType)` token 0x0602693E. The property types are inferred from the getter/field signatures and should be checked in the actual compiler.

## Relationship enum (metadata token 0x02001237)

`Il2Cpp.UnitRelationType`: `None=0`, `Parent=1`, `Children=2`, `ChildrenPrivate=3`, `Brother=4`, `ParentBack=5`, `ChildrenBack=6`, `BrotherBack=7`, `Married=8`, `Lover=9`, `Master=10`, `Student=11`.

The separate `Il2Cpp.UnitBothRelationType` enum uses **different** values: `Married=5`, `Lover=6`; never mix these two enum types or their integer values.

## Initial implementation

`ModCode/ModMain/ModMain.cs` targets precisely the `AddIntim(string,float,int,string,bool)` and `SetIntim(string,float)` overloads. It only acts when one side is the player and the other is a current `Married` or `Lover` relation according to the player's relationship data; it leaves positive changes and unrelated NPCs alone. `ClearIntim` is **not patched**, and breakup/reset behavior remains unverified.

**Known limitation:** `GetIntim(string)` returns `int` even though stored affinity/setter use `float`. The prototype's direct-set guard can therefore miss sub-integer decreases. A future version should read the stored float carefully once its runtime representation is verified. The game and the mod cannot be executed in this workspace, so no build/game-test success is claimed.
