namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>Native Yard vertical launch words indexed by the player's whole horizontal displacement.</summary>
public static class YardKickDefinitions
{
    /// <summary>The highest whole speed that selects its own launch; faster speeds share the last.</summary>
    internal const int FastestKickSpeed = 15;

    /// <summary>
    /// The upward launch each whole horizontal speed selects, in sixteenths of a pixel per frame. The cartridge authors
    /// each launch as a whole pixel count, three at rest and one more every third speed, with a fraction of zero, five
    /// eighths or one quarter of a pixel added toward zero; these are the resulting signed launches.
    /// </summary>
    internal static readonly SignedSixteenSixteen[] Kicks =
    [
        SignedSixteenSixteen.FromSixteenthsOfPixel(-48),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-38),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-44),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-64),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-54),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-60),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-80),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-70),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-76),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-96),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-86),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-92),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-112),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-102),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-108),
        SignedSixteenSixteen.FromSixteenthsOfPixel(-128),
    ];
}
