using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The room terrain Samus's block collision probes: the level data and the room objects that react to touched blocks (#633).</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal readonly partial record struct SamusTerrain
{
    /// <summary>The room's collision level the scan probes.</summary>
    internal required RoomLevelData Level { get; init; }

    /// <summary>The room's room objects, which the touched blocks' reactions reach.</summary>
    internal required ISamusRoomObjects RoomObjects { get; init; }
}
