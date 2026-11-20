using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>How far down the Boyon bounce curve rises before it saturates, and the bounces its arcs allow.</summary>
public static class BoyonSpeedDefinitions
{
    /// <summary>
    /// The curve's rising speeds from its top (#2369, #2555), before the Boyon's multiplier scales them: at place k the sum
    /// of zero through k, k(k+1)/2, of the finest steps a speed takes, a 256th of a pixel an update each, for each place
    /// it lists, ending just short of a pixel.
    /// </summary>
    public static ImmutableArray<UnsignedSixteenSixteen> RisingSamples { get; } =
    [
        UnsignedSixteenSixteen.Zero, UnsignedSixteenSixteen.FromPixelFraction(1, 256), UnsignedSixteenSixteen.FromPixelFraction(3, 256), UnsignedSixteenSixteen.FromPixelFraction(3, 128),
        UnsignedSixteenSixteen.FromPixelFraction(5, 128), UnsignedSixteenSixteen.FromPixelFraction(15, 256), UnsignedSixteenSixteen.FromPixelFraction(21, 256), UnsignedSixteenSixteen.FromPixelFraction(7, 64),
        UnsignedSixteenSixteen.FromPixelFraction(9, 64), UnsignedSixteenSixteen.FromPixelFraction(45, 256), UnsignedSixteenSixteen.FromPixelFraction(55, 256), UnsignedSixteenSixteen.FromPixelFraction(33, 128),
        UnsignedSixteenSixteen.FromPixelFraction(39, 128), UnsignedSixteenSixteen.FromPixelFraction(91, 256), UnsignedSixteenSixteen.FromPixelFraction(105, 256), UnsignedSixteenSixteen.FromPixelFraction(15, 32),
        UnsignedSixteenSixteen.FromPixelFraction(17, 32), UnsignedSixteenSixteen.FromPixelFraction(153, 256), UnsignedSixteenSixteen.FromPixelFraction(171, 256), UnsignedSixteenSixteen.FromPixelFraction(95, 128),
        UnsignedSixteenSixteen.FromPixelFraction(105, 128), UnsignedSixteenSixteen.FromPixelFraction(231, 256), UnsignedSixteenSixteen.FromPixelFraction(253, 256)];

    /// <summary>
    /// The speed at every place past the <see cref="RisingSamples"/>: the curve's saturated height, which the game
    /// substitutes from the twenty-fourth place down: one 256th short of a pixel an update.
    /// </summary>
    public static UnsignedSixteenSixteen SaturatedSample { get; } = UnsignedSixteenSixteen.FromPixelFraction(255, 256);

}
