using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="SaveRamLayout"/> that only verification reads.</summary>
internal static class SaveRamLayoutConstants
{
    /// <summary>Number of saved controller-button configuration words in each slot.</summary>
    public const int ButtonConfigWordCount = 11;
    /// <summary>Size of the unassigned $7E:D8F0-$7E:D8F7 allocation.</summary>
    public const int ProgressionPaddingByteCount = 8;
    /// <summary>Unassigned eight-byte SRAM-mirror allocation at $7E:D8F0-$7E:D8F7.</summary>
    public const int ProgressionPaddingWramAddress = OpenedDoorBitsWramAddress + Bank80SystemState.DoorBitByteCount;
    /// <summary>Offset of the selected-slot complement immediately after its stored word.</summary>
    public const int SelectedSlotComplementOffset = SaveRamLayout.SelectedSlotOffset + WordByteCount;
    /// <summary>Byte width of one little-endian SRAM layout word.</summary>
    public const int WordByteCount = 2;
    /// <summary>The SNES CPU-bus bank through which the cartridge exposes battery-backed SRAM.</summary>
    public const byte SramBank = 0x70;
    /// <summary>Mask that wraps logical SRAM accesses into the cartridge's 8-KiB <c>$70:0000-$70:1FFF</c> window.</summary>
    public const ushort SramOffsetMask = 0x1fff;
    /// <summary>First persistent event byte at $7E:D820.</summary>
    public const int EventsWramAddress = 0x7ed820;
    /// <summary>First area-boss byte at $7E:D828.</summary>
    public const int BossBitsWramAddress = 0x7ed828;
    /// <summary>First collected-item byte at $7E:D870.</summary>
    public const int CollectedItemBitsWramAddress = 0x7ed870;
    /// <summary>First opened-door byte at $7E:D8B0.</summary>
    public const int OpenedDoorBitsWramAddress = 0x7ed8b0;
    /// <summary>First acquired-map-station byte at $7E:D908.</summary>
    public const int MapStationsWramAddress = 0x7ed908;

    /// <summary>Provides SRAM and WRAM layout offsets for checking save slots, progression, and control settings.</summary>
    extension(SaveRamLayout)
    {
        /// <inheritdoc cref="SaveRamLayoutConstants.SramBank"/>
        internal static byte SramBank => SaveRamLayoutConstants.SramBank;
        /// <inheritdoc cref="SaveRamLayoutConstants.SramOffsetMask"/>
        internal static ushort SramOffsetMask => SaveRamLayoutConstants.SramOffsetMask;
        /// <inheritdoc cref="SaveRamLayoutConstants.EventsWramAddress"/>
        internal static int EventsWramAddress => SaveRamLayoutConstants.EventsWramAddress;
        /// <inheritdoc cref="SaveRamLayoutConstants.BossBitsWramAddress"/>
        internal static int BossBitsWramAddress => SaveRamLayoutConstants.BossBitsWramAddress;
        /// <inheritdoc cref="SaveRamLayoutConstants.CollectedItemBitsWramAddress"/>
        internal static int CollectedItemBitsWramAddress => SaveRamLayoutConstants.CollectedItemBitsWramAddress;
        /// <inheritdoc cref="SaveRamLayoutConstants.OpenedDoorBitsWramAddress"/>
        internal static int OpenedDoorBitsWramAddress => SaveRamLayoutConstants.OpenedDoorBitsWramAddress;
        /// <inheritdoc cref="SaveRamLayoutConstants.MapStationsWramAddress"/>
        internal static int MapStationsWramAddress => SaveRamLayoutConstants.MapStationsWramAddress;
        /// <inheritdoc cref="SaveRamLayoutConstants.ButtonConfigWordCount"/>
        internal static int ButtonConfigWordCount => SaveRamLayoutConstants.ButtonConfigWordCount;
        /// <inheritdoc cref="SaveRamLayoutConstants.ProgressionPaddingByteCount"/>
        internal static int ProgressionPaddingByteCount => SaveRamLayoutConstants.ProgressionPaddingByteCount;
        /// <inheritdoc cref="SaveRamLayoutConstants.ProgressionPaddingWramAddress"/>
        internal static int ProgressionPaddingWramAddress => SaveRamLayoutConstants.ProgressionPaddingWramAddress;
        /// <inheritdoc cref="SaveRamLayoutConstants.SelectedSlotComplementOffset"/>
        internal static int SelectedSlotComplementOffset => SaveRamLayoutConstants.SelectedSlotComplementOffset;
        /// <inheritdoc cref="SaveRamLayoutConstants.WordByteCount"/>
        internal static int WordByteCount => SaveRamLayoutConstants.WordByteCount;
    }
}
