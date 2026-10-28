namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The acceleration divisors Ceres and Norfair Ridley seek their targets with.</summary>
public static class RidleyInertia
{
    /// <summary>The divisor at inertia index <paramref name="index"/>, zero through fifteen.</summary>
    public static ushort Divisor(int index) => (uint)index < (uint)RidleyInertiaDefinitions.Divisors.Length
        ? RidleyInertiaDefinitions.Divisors[index]
        : throw new ArgumentOutOfRangeException(nameof(index));
}
