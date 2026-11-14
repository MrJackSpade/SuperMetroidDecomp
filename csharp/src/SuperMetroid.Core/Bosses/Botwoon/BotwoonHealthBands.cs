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
    /// Independently reviewed for #1165 against the Japanese and North American v1.0 release:
    /// threshold(i)=375*(8-i), i=0..7. Botwoon's health never exceeds its 3000 at spawn, so the
    /// game's signed comparison of the difference is the plain comparison. Eight passed thresholds is caller-owned
    /// completion, not an extrapolated threshold.
    /// </remarks>
    public static bool ShouldAdvance(int bandsPassed, int health)
    {
        if (bandsPassed is < 0 or >= BotwoonHealthBandDefinitions.BandCount)
            throw new InvalidDataException($"Botwoon has passed {bandsPassed} health thresholds, outside the eight he has.");

        int threshold = BotwoonHealthBandDefinitions.HealthPerThreshold * (BotwoonHealthBandDefinitions.BandCount - bandsPassed);
        return health < threshold;
    }
}
