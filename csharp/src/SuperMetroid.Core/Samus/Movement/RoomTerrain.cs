using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>A room's terrain (#2553): its level and the room objects its reacting blocks reach.</summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal sealed class RoomTerrain : SamusTerrain
{
    /// <summary>Creates the terrain of a room.</summary>
    /// <param name="level">The room's collision level.</param>
    /// <param name="roomObjects">The room's room objects, which its reacting blocks reach.</param>
    internal RoomTerrain(RoomLevelData level, ISamusRoomObjects roomObjects)
        : base(level) => RoomObjects = Ensure.NotNull(roomObjects);

    /// <summary>The room's room objects, which the touched blocks' reactions reach.</summary>
    internal ISamusRoomObjects RoomObjects { get; }
}
