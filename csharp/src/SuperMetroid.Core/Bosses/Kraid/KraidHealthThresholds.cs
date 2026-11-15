namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The health thresholds Kraid's initialization derives from its initial health.</summary>
internal static class KraidHealthThresholds
{
    /// <summary>
    /// <paramref name="eighths"/> eighths of <paramref name="initialHealth"/> (#1890): the initial health truncated to one
    /// eighth, multiplied by the number of eighths.
    /// </summary>
    /// <param name="initialHealth">The health Kraid initialized with.</param>
    /// <param name="eighths">How many eighths, one through eight.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="eighths"/> is not one through eight.</exception>
    public static int Eighths(int initialHealth, int eighths) =>
        Ensure.AtLeastZero(initialHealth) / KraidHealthThresholdDefinitions.EighthCount
        * Ensure.BetweenInclusive(1, KraidHealthThresholdDefinitions.EighthCount, eighths);
}
