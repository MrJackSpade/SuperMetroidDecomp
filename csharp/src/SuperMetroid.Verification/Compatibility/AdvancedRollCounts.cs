namespace SuperMetroid.Verification.Compatibility;

/// <summary>
/// The enemy counts the checks of rolls advanced by an enemy's frame counter (#1619) compare every sample at: every count
/// through one full round of an eight-bit roll and past it, and the counts at the top of the counter's range, where it wraps.
/// </summary>
internal static class AdvancedRollCounts
{
    /// <summary>Counts 0 through 257 and 65534 through 65535.</summary>
    internal static int[] All { get; } = [.. Enumerable.Range(0, 258), 65534, 65535];
}
