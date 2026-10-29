namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The health bands that select Golden Torizo's hurt colours.</summary>
internal static class TorizoCombatDefinitions
{
    /// <summary>The health each palette band spans: the band steps once per this much health.</summary>
    internal const int HealthPerPaletteBand = 2048;

    /// <summary>The bands health counts through before they repeat (health bit 15 does not select a band).</summary>
    internal const int HealthPaletteBands = 16;

    /// <summary>The last installed health band; every band of the upper half (health bit 14 set) shows it.</summary>
    internal const int LastHealthBand = 7;
}
