using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room a room-object population is installed into (#633): the level they edit, the system state that records what they open and collect,
/// and the area.
/// </summary>
[RoomLifetime]
public sealed partial record RoomObjectRoom
{
    /// <summary>Creates the room binding; every member is required.</summary>
    public RoomObjectRoom(RoomLevelData level, SharedGameState system, AreaId area)
    {

        Level = Ensure.NotNull(level);
        System = Ensure.NotNull(system);
        Area = area;
    }


    /// <summary>Room collision and block data room objects edit.</summary>
    public RoomLevelData Level { get; }

    /// <summary>System state recording the items collected, the doors opened for good and the story events.</summary>
    public SharedGameState System { get; }

    /// <summary>The room's area.</summary>
    public AreaId Area { get; }
}
