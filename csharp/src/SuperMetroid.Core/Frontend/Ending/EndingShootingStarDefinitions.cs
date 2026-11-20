using SuperMetroid.Core.Frontend.Ending.Artwork;
using static SuperMetroid.Core.Frontend.Ending.EndingShootingStarDefinition;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>Post-credits star motion and animation data.</summary>
internal static class EndingShootingStarDefinitions
{
    /// <summary>The colouring every star draws in.</summary>
    internal const EndingSpriteColouring Colouring = EndingSpriteColouring.ShootingStars;
    /// <summary>The stars the shooting-star setup creates.</summary>
    internal static int Count => stars.Length;
    /// <summary>Initialization and reset put both coordinates at the screen center.</summary>
    internal const int Origin = 128;
    /// <summary>The updates of the hidden lead-in each flight starts with, at the origin.</summary>
    internal const int LeadInDuration = 32;
    /// <summary>How far before its position each small sprite is centered.</summary>
    internal const int SpriteOffset = 4;
    /// <summary>
    /// The last pixel of the square field the stars fly in, on both axes: the screen's width, so a star leaving through
    /// the bottom of the screen flies on until it is as far down as the right edge is across. A star past it, or nearer
    /// the top or left than <see cref="SpriteOffset"/>, returns to the origin.
    /// </summary>
    internal const int FieldFarEdge = ScreenGeometry.ScreenWidthPixels - 1;
    /// <summary>The <see cref="SpriteOffset"/> centering applied on both axes.</summary>
    internal static Offset16<ScreenPixels> SpriteCenter => Offset16<ScreenPixels>.Of(SpriteOffset, SpriteOffset);
    /// <summary>The shooting stars: X/Y acceleration, animation period (none for a still star), initial delay.</summary>
    private static readonly EndingShootingStarDefinition[] stars = [
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(1, 16), SignedEightEight.FromPixelFraction(-1, 16)), 6, 8),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(3, 64), SignedEightEight.FromPixelFraction(-1, 256)), 6, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(13, 256), SignedEightEight.FromPixelFraction(-5, 128)), 6, 6),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(1, 32), SignedEightEight.FromPixelFraction(-1, 16)), 6, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(1, 128), SignedEightEight.FromPixelFraction(-7, 128)), 6, 8),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(1, 16), SignedEightEight.FromPixelFraction(-1, 32)), 6, 0),
        Still(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(1, 128), SignedEightEight.FromPixelFraction(-1, 256)), 0),
        Still(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(1, 256), SignedEightEight.FromPixelFraction(-1, 128)), 0),
        Still(new PixelVelocity<ScreenPixels>(SignedEightEight.Zero, SignedEightEight.FromPixelFraction(-3, 256)), 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(1, 128), SignedEightEight.FromPixelFraction(-5, 256)), 32, 4),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(15, 256), SignedEightEight.FromPixelFraction(1, 16)), 6, 8),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(1, 128), SignedEightEight.FromPixelFraction(9, 256)), 8, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(3, 64), SignedEightEight.FromPixelFraction(1, 32)), 6, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(1, 16), SignedEightEight.FromPixelFraction(1, 64)), 6, 8),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(1, 32), SignedEightEight.FromPixelFraction(3, 64)), 8, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(1, 128), SignedEightEight.FromPixelFraction(1, 32)), 8, 0),
        Still(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(1, 128), SignedEightEight.FromPixelFraction(1, 64)), 0),
        Still(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(1, 128), SignedEightEight.FromPixelFraction(1, 256)), 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-13, 256), SignedEightEight.FromPixelFraction(13, 256)), 4, 8),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-7, 128), SignedEightEight.FromPixelFraction(1, 32)), 4, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-7, 256), SignedEightEight.FromPixelFraction(7, 128)), 6, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-5, 256), SignedEightEight.FromPixelFraction(1, 16)), 4, 8),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-1, 64), SignedEightEight.FromPixelFraction(5, 256)), 32, 0),
        Still(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-1, 128), SignedEightEight.FromPixelFraction(3, 256)), 0),
        Still(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-1, 128), SignedEightEight.FromPixelFraction(1, 64)), 0),
        Still(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-3, 256), SignedEightEight.FromPixelFraction(1, 256)), 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-3, 64), SignedEightEight.FromPixelFraction(1, 128)), 4, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-3, 128), SignedEightEight.FromPixelFraction(5, 256)), 32, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-3, 256), SignedEightEight.FromPixelFraction(1, 32)), 32, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-7, 128), SignedEightEight.FromPixelFraction(-9, 256)), 4, 8),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-1, 32), SignedEightEight.FromPixelFraction(-3, 64)), 4, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-3, 128), SignedEightEight.FromPixelFraction(-7, 128)), 6, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-3, 64), SignedEightEight.FromPixelFraction(-1, 16)), 4, 8),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-1, 32), SignedEightEight.FromPixelFraction(-1, 64)), 32, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-3, 256), SignedEightEight.FromPixelFraction(-3, 128)), 32, 0),
        Animated(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-7, 256), SignedEightEight.FromPixelFraction(-1, 32)), 32, 0),
        Still(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-1, 128), SignedEightEight.FromPixelFraction(-3, 256)), 0),
        Still(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-1, 64), SignedEightEight.FromPixelFraction(-1, 64)), 0),
        Still(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-1, 32), SignedEightEight.FromPixelFraction(-1, 128)), 0),
        Still(new PixelVelocity<ScreenPixels>(SignedEightEight.FromPixelFraction(-1, 32), SignedEightEight.FromPixelFraction(-1, 64)), 0)
    ];
    /// <summary>The stars in order, for their setup and their flights.</summary>
    internal static ReadOnlySpan<EndingShootingStarDefinition> Stars => stars;

    /// <summary>
    /// The name each picture a visible star shows is installed under (#2202): a faint speck, a bright speck, a dim star and
    /// the full star.
    /// </summary>
    internal static readonly IReadOnlyDictionary<EndingShootingStarLook, string> SheetFrameNames = new Dictionary<EndingShootingStarLook, string>
    {
        [EndingShootingStarLook.FaintSpeck] = "ShootingStar.FaintSpeck",
        [EndingShootingStarLook.BrightSpeck] = "ShootingStar.BrightSpeck",
        [EndingShootingStarLook.DimStar] = "ShootingStar.DimStar",
        [EndingShootingStarLook.FullStar] = "ShootingStar.FullStar",
    };
}
