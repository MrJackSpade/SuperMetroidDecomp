
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

        /// <summary>Calculates rain's signed 8.8 horizontal velocity for selector 0..3.</summary>
        /// <remarks>Native $88:D981 selects a word at $88:D992 using RNG bits two/three.
        /// Selector bit one reduces the speed from six to four pixels/frame; bit zero
        /// selects positive rather than negative motion. Multiply by256, then preserve
        /// the two's-complement ushort representation. The four original values and
        /// all ushort RNG inputs are independently verified against NTSC J/U v1.0 and
        /// pinned bank_88.asm (362be646929cf8e483f692b73a6561cfc2dc1d0d).
        /// Invalid selectors retain the former indexed span's rejection.</remarks>
        public static ushort HorizontalVelocity(int selection)
        {
            if ((uint)selection >= 4)
                throw new IndexOutOfRangeException();
            int speed = (6 - (selection & 2)) << 8;
            return unchecked((ushort)((selection & 1) == 0 ? -speed : speed));
        }
    }

    /// <summary>FX type $08's literal scroll and source-blending operands.</summary>
    public static class Spores
    {
        /// <summary>$88:DA73 adds $FFC0 to BG3's signed 8.8 Y accumulator.</summary>
        public const ushort VerticalVelocity = 0x0040;
        /// <summary>$88:80AB writes CGADSUB=$32: BG2, OBJ palettes 4-7 and backdrop, not BG1.</summary>
        public const byte ColorMathSources = 0x32;
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

        /// <summary>Calculates the signed scanline displacement for a water wave index 0..15.</summary>
        /// <remarks>The original $88:C46E waveform consists of two identical eight-sample
        /// periods. Within each four-sample half-wave, reflect the integer ramp around
        /// its midpoint: min(index&amp;3, 3-(index&amp;3)). Bit two reverses its sign.
        /// This is an exact integer triangular waveform, without a floating-point
        /// generation claim. Water BG2/BG3 and the identical vertical lava/acid wave
        /// share this mapping. All original words at C46E and B60A are independently
        /// checked against supported NTSC J/U v1.0 and pinned bank_88.asm
        /// (362be646929cf8e483f692b73a6561cfc2dc1d0d).
        /// Reject outside the original sixteen-word span before periodic folding.</remarks>
        public static short WaveDisplacement(int index)
        {
            if ((uint)index >= WaveDisplacementCount)
                throw new IndexOutOfRangeException();
            int position = index & 3;
            int magnitude = Math.Min(position, 3 - position);
            return (short)((index & 4) == 0 ? magnitude : -magnitude);
        }
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

        /// <summary>Calculates the vertical heat-haze displacement for index 0..15.</summary>
        /// <remarks>Native $88:B60A is independently verified as the same mirrored,
        /// sign-alternating integer wave as $88:C46E. Reuse that exact bounded mapping;
        /// liquid-options bit one selects this BG2VOFS waveform.</remarks>
        public static short VerticalWaveDisplacement(int index) => Water.WaveDisplacement(index);

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
            ScrollingSkyChunkPointerDefinitions.Land[..6];

        /// <summary>The 23 eight-byte rows beginning at bank-$88 scrolling-sky data.</summary>
        public static ReadOnlySpan<SkyScrollSection> Sections => SectionRows;
    }

    /// <summary>Bank-$A0 room-shake displacement data and type boundaries.</summary>
    public static class Earthquake
    {
        /// <summary>$86:846B, kScreenShakeOffsets: signed XY offsets added to enemy projectile draw origins.</summary>
        public const int ProjectileDisplacementTableAddress = 0x86846b;
        /// <summary>$86:8427 indexes one pair of signed words for each earthquake type.</summary>
        public const int ProjectileBytesPerType = 4;
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

            /// <summary>Whether room initialization disables earthquake sounds for this identity.</summary>
            /// <remarks>Ports the six named comparisons at $88:82CD..82E9, whose shared
            /// branch initializes the sound timer to FFFF. All other ushort identities,
            /// including zero and unknown rooms, return false. Verified against original
            /// NTSC J/U v1.0 instruction operands and pinned bank_88.asm
            /// (362be646929cf8e483f692b73a6561cfc2dc1d0d). No stored membership list remains.</remarks>
            public static bool Contains(ushort room) => room is
                BombTorizo or Climb or Ridley or Pillar or MotherBrain or TourianEscape4;
        }
    }

}
