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
}
