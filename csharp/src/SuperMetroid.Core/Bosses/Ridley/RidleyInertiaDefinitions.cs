namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Shared Ceres/Norfair target-seeking acceleration divisors.</summary>
public static class RidleyInertiaDefinitions
{
    /// <summary>Moving Ridley to his death spot divides by the largest divisor.</summary>
    public const int DeathDivisor = LargestDivisor;

    /// <summary>Moving Ridley to his death spot adds sixteen to reversal deceleration, independently of Y.</summary>
    public const int DeathReversalBoost = 16;

    /// <summary>The fastest positive speed the Norfair acceleration routines allow, five pixels per frame.</summary>
    public static SignedEightEight MaximumVelocity { get; } = SignedEightEight.FromPixels(5);

    /// <summary>The fastest negative speed, five pixels per frame, compared with the N flag.</summary>
    public static SignedEightEight MinimumVelocity { get; } = SignedEightEight.FromPixels(-5);

    /// <summary>Base acceleration added when reversing direction.</summary>
    public const int ReversalAcceleration = 8;

    /// <summary>The largest acceleration divisor, the slowest seek; Ceres and Norfair share the range.</summary>
    public const int LargestDivisor = 16;

    /// <summary>The smallest acceleration divisor, the fastest seek.</summary>
    public const int SmallestDivisor = 1;
}
