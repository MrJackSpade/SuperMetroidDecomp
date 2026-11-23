using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>The samples of the Boyon's bounce curve, and how far down it rises before it saturates.</summary>
public static partial class BoyonSpeedDefinitions
{
    /// <summary>
    /// The curve's rising speeds from its top (#2369, #2555), before the Boyon's multiplier scales them: at place k the sum
    /// of zero through k, k(k+1)/2, of the finest steps a speed takes, a 256th of a pixel an update each, for each place
    /// it lists, ending just short of a pixel.
    /// </summary>
    public static ImmutableArray<UnsignedSixteenSixteen> RisingSamples { get; } =
        [.. Enumerable.Range(0, RisingSampleCount).Select(place => UnsignedSixteenSixteen.FromPixelFraction(place * (place + 1) / 2, 256))];

    /// <summary>How many places the curve rises through before it saturates.</summary>
    private const int RisingSampleCount = 23;

    /// <summary>
    /// The speed at every place past the <see cref="RisingSamples"/>: the curve's saturated height, which the game
    /// substitutes from the twenty-fourth place down: one 256th short of a pixel an update.
    /// </summary>
    public static UnsignedSixteenSixteen SaturatedSample { get; } = UnsignedSixteenSixteen.FromPixelFraction(255, 256);

}
