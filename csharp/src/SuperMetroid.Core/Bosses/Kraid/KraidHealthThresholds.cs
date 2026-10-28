namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The health thresholds Kraid's initialization derives from its initial health.</summary>
internal static class KraidHealthThresholds
{
    /// <summary>$A7:AA02-AA16: truncate health/8, then accumulate eight multiples.</summary>
    public static int Eighth(int initialHealth, int index)
    {
        if ((uint)index >= KraidHealthThresholdDefinitions.EighthCount) throw new ArgumentOutOfRangeException(nameof(index));
        return (initialHealth >> KraidHealthThresholdDefinitions.EighthShift) * (index + 1);
    }
}
