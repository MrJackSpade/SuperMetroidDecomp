namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Native post-credits star motion and animation data.</summary>
internal static class EndingShootingStarDefinitions
{
    /// <summary>$8B:E80B / Initialize_ShootingStars creates forty sixteen-byte records.</summary>
    internal const int Count = 40;
    /// <summary>$8B:E7F2 and $E983 initialize/reset both coordinates to the screen center.</summary>
    internal const ushort Origin = 0x80;
    /// <summary>$8B:E7EB and $E97D initialize/reset the invisible animation lead-in.</summary>
    internal const ushort InitialTimer = 0x20;
    /// <summary>$8B:E84A doubles acceleration from the second visible animation frame.</summary>
    internal const int DoubleAccelerationFrame = 2;
    /// <summary>$8B:E90B/$E91F center each small OBJ four pixels before its position.</summary>
    internal const ushort SpriteOffset = 4;
    /// <summary>The <see cref="SpriteOffset"/> centering applied on both axes.</summary>
    internal static Offset16<ScreenPixels> SpriteCenter => new((short)SpriteOffset, (short)SpriteOffset);
    /// <summary>$8B:E9CF / ShootingStar_Table: X/Y acceleration, animation period, initial delay.</summary>
    private static readonly EndingShootingStarDefinition[] records = [
        new(new(16, -16), 6, 8), new(new(12, -1), 6, 0), new(new(13, -10), 6, 6), new(new(8, -16), 6, 0),
        new(new(2, -14), 6, 8), new(new(16, -8), 6, 0), new(new(2, -1), 0x0f00, 0), new(new(1, -2), 0x0f00, 0),
        new(new(0, -3), 0x0f00, 0), new(new(2, -5), 32, 4), new(new(15, 16), 6, 8), new(new(2, 9), 8, 0),
        new(new(12, 8), 6, 0), new(new(16, 4), 6, 8), new(new(8, 12), 8, 0), new(new(2, 8), 8, 0),
        new(new(2, 4), 0x0f00, 0), new(new(2, 1), 0x0f00, 0), new(new(-13, 13), 4, 8), new(new(-14, 8), 4, 0),
        new(new(-7, 14), 6, 0), new(new(-5, 16), 4, 8), new(new(-4, 5), 32, 0), new(new(-2, 3), 0x0f00, 0),
        new(new(-2, 4), 0x0f00, 0), new(new(-3, 1), 0x0f00, 0), new(new(-12, 2), 4, 0), new(new(-6, 5), 32, 0),
        new(new(-3, 8), 32, 0), new(new(-14, -9), 4, 8), new(new(-8, -12), 4, 0), new(new(-6, -14), 6, 0),
        new(new(-12, -16), 4, 8), new(new(-8, -4), 32, 0), new(new(-3, -6), 32, 0), new(new(-7, -8), 32, 0),
        new(new(-2, -3), 0x0f00, 0), new(new(-4, -4), 0x0f00, 0), new(new(-8, -2), 0x0f00, 0), new(new(-8, -4), 0x0f00, 0)
    ];
    /// <summary>Exposes the forty native $8B:E9CF star records in order for initialization and motion updates.</summary>
    internal static ReadOnlySpan<EndingShootingStarDefinition> Records => records;
    /// <summary>The animation frames $8B:E9A7 (Handle_ShootingStars.tilemapValues) gives attributes for.</summary>
    internal const int FrameCount = 20;

    /// <summary>The first animation frame showing the full star; every later frame shows it too.</summary>
    internal const int FullStarFrame = 4;

    /// <summary>The visible animation frames with a sheet frame of their own: the growing star's steps and the full star.</summary>
    internal static IEnumerable<int> SheetFrames => Enumerable.Range(1, FullStarFrame);

    /// <summary>The sheet frame showing a visible animation frame (#1392).</summary>
    internal static string SheetFrameName(int frame) => frame is > 0 and < FrameCount
        ? $"ShootingStar.{Math.Min(frame, FullStarFrame)}"
        : throw new ArgumentOutOfRangeException(nameof(frame));

    /// <summary>
    /// $8B:E9A7 / Handle_ShootingStars.tilemapValues: frame 0 is the blank word, frames 1 to 3 grow through characters
    /// $1F0 to $1F2, and every later frame holds the full star at $1F3, all in sprite palette 4.
    /// </summary>
    internal static SnesObjAttributeWord Attribute(int frame) => frame switch
    {
        0 => default,
        > 0 and < FrameCount => SnesObjAttributeWord.Create(SnesCharacterGrid.Cell(31, Math.Min(frame - 1, 3)), 4, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(frame)),
    };
}
