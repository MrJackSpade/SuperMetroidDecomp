using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>How far down the Boyon bounce curve rises before it saturates, and the bounces its arcs allow.</summary>
public static class BoyonSpeedDefinitions
{
    /// <summary>
    /// The curve's rising samples from its top (#2369): at place k the sum of zero through k, k(k+1)/2, for the first 23
    /// places, ending at 253.
    /// </summary>
    public static ImmutableArray<int> RisingSamples { get; } =
        [0, 1, 3, 6, 10, 15, 21, 28, 36, 45, 55, 66, 78, 91, 105, 120, 136, 153, 171, 190, 210, 231, 253];

    /// <summary>
    /// The sample at every place past the <see cref="RisingSamples"/>: the curve's saturated height, which the game
    /// substitutes from the twenty-fourth place down.
    /// </summary>
    public const int SaturatedSample = 255;

}
