namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Shared Ceres/Norfair target-seeking acceleration divisors.</summary>
public static partial class RidleyInertiaDefinitions
{
    /// <summary>Moving Ridley to his death spot divides by the largest divisor.</summary>
    public const int DeathDivisor = LargestDivisor;

    /// <summary>
    /// Moving Ridley to his death spot adds a sixteenth of a pixel an update to reversal deceleration, independently of Y.
    /// </summary>
    public static SignedEightEight DeathReversalBoost { get; } = SignedEightEight.FromPixelFraction(1, 16);

    /// <summary>The fastest positive speed either Ridley's seek allows, five pixels per frame.</summary>
    public static SignedEightEight MaximumVelocity { get; } = SignedEightEight.FromPixels(5);

    /// <summary>The fastest negative speed either Ridley's seek allows, five pixels per frame.</summary>
    public static SignedEightEight MinimumVelocity { get; } = SignedEightEight.FromPixels(-5);

    /// <summary>Base acceleration added when reversing direction: a thirty-second of a pixel an update.</summary>
    public static SignedEightEight ReversalAcceleration { get; } = SignedEightEight.FromPixelFraction(1, 32);

    /// <summary>The largest acceleration divisor, the slowest seek; Ceres and Norfair share the range.</summary>
    public const int LargestDivisor = 16;

    /// <summary>The smallest acceleration divisor, the fastest seek.</summary>
    public const int SmallestDivisor = 1;
}
