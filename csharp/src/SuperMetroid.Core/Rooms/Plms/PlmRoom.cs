using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The room a PLM population is installed into (#633): the address space PLM programs read, the
/// level and BG1 streamer they edit, VRAM, the system state that owns their bits, and the area.
/// </summary>
public sealed record PlmRoom
{
    /// <summary>Creates the room binding; every member is required.</summary>
    public PlmRoom(RoomLevelData level, BackgroundTilemapStreamer streamer, SnesVram vram,
        Bank80SystemState system, AreaId areaIndex)
    {

        Level = Ensure.NotNull(level);
        Streamer = Ensure.NotNull(streamer);
        Vram = Ensure.NotNull(vram);
        System = Ensure.NotNull(system);
        AreaIndex = areaIndex;
    }


    /// <summary>Room collision and tilemap data PLMs edit.</summary>
    public RoomLevelData Level { get; }

    /// <summary>BG1 streamer PLM tile writes go through.</summary>
    public BackgroundTilemapStreamer Streamer { get; }

    /// <summary>VRAM PLM graphics load into.</summary>
    public SnesVram Vram { get; }

    /// <summary>System state owning item, door and event bits.</summary>
    public Bank80SystemState System { get; }

    /// <summary>The room's area.</summary>
    public AreaId AreaIndex { get; }
}
