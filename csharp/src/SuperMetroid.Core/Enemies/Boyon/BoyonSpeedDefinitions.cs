namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>The Boyon bounce curve's stored length.</summary>
public static class BoyonSpeedDefinitions
{
    /// <summary>The bounce clamps indices at or above 23 to 255 before multiplication.</summary>
    public const int StoredSampleCount = 23;

    /// <summary>The sample every index at or past <see cref="StoredSampleCount"/> reads: the curve's saturated height.</summary>
    public const int SaturatedSample = 255;

    /// <summary>The largest multiplier a bounce curve takes: the multiply reads it whole, so a larger one is rejected.</summary>
    public const int LargestMultiplier = 255;

    /// <summary>The largest scaled sample, <see cref="SaturatedSample"/> times <see cref="LargestMultiplier"/>.</summary>
    public const int LargestScaledSample = SaturatedSample * LargestMultiplier;

    /// <summary>
    /// The highest arc a Boyon bounces, in whole pixels: the game's height test agrees with a plain
    /// comparison of the arc's distance with its height up to this height, and the retail Boyons' arcs are 48 to 96 pixels
    /// high.
    /// </summary>
    public const int LargestJumpHeight = 128;

    /// <summary>
    /// The largest multiplier a Boyon derives its arc with: up to it, one entry's scaled sample is under 128 pixels, so
    /// the arc's distance never passes its height by 128 pixels or more, where the game's height test would read it as
    /// still short of the height, and the derivation's plain comparison is the game's; the retail Boyons' multipliers are
    /// 8 to 16.
    /// </summary>
    public const int LargestArcMultiplier = 128;
}
