namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Compiled health-band policy for Draygon's palette handler. The selected color records
/// remain presentation data; <see cref="DraygonHealthPalette"/> selects a band from these thresholds.
/// </summary>
internal static class DraygonHealthPaletteDefinitions
{

    /// <summary>Draygon body's authored enemy-header health at $A0:DE43.</summary>
    public const int MaximumAuthoredHealth = 6000;

    /// <summary>The health between successive band thresholds: threshold(i) = 750 * (7 - i).</summary>
    public const int HealthPerBand = 750;

    /// <summary>The band of the lowest health, the last of the eight.</summary>
    public const int LowestHealthBand = 7;
}
