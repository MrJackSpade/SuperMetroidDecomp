using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room a room-object population is installed into (#633, #1466): the level they edit, what they show over the room's load,
/// the system state that owns their bits, and the area.
/// </summary>
[RoomLifetime]
public sealed record RoomObjectRoom
{
    /// <summary>Creates the room binding; every member is required.</summary>
    public RoomObjectRoom(RoomLevelData level, RoomAppearance appearance, SharedGameState system, AreaId areaIndex)
    {

        Level = Ensure.NotNull(level);
        Appearance = Ensure.NotNull(appearance);
        System = Ensure.NotNull(system);
        AreaIndex = areaIndex;
    }


    /// <summary>Room collision and block data room objects edit.</summary>
    public RoomLevelData Level { get; }

    /// <summary>What the room's room objects show over its load, as a collectible shows in its graphics slot (#1466).</summary>
    public RoomAppearance Appearance { get; }

    /// <summary>System state owning item, door and event bits.</summary>
    public SharedGameState System { get; }

    /// <summary>The room's area.</summary>
    public AreaId AreaIndex { get; }
}
