using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>What one spawn of a Chozo statue's room object acts on (#2571), handed to the object as it is created and set up.</summary>
/// <param name="Statue">The statue's room objects, which the Lower Norfair hand reports to.</param>
/// <param name="Level">The room's blocks.</param>
/// <param name="Block">The object's block.</param>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly record struct ChozoStatueObjectSpawn(ChozoStatueObjects Statue, RoomLevelData Level, Position16<RoomBlocks> Block);
