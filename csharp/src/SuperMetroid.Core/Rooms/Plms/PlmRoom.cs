using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The room a PLM population is installed into (#633): the level they edit, VRAM, the system state that owns their
/// bits, and the area.
/// </summary>
[RoomLifetime]
public sealed record PlmRoom
{
    /// <summary>Creates the room binding; every member is required.</summary>
    public PlmRoom(RoomLevelData level, SnesVram vram, Bank80SystemState system, AreaId areaIndex)
    {

        Level = Ensure.NotNull(level);
        Vram = Ensure.NotNull(vram);
        System = Ensure.NotNull(system);
        AreaIndex = areaIndex;
    }


    /// <summary>Room collision and tilemap data PLMs edit.</summary>
    public RoomLevelData Level { get; }

    /// <summary>VRAM PLM graphics load into.</summary>
    public SnesVram Vram { get; }

    /// <summary>System state owning item, door and event bits.</summary>
    public Bank80SystemState System { get; }

    /// <summary>The room's area.</summary>
    public AreaId AreaIndex { get; }
}
