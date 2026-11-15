namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The health bands that select Golden Torizo's hurt colours.</summary>
internal static class TorizoCombatDefinitions
{
    /// <summary>The health each band spans: the band steps once per this much health.</summary>
    internal const int HealthPerBand = 2048;

    /// <summary>The last installed health band; every band of the upper half (16384 health and over) shows it.</summary>
    internal const int LastHealthBand = 7;
}
