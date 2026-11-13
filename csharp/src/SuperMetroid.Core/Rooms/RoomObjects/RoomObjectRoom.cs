using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room a room-object population is installed into (#633): the level they edit, the system state that owns their bits,
/// and the area.
/// </summary>
[RoomLifetime]
public sealed record RoomObjectRoom
{
    /// <summary>Creates the room binding; every member is required.</summary>
    public RoomObjectRoom(RoomLevelData level, SharedGameState system, AreaId areaIndex)
    {

        Level = Ensure.NotNull(level);
        System = Ensure.NotNull(system);
        AreaIndex = areaIndex;
    }


    /// <summary>Room collision and block data room objects edit.</summary>
    public RoomLevelData Level { get; }

    /// <summary>System state owning item, door and event bits.</summary>
    public SharedGameState System { get; }

    /// <summary>The room's area.</summary>
    public AreaId AreaIndex { get; }
}
