namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>How far down the Boyon bounce curve rises before it saturates, and the bounces its arcs allow.</summary>
public static class BoyonSpeedDefinitions
{
    /// <summary>How many places from the curve's top rise: from the twenty-fourth place down, the curve saturates.</summary>
    public const int StoredSampleCount = 23;

    /// <summary>The sample at every place from <see cref="StoredSampleCount"/> down: the curve's saturated height.</summary>
    public const int SaturatedSample = 255;

    /// <summary>The largest scaled sample, <see cref="SaturatedSample"/> times <see cref="LargestArcMultiplier"/>.</summary>
    public const int LargestScaledSample = SaturatedSample * LargestArcMultiplier;

    /// <summary>
    /// The highest arc a Boyon bounces, in whole pixels: the game's height test agrees with a plain
    /// comparison of the arc's distance with its height up to this height, and the Boyons' arcs are 48 to 96 pixels
    /// high.
    /// </summary>
    public const int LargestJumpHeight = 128;

    /// <summary>
    /// The largest multiplier a Boyon derives its arc with: up to it, one entry's scaled sample is under 128 pixels, so
    /// the arc's distance never passes its height by 128 pixels or more, where the game's height test would read it as
    /// still short of the height, and the derivation's plain comparison is the game's; the Boyons' multipliers are
    /// 8 to 16.
    /// </summary>
    public const int LargestArcMultiplier = 128;
}
