namespace SuperMetroid.Core.Game;

/// <summary>Native SRAM directory and per-slot payload layout used by bank <c>$81</c>.</summary>
/// <remarks>
/// Values in this catalog are byte offsets unless a summary explicitly calls out a CPU-bus
/// address. The serializer owns behavior; this type owns only the immutable cartridge data
/// structure so layout review does not require reading save/load control flow.
/// </remarks>
public static class SaveRamLayout
{
    public const int SlotCount = 3;
    public const int SlotByteCount = 0x065c;

    public const int PrimaryChecksumOffset = 0x0000;
    public const int PrimaryComplementOffset = 0x0008;
    public const int SelectedSlotOffset = 0x1fec;
    public const int SelectedSlotComplementOffset = SelectedSlotOffset + WordByteCount;
    public const int BackupChecksumOffset = 0x1ff0;
    public const int BackupComplementOffset = 0x1ff8;

    public const int WordByteCount = 2;
    public const byte SramBank = 0x70;
    public const ushort SramOffsetMask = 0x1fff;

    public const int EquippedItemsOffset = 0x0000;
    public const int CollectedItemsOffset = 0x0002;
    public const int EquippedBeamsOffset = 0x0004;
    public const int CollectedBeamsOffset = 0x0006;
    public const int ButtonConfigOffset = 0x0008;
    public const int ButtonConfigWordCount = 11;
    public const int ShootButtonOffset = ButtonConfigOffset + 8;
    public const int JumpButtonOffset = ButtonConfigOffset + 10;
    public const int DashButtonOffset = ButtonConfigOffset + 12;
    public const int CancelButtonOffset = ButtonConfigOffset + 14;
    public const int SelectButtonOffset = ButtonConfigOffset + 16;
    public const int AimDownButtonOffset = ButtonConfigOffset + 18;
    public const int AimUpButtonOffset = ButtonConfigOffset + 20;
    public const int ReserveModeOffset = 0x001e;
    public const int HealthOffset = 0x0020;
    public const int MaxHealthOffset = 0x0022;
    public const int MissilesOffset = 0x0024;
    public const int MaxMissilesOffset = 0x0026;
    public const int SuperMissilesOffset = 0x0028;
    public const int MaxSuperMissilesOffset = 0x002a;
    public const int PowerBombsOffset = 0x002c;
    public const int MaxPowerBombsOffset = 0x002e;
    public const int HudItemOffset = 0x0030;
    public const int MaxReserveEnergyOffset = 0x0032;
    public const int ReserveEnergyOffset = 0x0034;
    public const int GameTimeFramesOffset = 0x0038;
    public const int GameTimeSecondsOffset = 0x003a;
    public const int GameTimeMinutesOffset = 0x003c;
    public const int GameTimeHoursOffset = 0x003e;
    public const int MoonwalkOffset = 0x0042;
    public const int DebugFlagOffset = 0x0044;
    public const int NewFileMarkerOffset = 0x0046;
    public const int IconCancelOffset = 0x0048;
    public const int EventsOffset = 0x0060;
    public const int BossBitsOffset = 0x0068;
    public const int RoomChozoBitsOffset = 0x0070;
    public const int CollectedItemBitsOffset = 0x00b0;
    public const int OpenedDoorBitsOffset = 0x00f0;
    public const int UsedSaveStationsOffset = 0x0138;
    public const int MapStationsOffset = 0x0148;
    public const int SaveStationOffset = 0x0156;
    public const int AreaOffset = 0x0158;
    public const int CompressedMapDataOffset = 0x015c;
    public const int CompressedMapDataByteCount = 0x0500;
    public const int PackedMapAreaCount = 6;

    /// <summary>Bank-$81 byte-count table consumed by PackMapToSave/UnpackMapFromSave.</summary>
    public static readonly SnesAddress PackedMapByteCountTable =
        SnesAddress.FromUpperLoRom(0x81, 0x8131);

    /// <summary>Bank-$81 packed SRAM destination-offset word table.</summary>
    public static readonly SnesAddress PackedMapDestinationOffsetTable =
        SnesAddress.FromUpperLoRom(0x81, 0x8138);

    /// <summary>Bank-$81 pointer table for unpacked per-area map byte indexes.</summary>
    public static readonly SnesAddress PackedMapSourceIndexPointerTable =
        SnesAddress.FromUpperLoRom(0x81, 0x82d6);

    private static readonly ushort[] NativeSlotOffsets = [0x0010, 0x066c, 0x0cc8];

    /// <summary>Read-only starts of the three checksummed slot payloads.</summary>
    public static ReadOnlySpan<ushort> SlotOffsets => NativeSlotOffsets;
}
