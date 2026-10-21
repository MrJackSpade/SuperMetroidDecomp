namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>Cartridge addresses and pointers consumed by the pause-menu renderer.</summary>
internal static class PauseMenuRomData
{

    /// <summary>Pause-screen equipment tilemap at $B6:E800.</summary>
    public const int EquipmentTilemap = 0xb6e800;

    /// <summary>$82:C06C: beam item offset table.</summary>
    public const int BeamOffsetTable = 0x82c06c;

    /// <summary>$82:C08C: beam label tilemap pointer table.</summary>
    public const int BeamTilemapPointerTable = 0x82c08c;

    /// <summary>$82:C076: suit and miscellaneous item offset table.</summary>
    public const int SuitOffsetTable = 0x82c076;

    /// <summary>$82:C096: suit and miscellaneous label tilemap pointer table.</summary>
    public const int SuitTilemapPointerTable = 0x82c096;

    /// <summary>$82:C082: boot item offset table.</summary>
    public const int BootOffsetTable = 0x82c082;

    /// <summary>$82:C0A2: boot label tilemap pointer table.</summary>
    public const int BootTilemapPointerTable = 0x82c0a2;
}
