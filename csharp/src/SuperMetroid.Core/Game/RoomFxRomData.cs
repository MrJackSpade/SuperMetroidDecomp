using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Verified cartridge addresses, record fields, and fixed tables used by room FX.
/// </summary>
/// <remarks>
/// Room FX span banks $83, $87, $88, $89, $8A, and $8D. Keeping those values in one
/// catalog makes the bank boundaries explicit and prevents each consumer from inventing
/// its own offsets into the same sixteen-byte <c>FxDef</c> record.
/// </remarks>
public static class RoomFxRomData
{
    /// <summary>SNES banks that own the translated room-FX data and bytecode.</summary>
    public static class Banks
    {
        public const int RoomDefinitions = 0x830000;
        public const int AnimatedTiles = 0x870000;
        public const int EffectCode = 0x880000;
        public const int PaletteBlendData = 0x890000;
        public const int Tilemaps = 0x8a0000;
        public const int PaletteFx = 0x8d0000;
    }

    /// <summary>Layout of one bank-$83 <c>FxDef</c> entry.</summary>
    public static class Record
    {
        public const int ByteCount = 16;
        public const int DoorPointerOffset = 0;
        public const int BaseYPositionOffset = 2;
        public const int TargetYPositionOffset = 4;
        public const int YVelocityOffset = 6;
        public const int TimerOffset = 8;
        public const int TypeOffset = 9;
        public const int DefaultLayerBlendConfigurationOffset = 10;
        public const int Layer3LayerBlendConfigurationOffset = 11;
        public const int LiquidOptionsOffset = 12;
        public const int PaletteFxBitsetOffset = 13;
        public const int AnimatedTileBitsetOffset = 14;
        public const int PaletteBlendOffset = 15;

        /// <summary>Door word that terminates an FX list without selecting a record.</summary>
        public const ushort TerminatorDoorPointer = ushort.MaxValue;
    }

    /// <summary>Fixed pointer tables shared by room loading and effect interpreters.</summary>
    public static class Tables
    {
        public const int Layer3TilemapPointers = 0x83abf0;
        public const int TypeFunctionPointers = 0x83ac18;
        public const int AreaPaletteFxObjectListPointers = 0x83ac46;
        public const int AreaAnimatedTileObjectListPointers = 0x83ac56;
        public const int PaletteBlendColors = 0x89aa02;
    }

    /// <summary>VRAM and CGRAM layout used by the shared layer-three loader.</summary>
    public static class Layer3
    {
        public const ushort TilemapDestinationWord = 0x5be0;
        public const ushort TilemapByteCount = 0x0840;
        public const int EmptyPaletteColorIndex = 27;
        public const int PaletteBlendDestinationIndex = 25;
        public const int PaletteBlendColorCount = 3;
    }

    /// <summary>Landing Site rain tile animation and fixed-point velocities.</summary>
    public static class Rain
    {
        public const ushort AnimationDestinationWord = 0x4280;
        public const ushort AnimationByteCount = 0x0050;
        public const int AnimationFrameCount = 5;
        public const ushort AnimationFrameDuration = 10;
        public const int AnimationFirstFrameAddress = 0x87a874;
        public const ushort VerticalVelocity = 0x0600;

        /// <summary>
        /// Signed 8.8 BG3 horizontal velocities selected from bits two and three of RNG.
        /// </summary>
        public static ReadOnlySpan<ushort> HorizontalVelocities =>
            [0xfa00, 0x0600, 0xfc00, 0x0400];
    }

    /// <summary>Climb fog fixed-point BG3 velocities.</summary>
    public static class Fog
    {
        public const ushort VerticalVelocity = 0x0040;
        public const ushort HorizontalVelocity = 0x0050;
    }

