namespace SuperMetroid.Core.Frontend;

/// <summary>Cartridge definition tables consumed by the title-demo loaders.</summary>
public static class AttractDemoRomData
{
    /// <summary>$82:876C, DemoRoomData_pointers: four room-list pointers terminated by $FFFF.</summary>
    public const int RoomSetPointers = 0x82876c;
    /// <summary>$91:8885, DemoData_Pointers: four equipment/input-object list pointers.</summary>
    public const int EquipmentSetPointers = 0x918885;
    /// <summary>$91:89FD, DemoSamusSetup_Pointers: four Samus-initializer list pointers.</summary>
    public const int SamusSetupSetPointers = 0x9189fd;
    /// <summary>Number of physical entries in each retail demo-set pointer table.</summary>
    public const int SetCount = 4;
    /// <summary>LoadDemoRoomData's bank and eighteen-byte record stride.</summary>
    public const int RoomBank = 0x820000, RoomRecordBytes = 18;
    /// <summary>LoadDemoData's bank and sixteen-byte equipment record stride.</summary>
    public const int EquipmentBank = 0x910000, EquipmentRecordBytes = 16;
    /// <summary>CheckForNextDemo's room-list termination word.</summary>
    public const ushort EndOfSet = 0xffff;

    /// <summary>Word offsets in a DemoRoomData record at bank $82.</summary>
    public static class RoomFields
    {
        public const int Room = 0, Door = 2, DoorSlot = 4, CameraX = 6, CameraY = 8,
            SamusYFromTop = 10, SamusXFromCenter = 12, Duration = 14, Setup = 16;
    }

    /// <summary>Word offsets in a DemoSetDef record at bank $91.</summary>
    public static class EquipmentFields
    {
        public const int Items = 0, Missiles = 2, SuperMissiles = 4, PowerBombs = 6,
            Health = 8, CollectedBeams = 10, EquippedBeams = 12, InputObject = 14;
    }
}
