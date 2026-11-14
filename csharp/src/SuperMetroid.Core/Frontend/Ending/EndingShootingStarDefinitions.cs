using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Post-credits star motion and animation data.</summary>
internal static class EndingShootingStarDefinitions
{
    /// <summary>The colouring every star draws in.</summary>
    internal const EndingSpriteColouring Colouring = EndingSpriteColouring.ShootingStars;
    /// <summary>The shooting-star setup creates forty records.</summary>
    internal const int Count = 40;
    /// <summary>Initialization and reset put both coordinates at the screen center.</summary>
    internal const int Origin = 128;
    /// <summary>Initialization and reset start the invisible animation lead-in.</summary>
    internal const int InitialTimer = 32;
    /// <summary>Acceleration doubles from the second visible animation frame.</summary>
    internal const int DoubleAccelerationFrame = 2;
    /// <summary>Each small sprite is centered four pixels before its position.</summary>
    internal const int SpriteOffset = 4;
    /// <summary>
    /// The last pixel of the square field the stars fly in, on both axes: the screen's width, so a star leaving through
    /// the bottom of the screen flies on until it is as far down as the right edge is across. A star past it, or nearer
    /// the top or left than <see cref="SpriteOffset"/>, returns to the origin.
    /// </summary>
    internal const int FieldFarEdge = ScreenGeometry.ScreenWidthPixels - 1;
    /// <summary>The <see cref="SpriteOffset"/> centering applied on both axes.</summary>
    internal static Offset16<ScreenPixels> SpriteCenter => new(CoordinateAxis.AsSigned(SpriteOffset), CoordinateAxis.AsSigned(SpriteOffset));
    /// <summary>The shooting stars: X/Y acceleration, animation period, initial delay.</summary>
    private static readonly EndingShootingStarDefinition[] stars = [
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(16, -16), 6, 8),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(12, -1), 6, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(13, -10), 6, 6),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(8, -16), 6, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(2, -14), 6, 8),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(16, -8), 6, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(2, -1), 3840, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(1, -2), 3840, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(0, -3), 3840, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(2, -5), 32, 4),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(15, 16), 6, 8),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(2, 9), 8, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(12, 8), 6, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(16, 4), 6, 8),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(8, 12), 8, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(2, 8), 8, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(2, 4), 3840, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(2, 1), 3840, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-13, 13), 4, 8),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-14, 8), 4, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-7, 14), 6, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-5, 16), 4, 8),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-4, 5), 32, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-2, 3), 3840, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-2, 4), 3840, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-3, 1), 3840, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-12, 2), 4, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-6, 5), 32, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-3, 8), 32, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-14, -9), 4, 8),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-8, -12), 4, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-6, -14), 6, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-12, -16), 4, 8),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-8, -4), 32, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-3, -6), 32, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-7, -8), 32, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-2, -3), 3840, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-4, -4), 3840, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-8, -2), 3840, 0),
        new(EightEightVector<ScreenPixels>.FromTwoHundredFiftySixthsOfPixel(-8, -4), 3840, 0)
    ];
    /// <summary>Exposes the forty star records in order for initialization and motion updates.</summary>
    internal static ReadOnlySpan<EndingShootingStarDefinition> Stars => stars;
    /// <summary>The animation frames a shooting star shows.</summary>
    internal const int FrameCount = 20;

    /// <summary>The first animation frame showing the full star; every later frame shows it too.</summary>
    internal const int FullStarFrame = 4;
}
