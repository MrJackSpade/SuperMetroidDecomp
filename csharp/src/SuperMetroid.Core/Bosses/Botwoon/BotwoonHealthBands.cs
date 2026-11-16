namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's health-band progression: when his health has fallen past the next band's threshold.</summary>
internal static class BotwoonHealthBands
{
    /// <summary>The health band last shown after <paramref name="bandsPassed"/> thresholds were passed, or null before the first.</summary>
    internal static int? LastBandShown(int bandsPassed) => bandsPassed > 0 ? bandsPassed - 1 : null;

    /// <summary>
    /// Returns whether the current health is below the threshold that follows <paramref name="bandsPassed"/> passed
    /// thresholds, zero through seven.
    /// </summary>
    /// <remarks>
    /// The threshold after i passed thresholds is 375 times (8 - i), for i from zero through seven. Eight passed
    /// thresholds means Botwoon's health bands are done, which the fight decides itself; there is no ninth threshold.
    /// </remarks>
    public static bool ShouldAdvance(int bandsPassed, int health)
    {
        if (bandsPassed is < 0 or >= BotwoonHealthBandDefinitions.BandCount)
            throw new InvalidDataException($"Botwoon has passed {bandsPassed} health thresholds, outside the eight he has.");

        int threshold = BotwoonHealthBandDefinitions.HealthPerThreshold * (BotwoonHealthBandDefinitions.BandCount - bandsPassed);
        return health < threshold;
    }
}
