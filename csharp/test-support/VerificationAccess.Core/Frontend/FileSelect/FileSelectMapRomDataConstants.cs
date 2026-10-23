namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Cartridge values of <see cref="FileSelectMapRomData"/> that only verification reads.</summary>
internal static class FileSelectMapRomDataConstants
{
    /// <summary><c>$81:AAA0</c>, FileSelectMapArea_IndexTable: display-order to geographic-area mapping.</summary>
    public const int DisplayAreaIndices = 0x81aaa0;
    /// <summary>Four signed 16.16 edge velocities, stored low word then high word.</summary>
    public const int VelocityRecordBytes = 16;
    /// <summary><c>$81:AA94</c>, RoomSelectMap_ExpandingSquare_Timers: completion occurs on signed underflow.</summary>
    public const int WindowTimers = 0x81aa94;
    /// <summary><c>$81:AA34</c>, RoomSelectMap_ExpandingSquare_Velocities: four low/high pairs per area.</summary>
    public const int WindowVelocities = 0x81aa34;

    /// <summary>Provides file-select area indices and map-window timing and velocity records from the cartridge.</summary>
    extension(FileSelectMapRomData)
    {
        /// <inheritdoc cref="FileSelectMapRomDataConstants.DisplayAreaIndices"/>
        internal static int DisplayAreaIndices => FileSelectMapRomDataConstants.DisplayAreaIndices;
        /// <inheritdoc cref="FileSelectMapRomDataConstants.VelocityRecordBytes"/>
        internal static int VelocityRecordBytes => FileSelectMapRomDataConstants.VelocityRecordBytes;
        /// <inheritdoc cref="FileSelectMapRomDataConstants.WindowTimers"/>
        internal static int WindowTimers => FileSelectMapRomDataConstants.WindowTimers;
        /// <inheritdoc cref="FileSelectMapRomDataConstants.WindowVelocities"/>
        internal static int WindowVelocities => FileSelectMapRomDataConstants.WindowVelocities;
    }
    /// <summary>Menu BG3SC=$5C: area-background tilemap VRAM word base.</summary>
    public const ushort AreaBackgroundVram = 0x5c00;
    /// <summary>Menu BG34NBA=$04: BG3 character VRAM word base.</summary>
    public const ushort AreaBackgroundCharacters = 0x4000;
    /// <summary>BG12NBA=$33 selects the map and frame character base.</summary>
    public const ushort RoomCharacters = 0x3000;
}
