using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>A room's terrain (#2553): its level and the room objects its reacting blocks reach.</summary>
internal sealed partial class RoomTerrain : SamusTerrain
{
    /// <summary>Creates the terrain of a room.</summary>
    /// <param name="level">The room's collision level.</param>
    /// <param name="roomObjects">The room's room objects, which its reacting blocks reach.</param>
    internal RoomTerrain(RoomLevelData level, ISamusRoomObjects roomObjects)
        : base(level) => RoomObjects = Ensure.NotNull(roomObjects);

    /// <summary>The room's room objects, which the touched blocks' reactions reach.</summary>
    internal ISamusRoomObjects RoomObjects { get; }

    /// <summary>A room's block reaches its room objects: the met block carries its reaction.</summary>
    /// <param name="standing">The block a met cell stands for.</param>
    private protected override MetBlock Met(StandingBlock standing) => new(standing, new ReactingBlock(this, standing));
}
