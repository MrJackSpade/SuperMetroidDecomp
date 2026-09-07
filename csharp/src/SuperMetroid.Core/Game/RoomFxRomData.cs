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
        /// <summary>
        /// First gameplay-region word of the ordinary BG3 page. Startup fills this range
        /// with character $6F before any room effect is loaded.
        /// </summary>
        public const ushort PaddingDestinationWord = 0x5880;

        /// <summary>
        /// Blank tilemap word copied through the gameplay portion of the first BG3 page by
        /// <c>Load_StandardBG3Tiles_SpriteTiles_ClearTilemaps</c> at $82:82E2.
        /// </summary>
        public const ushort PaddingTilemapWord = 0x006f;

        /// <summary>Words from BG3 row four through the end of its first 32-row page.</summary>
        public const int PaddingWordCount = 0x0380;

        /// <summary>
        /// BG3SC base used by liquid effects. Its vertical-size bit exposes the HUD/padding
        /// page followed by the room-effect page as one 32-by-64 tilemap.
        /// </summary>
        public const ushort LiquidTilemapBaseWord = 0x5800;

        /// <summary>
        /// BG3SC base selected directly by the rain and fog initializers at $88:D950 and
        /// $88:DB08. These effects use only the second 32-by-32 page.
        /// </summary>
        public const ushort FullScreenAtmosphereTilemapBaseWord = 0x5c00;

        /// <summary>Vertical coordinate mask for the liquid 32-by-64 BG3 tilemap.</summary>
        public const int LiquidVerticalCoordinateMask = 0x01ff;

        /// <summary>Vertical coordinate mask for rain/fog's 32-by-32 BG3 tilemap.</summary>
        public const int FullScreenAtmosphereVerticalCoordinateMask = 0x00ff;

        /// <summary>
        /// First word cleared by <c>Clear_FX_Tilemap</c> at $82:E566 and
        /// <c>ClearFXTilemap</c> at $80:A29C before a room's effect tilemap is uploaded.
        /// </summary>
        public const ushort ClearDestinationWord = 0x5880;

        /// <summary>Blank BG3 tilemap word installed by both native FX-clear routines.</summary>
        public const ushort ClearTilemapWord = 0x184e;

        /// <summary>Words cleared from $5880 through $5FFF on every room load.</summary>
        public const int ClearWordCount = 0x0780;

        public const ushort TilemapDestinationWord = 0x5be0;
        public const ushort TilemapByteCount = 0x0840;
        public const int EmptyPaletteColorIndex = 27;
        public const int PaletteBlendDestinationIndex = 25;
        public const int PaletteBlendColorCount = 3;
    }

    /// <summary>Retail headers and first frames of type-owned bank-$87 animations.</summary>
    public static class Layer3AnimatedTiles
    {
        /// <summary>Common lava/acid transfer destination from objects $82AB/$82C9.</summary>
        public const ushort LiquidDestinationWord = 0x4280;

        /// <summary>Common lava/acid frame size from objects $82AB/$82C9.</summary>
        public const ushort LiquidFrameByteCount = 0x0040;

        /// <summary>First lava instruction-list entry at $87:8293.</summary>
        public const ushort LavaFirstInstruction = 0x8293;

        /// <summary>First lava character frame at $87:A564.</summary>
        public const ushort LavaFirstFrame = 0xa564;

        /// <summary>First acid instruction-list entry at $87:82B1.</summary>
        public const ushort AcidFirstInstruction = 0x82b1;

        /// <summary>First acid character frame at $87:A6A4.</summary>
        public const ushort AcidFirstFrame = 0xa6a4;

        /// <summary>Rain transfer destination from object $87:82E7.</summary>
        public const ushort RainDestinationWord = 0x4280;

        /// <summary>Rain frame size from object $87:82E7.</summary>
        public const ushort RainFrameByteCount = 0x0050;

        /// <summary>First rain instruction-list entry at $87:82CF.</summary>
        public const ushort RainFirstInstruction = 0x82cf;

        /// <summary>First rain character frame at $87:A874.</summary>
        public const ushort RainFirstFrame = 0xa874;
    }

    /// <summary>Landing Site rain tile animation and fixed-point velocities.</summary>
    public static class Rain
    {
        public const ushort VerticalVelocity = 0x0600;

        /// <summary>
        /// Signed 8.8 BG3 horizontal velocities selected from bits two and three of RNG.
        /// </summary>
        public static ReadOnlySpan<ushort> HorizontalVelocities =>
            [0xfa00, 0x0600, 0xfc00, 0x0400];
    }

    /// <summary>Bank-$88 water-surface HDMA constants at <c>$88:C3FF-$C644</c>.</summary>
    public static class Water
    {
        /// <summary>
        /// Liquid-options bit two at WRAM <c>$197E</c>. When set, Samus movement ignores
        /// the water surface; the n00b-tube PLM clears this bit at <c>$84:D525</c> after
        /// the glass breaks.
        /// </summary>
        public const ushort PhysicsDisabledOption = 0x0004;

        /// <summary>Frames between BG3 wave-table rotations.</summary>
        public const ushort Bg3WavePhaseDuration = 10;

        /// <summary>Frames between optional BG2 wave-table rotations.</summary>
        public const ushort Bg2WavePhaseDuration = 6;

        /// <summary>Fractional 8.8 X-scroll increment selected by liquid-options bit zero.</summary>
        public const ushort HorizontalSubscrollVelocity = 0x0040;

        /// <summary>Number of signed words in the circular water displacement table.</summary>
        public const int WaveDisplacementCount = 16;

        /// <summary>
        /// Signed per-scanline offsets from <c>WaveDisplacementTable_Water</c> at
        /// <c>$88:C46E</c>. The repeated eight-value waveform is intentional.
        /// </summary>
        public static ReadOnlySpan<short> WaveDisplacements =>
            [0, 1, 1, 0, 0, -1, -1, 0, 0, 1, 1, 0, 0, -1, -1, 0];
    }

    /// <summary>Bank-$88 lava/acid BG2 distortion data at <c>$88:B4D5-$B628</c>.</summary>
    public static class LavaAcid
    {
        /// <summary>Liquid-options bit selecting the vertical BG2 wave used by heat rooms.</summary>
        public const ushort VerticalBg2WaveOption = 0x0002;

        /// <summary>Liquid-options bit selecting the alternate horizontal BG2 wave.</summary>
        public const ushort HorizontalBg2WaveOption = 0x0004;

        /// <summary>Frames between vertical-wave rotations in <c>$88:B5A9</c>.</summary>
        public const ushort VerticalWavePhaseDuration = 4;

        /// <summary>Frames between horizontal-wave rotations in <c>$88:B53B</c>.</summary>
        public const ushort HorizontalWavePhaseDuration = 6;

        /// <summary>Number of one-scanline entries in either circular HDMA waveform.</summary>
        public const int WaveDisplacementCount = 16;

        /// <summary>
        /// Signed BG2VOFS offsets read from <c>$88:B60A</c>. Ordinary Norfair records set
        /// liquid-options bit one and therefore use this waveform as the visible heat haze.
        /// </summary>
        public static ReadOnlySpan<short> VerticalWaveDisplacements =>
            [0, 1, 1, 0, 0, -1, -1, 0, 0, 1, 1, 0, 0, -1, -1, 0];

        /// <summary>Signed BG2HOFS offsets read from <c>$88:B589</c>.</summary>
        public static ReadOnlySpan<short> HorizontalWaveDisplacements =>
            [0, 0, 1, 1, 1, 1, 0, 0, -1, -1, -1, -1, 0, 0, 0, 0];
    }

    /// <summary>Shared water/lava/acid tide encoding consumed by <c>FxHandleTide</c>.</summary>
    public static class LiquidTide
    {
        /// <summary>Liquid-options bit seven selects the ±8-pixel tide.</summary>
        public const ushort SmallTideOption = 0x0080;

        /// <summary>Liquid-options bit six selects the ±32-pixel tide.</summary>
        public const ushort LargeTideOption = 0x0040;

        /// <summary>Sign-extended 8-bit sine words beginning at $A0:B443.</summary>
        public const int SignedSineTableAddress = 0xa0b443;

        /// <summary>Small-tide sine scale before the native byte-shifted fixed add.</summary>
        public const int SmallTideScale = 8;

        /// <summary>Large-tide sine scale before the native byte-shifted fixed add.</summary>
        public const int LargeTideScale = 32;

        /// <summary>Small-tide phase delta for a nonnegative sine sample.</summary>
        public const ushort SmallTidePositivePhaseDelta = 288;

        /// <summary>Small-tide phase delta for a negative sine sample.</summary>
        public const ushort SmallTideNegativePhaseDelta = 192;

        /// <summary>Large-tide phase delta for a nonnegative sine sample.</summary>
        public const ushort LargeTidePositivePhaseDelta = 224;

        /// <summary>Large-tide phase delta for a negative sine sample.</summary>
        public const ushort LargeTideNegativePhaseDelta = 128;
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
        /// <summary>$88:ADA6, ocean sky chunk pointers passed by RoomMainAsm_ScrollingSkyOcean ($88:AF99).</summary>
        public const int OceanChunkPointerTableAddress = 0x88ada6;
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

        /// <summary>Room-shake type written every active lava/acid rise frame.</summary>
        public const ushort RisingLiquidType = 0x0015;

        /// <summary>
        /// Bits set in the global earthquake timer every active lava/acid rise frame.
        /// Native uses <c>TSB</c>, so existing low bits are preserved.
        /// </summary>
        public const ushort RisingLiquidTimerBits = 0x0020;

        /// <summary>
        /// Base delays paired with the eight <c>$46</c> entries at
        /// <c>$88:B256-$B276</c>. The live RNG low two bits are added at each emission.
        /// </summary>
        public static ReadOnlySpan<ushort> RisingLiquidSoundBaseTimers =>
            [1, 3, 2, 1, 1, 2, 2, 1];

        /// <summary>Rooms whose special-FX initialization suppresses quake sounds.</summary>
        public static class SoundSuppressedRooms
        {
            /// <summary>Bomb Torizo room header <c>$8F:9804</c>.</summary>
            public const ushort BombTorizo = 0x9804;

            /// <summary>Climb room header <c>$8F:96BA</c>.</summary>
            public const ushort Climb = 0x96ba;

            /// <summary>Ridley room header <c>$8F:B32E</c>.</summary>
            public const ushort Ridley = 0xb32e;

            /// <summary>Pillar room header <c>$8F:B457</c>.</summary>
            public const ushort Pillar = 0xb457;

            /// <summary>Mother Brain room header <c>$8F:DD58</c>.</summary>
            public const ushort MotherBrain = 0xdd58;

            /// <summary>Fourth Tourian escape room header <c>$8F:DEDE</c>.</summary>
            public const ushort TourianEscape4 = 0xdede;

            /// <summary>
            /// Exact comparison set at <c>$88:82CD-$82E9</c>. Matching rooms initialize
            /// the earthquake-sound timer to <c>$FFFF</c>.
            /// </summary>
            public static ReadOnlySpan<ushort> All =>
                [BombTorizo, Climb, Ridley, Pillar, MotherBrain, TourianEscape4];
        }
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
