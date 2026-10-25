namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// Verification's native view of the file-select maps (#1416): the cartridge tables and video-memory bases only the ROM
/// oracles read. Core holds the decoded map-window motion and installed screens.
/// </summary>
internal static class FileSelectMapNativeSources
{
    /// <summary>Four signed 16.16 edge velocities, stored low word then high word.</summary>
    public const int VelocityRecordBytes = 16;
    /// <summary><c>$81:AA94</c>, RoomSelectMap_ExpandingSquare_Timers: completion occurs on signed underflow.</summary>
    public const int WindowTimers = 0x81aa94;
    /// <summary><c>$81:AA34</c>, RoomSelectMap_ExpandingSquare_Velocities: four low/high pairs per area.</summary>
    public const int WindowVelocities = 0x81aa34;
    /// <summary>Menu BG3SC=$5C: area-background tilemap VRAM word base.</summary>
    public const ushort AreaBackgroundVram = 0x5c00;
    /// <summary>BG12NBA=$33 selects the map and frame character base.</summary>
    public const ushort RoomCharacters = 0x3000;
}
