namespace SuperMetroid.Core.Game;

/// <summary>Immutable cartridge tables and fixed native values for special Samus sequences.</summary>
/// <remarks>
/// These values describe the original state machines; live phase, timer, position, and
/// velocity remain in their dedicated owners. Palette addresses live separately in
/// <see cref="SamusPaletteRomData"/> so rendering data is not mixed with choreography.
/// </remarks>
public static class SamusSpecialSequenceRomData
{
    /// <summary>Fatal-damage graphics transfers and state boundaries.</summary>
    public static class Death
    {
        /// <summary>Sixteen calls display the ordinary death pose before flashing.</summary>
        public const ushort PreFlashFrameCount = 16;
        /// <summary>Sixty calls alternate palettes before the suit explodes.</summary>
        public const ushort FlashFrameCount = 60;
        /// <summary>Bytes copied by each of the five death graphics transfers.</summary>
        public const int TileSegmentByteCount = 0x0400;
        /// <summary>Right-facing base index in Samus's death-explosion spritemap table.</summary>
        public const ushort RightExplosionSpritemap = 0x081c;
        /// <summary>Left-facing base index in Samus's death-explosion spritemap table.</summary>
        public const ushort LeftExplosionSpritemap = 0x0825;

        /// <summary>$9B:B7BF/B7C9: cyclic $400-byte source pages and matching $200-word VRAM destinations.</summary>
        public static TileSegmentSequence TileSegments => default;
        /// <summary>$9B:B420: ball, unused glitch-ball and ordinary movement select their death pose phase.</summary>
        public static InitialFrameSequence InitialFramesByMovementType => default;

        public readonly struct TileSegmentSequence : IReadOnlyList<SamusDeathTileSegment>
        {
            public int Count => 5;
            public int Length => Count;
            public SamusDeathTileSegment this[int index]
            {
                get
                {
                    if ((uint)index >= Count) throw new IndexOutOfRangeException();
                    int segment = (index + 1) % Count;
                    return new(TileSourceBase + segment * TileSegmentByteCount,
                        (ushort)(TileDestinationBase + segment * (TileSegmentByteCount / 2)));
                }
            }
            public IEnumerator<SamusDeathTileSegment> GetEnumerator()
            {
                for (int index = 0; index < Count; index++) yield return this[index];
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }
        public readonly struct InitialFrameSequence : IReadOnlyList<byte>
        {
            public int Count => (int)SamusMovementType.Special + 1;
            public int Length => Count;
            public byte this[int index]
            {
                get
                {
                    if ((uint)index >= Count) throw new IndexOutOfRangeException();
                    return (SamusMovementType)index switch
                    {
                        SamusMovementType.MorphBallGround or SamusMovementType.MorphBallFalling or
                        SamusMovementType.SpringBallGround or SamusMovementType.SpringBallInAir or
                        SamusMovementType.SpringBallFalling => 1,
                        SamusMovementType.UnusedGlitchBall or SamusMovementType.UnusedGlitchBallAlternate => 0,
                        _ => 5,
                    };
                }
            }
            public IEnumerator<byte> GetEnumerator()
            {
                for (int index = 0; index < Count; index++) yield return this[index];
            }
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }
        /// <summary>$9B:8000: first of five consecutive death-artwork pages, queued last by $9B:B7BF.</summary>
        private const int TileSourceBase = 0x9b8000;
        /// <summary>$9B:B7C9 final word $6000: base VRAM word destination of those five pages.</summary>
        private const ushort TileDestinationBase = 0x6000;
    }
    /// <summary>Varia/Gravity pickup HDMA curve and native 8.8 geometry.</summary>
    public static class SuitPickup
    {
        /// <summary>Collapsed beam with both window endpoints at X=120.</summary>
        public const ushort NarrowBeamEndpoints = 0x7878;
        /// <summary>Inverted endpoints representing an empty window.</summary>
        public const ushort EmptyWindowEndpoints = 0x00ff;
        /// <summary>Endpoints representing the complete 256-pixel window.</summary>
        public const ushort FullScreenEndpoints = 0xff00;
        /// <summary>Initial unsigned 8.8 widening speed.</summary>
        public const ushort InitialWideningSpeed = 0x0100;
        /// <summary>Per-frame unsigned 8.8 widening acceleration.</summary>
        public const ushort WideningAcceleration = 0x0060;
        /// <summary>Per-frame unsigned 8.8 shrinking deceleration.</summary>
        public const ushort ShrinkingDeceleration = 0x0020;
        /// <summary>Minimum unsigned 8.8 shrinking speed.</summary>
        public const ushort MinimumShrinkingSpeed = 0x0100;
        /// <summary>Stage-two initial asymmetric endpoint pair.</summary>
        public const ushort CurvedWideningStart = 0x846c;
        /// <summary>Stage-five signed starting endpoint pair from <c>$88:E222</c>.</summary>
        public const ushort DissipationStart = 0xf8ff;
        /// <summary>Number of scanlines and window words produced by the HDMA builder.</summary>
        public const int WindowScanlineCount = 256;
    }

