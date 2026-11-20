using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The terrain Samus's movement, collision, grapple and projectiles meet (#633, #2553): a room's level, whose reacting
/// blocks reach the room's objects (<see cref="RoomTerrain"/>), or a plain terrain of air, solid blocks and slopes alone,
/// which holds no block that reacts (<see cref="PlainTerrain"/>). These two are the only terrains.
/// </summary>
[RoomScopeBoundary("A per-call input, built for one call and never kept.")]
internal abstract class SamusTerrain
{
    /// <summary>Creates the terrain of <paramref name="level"/>.</summary>
    /// <param name="level">The collision level the scans probe.</param>
    private protected SamusTerrain(RoomLevelData level) => Level = Ensure.NotNull(level);

    /// <summary>The collision level the scans probe.</summary>
    internal RoomLevelData Level { get; }

    /// <summary>The terrain of a room: <paramref name="level"/>, whose reacting blocks reach <paramref name="roomObjects"/>.</summary>
    /// <param name="level">The room's collision level.</param>
    /// <param name="roomObjects">The room's room objects, which its reacting blocks reach.</param>
    internal static RoomTerrain InRoom(RoomLevelData level, ISamusRoomObjects roomObjects) => new(level, roomObjects);
}
