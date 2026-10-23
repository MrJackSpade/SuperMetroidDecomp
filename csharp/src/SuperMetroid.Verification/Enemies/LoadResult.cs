using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies;

/// <summary>A loaded enemy population, the OBJ characters its queued tile uploads produce, and its colours.</summary>
/// <param name="Enemies">The loaded enemy system.</param>
/// <param name="ObjCharacters">The OBJ character table after the room's enemy tile uploads land.</param>
/// <param name="Colors">The colours the load stages.</param>
internal readonly record struct LoadResult(RoomEnemySystem Enemies, byte[] ObjCharacters, SamusColourRig Colors);
