namespace SuperMetroid.Core.Runtime.Attract;

/// <summary>Cartridge values of <see cref="AttractDemoRomData"/> that only verification reads.</summary>
internal static class AttractDemoRomDataConstants
{
    /// <summary>CheckForNextDemo's room-list termination word.</summary>
    public const ushort EndOfSet = 0xffff;
    /// <summary>LoadDemoData's bank and sixteen-byte equipment record stride.</summary>
    public const int EquipmentBank = 0x910000;
    /// <summary>LoadDemoData's bank and sixteen-byte equipment record stride.</summary>
    public const int EquipmentRecordBytes = 16;
    /// <summary>$91:8885, DemoData_Pointers: four equipment/input-object list pointers.</summary>
    public const int EquipmentSetPointers = 0x918885;
    /// <summary>LoadDemoRoomData's bank and eighteen-byte record stride.</summary>
    public const int RoomBank = 0x820000;
    /// <summary>LoadDemoRoomData's bank and eighteen-byte record stride.</summary>
    public const int RoomRecordBytes = 18;
    /// <summary>$82:876C, DemoRoomData_pointers: four room-list pointers terminated by $FFFF.</summary>
    public const int RoomSetPointers = 0x82876c;
    /// <summary>$91:89FD, DemoSamusSetup_Pointers: four Samus-initializer list pointers.</summary>
    public const int SamusSetupSetPointers = 0x9189fd;

    /// <summary>Provides the demo set boundaries and record layouts needed to compare imported attract mode equipment, rooms, and Samus setup.</summary>
    extension(AttractDemoRomData)
    {
        /// <inheritdoc cref="AttractDemoRomDataConstants.EndOfSet"/>
        internal static ushort EndOfSet => AttractDemoRomDataConstants.EndOfSet;
        /// <inheritdoc cref="AttractDemoRomDataConstants.EquipmentBank"/>
        internal static int EquipmentBank => AttractDemoRomDataConstants.EquipmentBank;
        /// <inheritdoc cref="AttractDemoRomDataConstants.EquipmentRecordBytes"/>
        internal static int EquipmentRecordBytes => AttractDemoRomDataConstants.EquipmentRecordBytes;
        /// <inheritdoc cref="AttractDemoRomDataConstants.EquipmentSetPointers"/>
        internal static int EquipmentSetPointers => AttractDemoRomDataConstants.EquipmentSetPointers;
        /// <inheritdoc cref="AttractDemoRomDataConstants.RoomBank"/>
        internal static int RoomBank => AttractDemoRomDataConstants.RoomBank;
        /// <inheritdoc cref="AttractDemoRomDataConstants.RoomRecordBytes"/>
        internal static int RoomRecordBytes => AttractDemoRomDataConstants.RoomRecordBytes;
        /// <inheritdoc cref="AttractDemoRomDataConstants.RoomSetPointers"/>
        internal static int RoomSetPointers => AttractDemoRomDataConstants.RoomSetPointers;
        /// <inheritdoc cref="AttractDemoRomDataConstants.SamusSetupSetPointers"/>
        internal static int SamusSetupSetPointers => AttractDemoRomDataConstants.SamusSetupSetPointers;
    }
}
