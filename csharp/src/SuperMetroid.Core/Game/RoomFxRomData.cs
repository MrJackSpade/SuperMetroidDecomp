
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
        /// <summary>SNES bank base containing animated-tile object definitions and bytecode.</summary>
        public const int AnimatedTiles = 0x870000;
        /// <summary>SNES bank base containing shared room-effect tilemaps.</summary>
        public const int Tilemaps = 0x8a0000;
        /// <summary>SNES bank base containing palette-effect objects and bytecode.</summary>
        public const int PaletteFx = 0x8d0000;
    }

    /// <summary>Layout of one bank-$83 <c>FxDef</c> entry.</summary>
    public static class Record
    {
        /// <summary>Size in bytes of one bank-$83 room-FX definition.</summary>
        public const int ByteCount = 16;

        /// <summary>Door word that terminates an FX list without selecting a record.</summary>
        public const ushort TerminatorDoorPointer = ushort.MaxValue;
    }

    /// <summary>Fixed pointer tables shared by room loading and effect interpreters.</summary>
    public static class Tables
    {
        /// <summary>$83:ABF0, table of layer-three effect tilemap pointers.</summary>
        public const int Layer3TilemapPointers = 0x83abf0;
        /// <summary>$89:AA02, color words blended into the temporary FX palette entries.</summary>
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

        /// <summary>VRAM word destination of a room effect's layer-three tilemap.</summary>
        public const ushort TilemapDestinationWord = 0x5be0;
        /// <summary>CGRAM index cleared when no room-effect palette is selected.</summary>
        public const int EmptyPaletteColorIndex = 27;
        /// <summary>First CGRAM index receiving the shared three-color FX blend.</summary>
        public const int PaletteBlendDestinationIndex = 25;
        /// <summary>Number of colors copied by the shared room-effect palette blend.</summary>
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

        /// <summary>First acid instruction-list entry at $87:82B1.</summary>
        public const ushort AcidFirstInstruction = 0x82b1;

        /// <summary>Rain transfer destination from object $87:82E7.</summary>
        public const ushort RainDestinationWord = 0x4280;

        /// <summary>Rain frame size from object $87:82E7.</summary>
        public const ushort RainFrameByteCount = 0x0050;

        /// <summary>First rain instruction-list entry at $87:82CF.</summary>
        public const ushort RainFirstInstruction = 0x82cf;
    }

    /// <summary>Landing Site rain tile animation and fixed-point velocities.</summary>
    public static class Rain
    {
        /// <summary>Positive six-pixel vertical rain velocity in signed 8.8 units.</summary>
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

        /// <summary>
        /// Frames between lava's ambient sounds: <c>Instruction_LavaSoundTimer_70</c>
        /// ($88:B3A9) and its reload at $88:B433.
        /// </summary>
        public const ushort AmbientSoundPeriod = 0x70;

        /// <summary>Most library-two sounds queued when lava requests an ambient sound.</summary>
        public const byte AmbientSoundMaximumQueued = 6;

        /// <summary>
        /// <c>Lava_SoundEffects</c> ($88:B3A1): library-two sounds chosen by the low three
        /// RNG bits.
        /// </summary>
        public static SoundEffectId AmbientSound(int randomIndex) => SoundEffectId.FromCartridge(
            SoundEffectLibrary.Library2,
            (randomIndex & 7) switch
            {
                0 or 3 or 6 => 0x12,
                1 or 4 or 7 => 0x13,
                _ => 0x14,
            });

        /// <summary>Calculates the vertical heat-haze displacement for index 0..15.</summary>
        /// <remarks>Native $88:B60A is independently verified as the same mirrored,
        /// sign-alternating integer wave as $88:C46E. Reuse that exact bounded mapping;
        /// liquid-options bit one selects this BG2VOFS waveform.</remarks>
        public static short VerticalWaveDisplacement(int index) => Water.WaveDisplacement(index);

        /// <summary>
        /// Signed BG2HOFS displacement for index 0..15, matching $88:B589.
        /// This pulse pair subtracts a four-sample unit pulse starting at eight
        /// from the same pulse starting at two. The native $88:B53B consumer
        /// rotates all sixteen samples; its two unequal zero gaps are preserved.
        /// No sinusoidal interpolation or extrapolation is implied.
        /// </summary>
        public static short HorizontalWaveDisplacement(int index)
        {
            if ((uint)index >= WaveDisplacementCount) throw new IndexOutOfRangeException();
            int positivePulse = (uint)(index - 2) < 4 ? 1 : 0;
            int negativePulse = (uint)(index - 8) < 4 ? 1 : 0;
            return (short)(positivePulse - negativePulse);
        }
    }

    /// <summary>Shared water/lava/acid tide encoding consumed by <c>FxHandleTide</c>.</summary>
    public static class LiquidTide
    {
        /// <summary>Liquid-options bit seven selects the ±8-pixel tide.</summary>
        public const ushort SmallTideOption = 0x0080;

        /// <summary>Liquid-options bit six selects the ±32-pixel tide.</summary>
        public const ushort LargeTideOption = 0x0040;

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
        /// <summary>Positive one-quarter-pixel vertical fog velocity in signed 8.8 units.</summary>
        public const ushort VerticalVelocity = 0x0040;
        /// <summary>Positive five-sixteenths-pixel horizontal fog velocity in signed 8.8 units.</summary>
        public const ushort HorizontalVelocity = 0x0050;
    }

    /// <summary>Landing Site scrolling-sky table and circular tilemap layout.</summary>
    public static class ScrollingSky
    {
        /// <summary>Twenty-three eight-byte sky-band records processed by $88:ADC2.</summary>
        public const int SectionCount = 23;

        /// <summary>Selects a sky band's boundary, unsigned16.16 velocity and HDMA data slot.</summary>
        /// <remarks>Native records at $88:AEC1 have fixed band geometry, so boundaries
        /// are explicit configuration cases. Bands0..16 alternate fractional speeds
        /// 8000/C000; band17 has its own7000 speed, bands18/19 resume C000/8000,
        /// and the three bottom bands are stationary. Integer speed is always zero.
        /// Data slots follow band indices except band11 targets slot8 again, giving it
        /// two additions per frame. All four fields are independently verified against
        /// supported NTSC J/U v1.0 and pinned bank_88.asm
        /// (362be646929cf8e483f692b73a6561cfc2dc1d0d).
        /// Reject outside0..22 before computing fields; no row array/cache remains.</remarks>
        public static SkyScrollSection GetSection(int index)
        {
            ushort top = index switch
            {
                0 => 0x0000, 1 => 0x0010, 2 => 0x0038, 3 => 0x00d0,
                4 => 0x00e0, 5 => 0x0120, 6 => 0x01a0, 7 => 0x01d8,
                8 => 0x0238, 9 => 0x0268, 10 => 0x02a0, 11 => 0x02e0,
                12 => 0x0300, 13 => 0x0320, 14 => 0x0350, 15 => 0x0378,
                16 => 0x03c8, 17 => 0x0440, 18 => 0x0460, 19 => 0x0480,
                20 => 0x0490, 21 => 0x04a8, 22 => 0x04b8,
                _ => throw new IndexOutOfRangeException(),
            };
            ushort subspeed = (ushort)(index < 17 ? 0x8000 + (index & 1) * 0x4000 :
                index == 17 ? 0x7000 : index < 20 ? 0xc000 - (index - 18) * 0x4000 : 0);
            return new(top, subspeed, 0, index == 11 ? 8 : index);
        }
        /// <summary>VRAM word base of the scrolling-sky BG2 tilemap.</summary>
        public const ushort Bg2TilemapBaseWord = 0x4800;
        /// <summary>Number of horizontal-scroll accumulator and HDMA data slots.</summary>
        public const int DataSlotCount = 23;
        /// <summary>Exclusive vertical world position of the authored sky-band table.</summary>
        public const ushort WorldEndPosition = 0x0500;
        /// <summary>First gameplay scanline below the 32-pixel HUD.</summary>
        public const ushort GameplayFirstScanline = 32;
        /// <summary>Byte stride of one 32-word BG tilemap row.</summary>
        public const ushort TilemapRowByteCount = 0x0040;
        /// <summary>Word count of one 32-tile BG tilemap row.</summary>
        public const ushort TilemapHalfRowWordCount = 0x0020;
        /// <summary>Camera Y offset selecting the upper circular tilemap row.</summary>
        public const ushort UpperRowCameraOffset = 16;
        /// <summary>Camera Y offset selecting the lower circular tilemap row.</summary>
        public const ushort LowerRowCameraOffset = 240;
        /// <summary>Mask aligning the circular tilemap position to an eight-pixel row.</summary>
        public const ushort TilemapPositionMask = 0x01f8;
        /// <summary>Mask aligning a sky source position within its 2048-pixel wrap.</summary>
        public const ushort SourcePositionMask = 0x07f8;


    }

    /// <summary>Bank-$A0 room-shake displacement data and type boundaries.</summary>
    public static class Earthquake
    {
        /// <summary>First earthquake type that also shakes enemy positions.</summary>
        public const ushort FirstEnemyShakingType = 0x0012;
        /// <summary>First earthquake type whose displacement is not applied to rendering.</summary>
        public const ushort FirstNonRenderedType = 0x0024;
        /// <summary>Timer bit selecting the alternating shake direction.</summary>
        public const ushort AlternatingDirectionTimerMask = 2;
        /// <summary>Updates for which an enemy retains one applied shake displacement.</summary>
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
        /// <remarks>
        /// Retained as the particular irregular rhythm of repeated identical rumble
        /// sounds. The index advances chronologically by one emission, not by angle,
        /// distance, intensity or a semantic sound choice. The complete cycle has
        /// neither a ramp nor half-cycle reflection/repetition; equal delay values
        /// have different successors. Live RNG supplies separate variation. Reciting
        /// these eight choices in cases or fitting them would merely disguise the
        /// same rhythmic content, while replacing the rhythm changes sound cadence.
        /// This is the arbitrary-sequence/nonsense exception, not a cost or size exemption.
        /// </remarks>
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
