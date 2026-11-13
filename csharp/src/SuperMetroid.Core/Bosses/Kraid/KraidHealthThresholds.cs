namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The health thresholds Kraid's initialization derives from its initial health.</summary>
internal static class KraidHealthThresholds
{
    /// <summary>Truncate health/8, then accumulate eight multiples.</summary>
    public static int Eighth(int initialHealth, int index)
    {
        if ((uint)index >= KraidHealthThresholdDefinitions.EighthCount) throw new ArgumentOutOfRangeException(nameof(index));
        return Ensure.AtLeastZero(initialHealth) / KraidHealthThresholdDefinitions.EighthCount * (index + 1);
    }
}
