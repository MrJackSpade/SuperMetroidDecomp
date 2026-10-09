namespace SuperMetroid.Core.Frontend;

/// <summary>Native post-credits star motion and animation data.</summary>
internal static class EndingShootingStarDefinitions
{
    /// <summary>$8B:E80B / Initialize_ShootingStars creates forty sixteen-byte records.</summary>
    internal const int Count = 40;
    /// <summary>$8B:E7F2 and $E983 initialize/reset both coordinates to the screen center.</summary>
    internal const ushort Origin = 0x80;
    /// <summary>$8B:E7EB and $E97D initialize/reset the invisible animation lead-in.</summary>
    internal const ushort InitialTimer = 0x20;
    /// <summary>$8B:E949 advances the packed frame byte by two, indexing word attributes.</summary>
    internal const ushort AnimationIncrement = 0x0200;
    /// <summary>$8B:E84A doubles acceleration from the second visible animation frame.</summary>
    internal const ushort DoubleAccelerationFrame = 0x0400;
    /// <summary>$8B:E90B/$E91F center each small OBJ four pixels before its position.</summary>
    internal const ushort SpriteOffset = 4;
    /// <summary>$8B:E9CF / ShootingStar_Table: X/Y acceleration, animation period, initial delay.</summary>
    private static readonly EndingShootingStarDefinition[] records = [
        new(16,-16,6,8), new(12,-1,6,0), new(13,-10,6,6), new(8,-16,6,0),
        new(2,-14,6,8), new(16,-8,6,0), new(2,-1,0x0f00,0), new(1,-2,0x0f00,0),
        new(0,-3,0x0f00,0), new(2,-5,32,4), new(15,16,6,8), new(2,9,8,0),
        new(12,8,6,0), new(16,4,6,8), new(8,12,8,0), new(2,8,8,0),
        new(2,4,0x0f00,0), new(2,1,0x0f00,0), new(-13,13,4,8), new(-14,8,4,0),
        new(-7,14,6,0), new(-5,16,4,8), new(-4,5,32,0), new(-2,3,0x0f00,0),
        new(-2,4,0x0f00,0), new(-3,1,0x0f00,0), new(-12,2,4,0), new(-6,5,32,0),
        new(-3,8,32,0), new(-14,-9,4,8), new(-8,-12,4,0), new(-6,-14,6,0),
        new(-12,-16,4,8), new(-8,-4,32,0), new(-3,-6,32,0), new(-7,-8,32,0),
        new(-2,-3,0x0f00,0), new(-4,-4,0x0f00,0), new(-8,-2,0x0f00,0), new(-8,-4,0x0f00,0)
    ];
    /// <summary>Gets the forty star-motion records in their native slot order.</summary>
    internal static ReadOnlySpan<EndingShootingStarDefinition> Records => records;
    /// <summary>$8B:E9A7 / Handle_ShootingStars.tilemapValues: small OBJ tile/attribute words.</summary>
    internal static ReadOnlySpan<ushort> Attributes => [
        0, 0x09f0, 0x09f1, 0x09f2, 0x09f3, 0x09f3, 0x09f3, 0x09f3,
        0x09f3, 0x09f3, 0x09f3, 0x09f3, 0x09f3, 0x09f3, 0x09f3, 0x09f3,
        0x09f3, 0x09f3, 0x09f3, 0x09f3
    ];
}

/// <summary>Motion and timing parameters used to initialize and advance one post-credits shooting star.</summary>
/// <param name="XAcceleration">Signed 8.8 horizontal velocity increment applied on each active update.</param>
/// <param name="YAcceleration">Signed 8.8 vertical velocity increment applied on each active update.</param>
/// <param name="Period">Update interval loaded after each animation-frame advance.</param>
/// <param name="Delay">Initial launch delay; zero starts the star immediately.</param>
internal readonly record struct EndingShootingStarDefinition(short XAcceleration, short YAcceleration, ushort Period, ushort Delay);
