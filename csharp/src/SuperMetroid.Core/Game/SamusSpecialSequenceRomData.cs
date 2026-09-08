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
        private static readonly SamusDeathTileSegment[] DeathTileSegments =
        [
            new(0x9b8400, 0x6200),
            new(0x9b8800, 0x6400),
            new(0x9b8c00, 0x6600),
            new(0x9b9000, 0x6800),
            new(0x9b8000, 0x6000),
        ];

        private static readonly byte[] MovementTypeInitialFrames =
        [
            5, 5, 5, 5, 1, 5, 5, 0,
            1, 0, 5, 5, 5, 5, 5, 5,
            5, 1, 1, 1, 5, 5, 5, 5,
            5, 5, 5, 5,
        ];

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

        /// <summary>The five bank-$9B tile sources paired with their encoded VRAM targets.</summary>
        public static ReadOnlySpan<SamusDeathTileSegment> TileSegments => DeathTileSegments;

        /// <summary>Initial visible death frame indexed by the 28 retail movement types.</summary>
        public static ReadOnlySpan<byte> InitialFramesByMovementType =>
            MovementTypeInitialFrames;
    }

    /// <summary>Varia/Gravity pickup HDMA curve and native 8.8 geometry.</summary>
    public static class SuitPickup
    {
        /// <summary><c>$88:E3C9</c>, 128-byte symmetric light-beam curve.</summary>
        public const int BeamCurve = 0x88e3c9;
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
        public const ushort FirstYellowShape = 0x9f06;
        /// <summary>Exclusive end of yellow ellipse definitions.</summary>
        public const ushort YellowShapeEnd = 0xa206;
        /// <summary>First white ellipse definition in bank <c>$88</c>.</summary>
        public const ushort FirstWhiteShape = 0x9246;
        /// <summary>Exclusive end of white ellipse definitions.</summary>
        public const ushort WhiteShapeEnd = 0x9f06;
        /// <summary>Bytes occupied by one 96-word ellipse definition.</summary>
        public const ushort ShapeStride = 192;
        /// <summary>Radius at which white pre-explosion becomes yellow.</summary>
        public const ushort PreExplosionWhiteLimit = 0x9200;
        /// <summary>Radius at which yellow explosion switches to white shapes.</summary>
        public const ushort ExplosionYellowLimit = 0x8600;
        /// <summary>Crystal Flash's shorter explosion radius limit.</summary>
        public const ushort CrystalFlashRadiusLimit = 0x2000;
    }

    /// <summary>Shared trigonometric and boost-state constants used by shinespark.</summary>
    public static class Shinespark
    {
        /// <summary><c>$90:D2BD</c>, EndSuperJump's minimum energy to sustain a native shinespark.</summary>
        public const ushort MinimumSustainingEnergy = 30;
        /// <summary><c>$A0:B443</c>, positive-half signed sine samples.</summary>
        public const int PositiveSineTable = 0xa0b443;
        /// <summary>High-byte state identifying a fully active Speed Booster.</summary>
        public const ushort ActiveSpeedBoostCounter = 0x0400;
    }
}

/// <summary>One immutable death-tile DMA source/destination pairing.</summary>
public readonly record struct SamusDeathTileSegment(
    int SourceAddress,
    ushort EncodedVramDestination);