    /// <summary>Power Bomb/Crystal Flash HDMA radii, shape streams, and phase boundaries.</summary>
    public static class PowerBomb
    {
        /// <summary>Initial unsigned 8.8 radius.</summary>
        public const ushort InitialRadius = 0x0400;
        /// <summary>Initial pre-explosion unsigned 8.8 radial velocity.</summary>
        public const ushort InitialPreExplosionSpeed = 0x3000;
        /// <summary>Pre-explosion unsigned 8.8 acceleration.</summary>
        public const ushort PreExplosionAcceleration = 0x0080;
        /// <summary>Explosion unsigned 8.8 acceleration.</summary>
        public const ushort ExplosionAcceleration = 0x0030;
        /// <summary>First yellow ellipse definition in bank <c>$88</c>.</summary>
        /// <remarks>
        /// #625 exact pre-scaled profile generator: yellow frame f=0..3 uses n=f+3 and
        /// R=min(255, floor(4+48*n-n*(n-1)/4)). This is the initial radius 4, velocity 48,
        /// and per-frame deceleration 1/2, integrated before discarding the fractional radius.
        /// Feed R into the quantized ellipse/scaling recipe on FirstWhiteShape. All 768
        /// bytes match the ROM and pinned PowerBomb_PreExplosion_ShapeDefinitionTables_PreScaled.
        /// The last frame clamps R to 255; it is not a per-row correction. Use the authored
        /// frame number, not the live radius: stage boundaries affect runtime accumulator timing.
        /// </remarks>
        public const ushort FirstYellowShape = 0x9f06;
        /// <summary>Exclusive end of yellow ellipse definitions.</summary>
        public const ushort YellowShapeEnd = 0xa206;
        /// <summary>First white ellipse definition in bank <c>$88</c>.</summary>
        /// <remarks>
        /// #625 exact, correction-free generator for all 17*192 white-profile bytes:
        /// frame f=0..16 uses n=f+37 and R=min(255, 4+floor(3*n*(n-1)/32)). This integrates
        /// radius 4, initial speed zero, acceleration 3/16; round only after the quadratic sum.
        /// Generate a 192-row unit profile B: for j=0..191, k=floor(512*asin((j+1)/192)/pi),
        /// then B(j)=min(255, floor(256*cos(k*pi/512))). The endpoint j=191 is exactly zero.
        /// Thus the ellipse uses a 1,024-step full circle BEFORE rasterization, not a smooth square root.
        /// For output row y=0..191 let j=max(0, ceil(256*y/R)-1), equivalently
        /// max(0, (256*y-1)/R) with integer division. Return zero if j&gt;=192, else floor(B(j)*R/256).
        /// Preserve both quantization stages and the lower-side inverse-scale boundary.
        /// The #1165 research verified all 4,032 white/yellow bytes against NTSC J/U v1.0 ROM
        /// and pinned bank_88.asm. Its deterministic decimal implementation selects k by bounded
        /// sine comparisons rather than platform asin, and certified every resulting integer.
        /// Both true pi and 3.14159 reproduce the bytes, so their historical choice is undetermined.
        /// A continuous ellipse basis misses 955 bytes; ordinary floor(256*y/R) misses 59.
        /// Full-circle resolutions 256, 512, 2048, and 4096 also fail. No stored shape/radius table
        /// or per-entry exceptions are required. These are authored frame profiles: do not substitute
        /// the live ExplosionRadius, whose white-phase threshold is offset from n=37.
        /// Runtime replacement, caching strategy, and performance verification remain deferred.
        /// </remarks>
        public const ushort FirstWhiteShape = 0x9246;
        /// <summary>Exclusive end of white ellipse definitions.</summary>
        public const ushort WhiteShapeEnd = 0x9f06;
        /// <summary>Bytes occupied by one ellipse definition: 192 one-byte pixel-row widths.</summary>
        public const ushort ShapeStride = 192;
        /// <summary>Radius at which white pre-explosion becomes yellow.</summary>
        public const ushort PreExplosionWhiteLimit = 0x9200;
        /// <summary>Radius at which yellow explosion switches to white shapes.</summary>
        public const ushort ExplosionYellowLimit = 0x8600;
        /// <summary>Crystal Flash's shorter explosion radius limit.</summary>
        public const ushort CrystalFlashRadiusLimit = 0x2000;
        /// <summary>
        /// Maximum component of the first native Crystal Flash color at $88:8D85:
        /// RGB5 (14,14,10). This is a lifetime operand, not replacement artwork.
        /// </summary>
        private const byte CrystalFlashInitialFadeSteps = 14;
        /// <summary>
        /// $88:A5BE-$A5CC divides the consumed unsigned 8.8 radius by $0800 to
        /// select a color. Crystal Flash reaches only the first four yellow rows.
        /// </summary>
        private const int CrystalFlashColorRadiusShift = 11;
        /// <summary>
        /// Compiled $88:A35D wake criterion for a stock Crystal Flash: the maximum
        /// native component is 14+row for the four reachable $88:8D85 rows. Keeping
        /// this operand independent of editable RGB preserves the native fade count,
        /// signed-timer cadence and cleanup frame without storing presentation colors.
        /// </summary>
        public static byte CrystalFlashAfterglowSteps(ushort consumedRadius)
        {
            if (consumedRadius >= CrystalFlashRadiusLimit)
                throw new ArgumentOutOfRangeException(nameof(consumedRadius),
                    "Crystal Flash consumes a radius below its compiled expansion limit.");
            return (byte)(CrystalFlashInitialFadeSteps + (consumedRadius >> CrystalFlashColorRadiusShift));
        }
        /// <summary>$88:8B96, byte reloaded by ordinary and Crystal Flash afterglow handlers.</summary>
        public const byte AfterglowTimerReload = 3;
    }

    /// <summary>Shared trigonometric and boost-state constants used by shinespark.</summary>
    public static class Shinespark
    {
        /// <summary><c>$90:D2BD</c>, EndSuperJump's minimum energy to sustain a native shinespark.</summary>
        public const ushort MinimumSustainingEnergy = 30;
        /// <summary>High-byte state identifying a fully active Speed Booster.</summary>
        public const ushort ActiveSpeedBoostCounter = 0x0400;
    }
}

/// <summary>One immutable death-tile DMA source/destination pairing.</summary>
public readonly record struct SamusDeathTileSegment(
    int SourceAddress,
    ushort EncodedVramDestination);
