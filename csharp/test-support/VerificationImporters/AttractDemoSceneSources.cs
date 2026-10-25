namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>Cartridge locations, banks and record strides of the title-demo scene tables the scene importer reads.</summary>
internal static class AttractDemoSceneSources
{
    /// <summary>CheckForNextDemo's room-list termination word.</summary>
    public const ushort EndOfSet = 0xffff;
    /// <summary>LoadDemoData's equipment and Samus-setup record bank.</summary>
    public const int EquipmentBank = 0x910000;
    /// <summary>LoadDemoData's sixteen-byte equipment record stride.</summary>
    public const int EquipmentRecordBytes = 16;
    /// <summary>$91:8885, DemoData_Pointers: four equipment/input-object list pointers.</summary>
    public const int EquipmentSetPointers = 0x918885;
    /// <summary>LoadDemoRoomData's room record bank.</summary>
    public const int RoomBank = 0x820000;
    /// <summary>LoadDemoRoomData's eighteen-byte room record stride.</summary>
    public const int RoomRecordBytes = 18;
    /// <summary>$82:876C, DemoRoomData_pointers: four room-list pointers terminated by $FFFF.</summary>
    public const int RoomSetPointers = 0x82876c;
    /// <summary>$91:89FD, DemoSamusSetup_Pointers: four Samus-initializer list pointers.</summary>
    public const int SamusSetupSetPointers = 0x9189fd;
}
