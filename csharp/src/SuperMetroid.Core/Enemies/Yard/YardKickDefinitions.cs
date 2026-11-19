namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Yard vertical launch speeds indexed by the player's whole horizontal displacement.</summary>
public static class YardKickDefinitions
{
    /// <summary>The highest whole speed that selects its own launch; faster speeds share the last.</summary>
    internal const int FastestKickSpeed = 15;

    /// <summary>
    /// The upward launch each whole horizontal speed selects, in sixteenths of a pixel per frame. The game authors
    /// each launch as a whole pixel count, three at rest and one more every third speed, with a fraction of zero, five
    /// eighths or one quarter of a pixel added toward zero; these are the resulting signed launches.
    /// </summary>
    internal static readonly SignedSixteenSixteen[] Kicks =
    [
        SignedSixteenSixteen.FromWholePixels(-3),
        SignedSixteenSixteen.FromPixelFraction(-19, 8),
        SignedSixteenSixteen.FromPixelFraction(-11, 4),
        SignedSixteenSixteen.FromWholePixels(-4),
        SignedSixteenSixteen.FromPixelFraction(-27, 8),
        SignedSixteenSixteen.FromPixelFraction(-15, 4),
        SignedSixteenSixteen.FromWholePixels(-5),
        SignedSixteenSixteen.FromPixelFraction(-35, 8),
        SignedSixteenSixteen.FromPixelFraction(-19, 4),
        SignedSixteenSixteen.FromWholePixels(-6),
        SignedSixteenSixteen.FromPixelFraction(-43, 8),
        SignedSixteenSixteen.FromPixelFraction(-23, 4),
        SignedSixteenSixteen.FromWholePixels(-7),
        SignedSixteenSixteen.FromPixelFraction(-51, 8),
        SignedSixteenSixteen.FromPixelFraction(-27, 4),
        SignedSixteenSixteen.FromWholePixels(-8),
    ];
}
