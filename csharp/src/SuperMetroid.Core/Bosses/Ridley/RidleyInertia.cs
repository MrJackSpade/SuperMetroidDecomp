namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The acceleration divisors Ceres and Norfair Ridley seek their targets with.</summary>
public static class RidleyInertia
{
    /// <summary>Accepts <paramref name="divisor"/> when it is one Ridley seeks with.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="divisor"/> is outside <see cref="RidleyInertiaDefinitions.SmallestDivisor"/> through <see cref="RidleyInertiaDefinitions.LargestDivisor"/>.</exception>
    public static int Divisor(int divisor) =>
        Ensure.BetweenInclusive(RidleyInertiaDefinitions.SmallestDivisor, RidleyInertiaDefinitions.LargestDivisor, divisor);
}