    /// <summary>Landing Site scrolling-sky table and circular tilemap layout.</summary>
    public static class ScrollingSky
    {
        private static readonly SkyScrollSection[] SectionRows =
        [
            new(0x0000, 0x8000, 0x0000, 0),
            new(0x0010, 0xc000, 0x0000, 1),
            new(0x0038, 0x8000, 0x0000, 2),
            new(0x00d0, 0xc000, 0x0000, 3),
            new(0x00e0, 0x8000, 0x0000, 4),
            new(0x0120, 0xc000, 0x0000, 5),
            new(0x01a0, 0x8000, 0x0000, 6),
            new(0x01d8, 0xc000, 0x0000, 7),
            new(0x0238, 0x8000, 0x0000, 8),
            new(0x0268, 0xc000, 0x0000, 9),
            new(0x02a0, 0x8000, 0x0000, 10),

            // $02E0 deliberately targets slot eight again, making that strip advance
            // twice per frame exactly as the bank-$88 table specifies.
            new(0x02e0, 0xc000, 0x0000, 8),
            new(0x0300, 0x8000, 0x0000, 12),
            new(0x0320, 0xc000, 0x0000, 13),
            new(0x0350, 0x8000, 0x0000, 14),
            new(0x0378, 0xc000, 0x0000, 15),
            new(0x03c8, 0x8000, 0x0000, 16),
            new(0x0440, 0x7000, 0x0000, 17),
            new(0x0460, 0xc000, 0x0000, 18),
            new(0x0480, 0x8000, 0x0000, 19),
            new(0x0490, 0x0000, 0x0000, 20),
            new(0x04a8, 0x0000, 0x0000, 21),
            new(0x04b8, 0x0000, 0x0000, 22),
        ];

        public const ushort Bg2TilemapBaseWord = 0x4800;
        public const int LandChunkPointerTableAddress = 0x88ad9c;
        public const int DataSlotCount = 23;
        public const ushort WorldEndPosition = 0x0500;
        public const ushort GameplayFirstScanline = 32;
        public const ushort TilemapRowByteCount = 0x0040;
        public const ushort TilemapHalfRowWordCount = 0x0020;
        public const ushort UpperRowCameraOffset = 16;
        public const ushort LowerRowCameraOffset = 240;
        public const ushort TilemapPositionMask = 0x01f8;
        public const ushort SourcePositionMask = 0x07f8;

        /// <summary>
        /// Five declared land chunks followed by the adjacent ocean table's first word.
        /// The sixth entry is a deliberate native fall-through used near the room bottom.
        /// </summary>
        public static ReadOnlySpan<ushort> LandChunkOffsets =>
            [0xb180, 0xb980, 0xc180, 0xc980, 0xd180, 0xb180];

        /// <summary>The 23 eight-byte rows beginning at bank-$88 scrolling-sky data.</summary>
        public static ReadOnlySpan<SkyScrollSection> Sections => SectionRows;
    }

    /// <summary>Bank-$A0 room-shake displacement data and type boundaries.</summary>
    public static class Earthquake
    {
        public const ushort FirstEnemyShakingType = 0x0012;
        public const ushort FirstNonRenderedType = 0x0024;
        public const int BgDisplacementTableAddress = 0xa0872d;
        public const int BytesPerType = 8;
        public const ushort AlternatingDirectionTimerMask = 2;
        public const ushort EnemyShakeDuration = 2;
    }

    /// <summary>
    /// Selects the first default or door-specific record using the cartridge's linear walk.
    /// </summary>
    /// <returns>The bank-local record pointer, or zero when the list terminator is reached.</returns>
    public static ushort SelectRecord(
        ISnesAddressSpace bus,
        ushort fxPointer,
        ushort doorPointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ushort record = fxPointer;
        for (int guard = 0; guard < 256; guard++)
        {
            ushort candidateDoor = RomDataReader.ReadWordFixedBank(
                bus,
                Banks.RoomDefinitions | unchecked((ushort)(record + Record.DoorPointerOffset)));
            if (candidateDoor == 0 || candidateDoor == doorPointer)
                return record;
            if (candidateDoor == Record.TerminatorDoorPointer)
                return 0;
            record = unchecked((ushort)(record + Record.ByteCount));
        }

        throw new InvalidDataException(
            $"Room FX list $83:{fxPointer:X4} did not terminate for door $83:{doorPointer:X4}.");
    }

    /// <summary>Reads one byte from a selected bank-$83 FX record.</summary>
    public static byte ReadRecordByte(ISnesAddressSpace bus, ushort record, int fieldOffset)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if ((uint)fieldOffset >= Record.ByteCount)
            throw new ArgumentOutOfRangeException(nameof(fieldOffset));
        return bus.ReadByte(
            Banks.RoomDefinitions | unchecked((ushort)(record + fieldOffset)));
    }

    /// <summary>Reads one little-endian word from a selected bank-$83 FX record.</summary>
    public static ushort ReadRecordWord(ISnesAddressSpace bus, ushort record, int fieldOffset)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if ((uint)fieldOffset > Record.ByteCount - sizeof(ushort))
            throw new ArgumentOutOfRangeException(nameof(fieldOffset));
        return RomDataReader.ReadWordFixedBank(
            bus,
            Banks.RoomDefinitions | unchecked((ushort)(record + fieldOffset)));
    }
}
