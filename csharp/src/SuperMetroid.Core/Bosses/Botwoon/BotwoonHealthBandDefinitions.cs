namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// Botwoon's health bands (#1544): each threshold his health falls past shows the next band's colours, which the import
/// pre-colours his head and body in.
/// </summary>
internal static class BotwoonHealthBandDefinitions
{
    /// <summary>His health bands, one per threshold; once he has passed them all, his colours stop changing.</summary>
    public const int BandCount = 8;

    /// <summary>The health between successive thresholds: threshold(i) = 375 * (8 - i).</summary>
    public const int HealthPerThreshold = 375;
}
