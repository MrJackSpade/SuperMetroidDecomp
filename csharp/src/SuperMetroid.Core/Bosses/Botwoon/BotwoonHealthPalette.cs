namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>Botwoon's health-band palette progression: when its health has fallen past the next band's threshold.</summary>
internal static class BotwoonHealthPalette
{
    /// <summary>The health band last shown after <paramref name="bandsPassed"/> thresholds were passed, or null before the first.</summary>
    internal static int? LastBandShown(int bandsPassed) => bandsPassed > 0 ? bandsPassed - 1 : null;

    /// <summary>
    /// Returns whether the current health is below the threshold of palette phase <paramref name="phase"/>, zero
    /// through seven.
    /// </summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the NTSC J/U v1.0 release:
    /// threshold(i)=375*(8-i), i=0..7. Botwoon's health never exceeds its 3000 at spawn, so the
    /// game's signed comparison of the difference is the plain comparison. Phase 8 is caller-owned completion,
    /// not an extrapolated threshold.
    /// </remarks>
    public static bool ShouldAdvance(int phase, int health)
    {
        if (phase is < 0 or >= BotwoonHealthPaletteDefinitions.CompletePhase)
            throw new InvalidDataException($"Botwoon palette phase {phase} is outside the eight retail threshold entries.");

        int threshold = BotwoonHealthPaletteDefinitions.HealthPerThreshold * (BotwoonHealthPaletteDefinitions.PaletteCount - phase);
        return health < threshold;
    }
}
