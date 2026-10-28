namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The health bands that select Golden Torizo's hurt palette row.</summary>
internal static class TorizoCombatDefinitions
{
    /// <summary>The health each palette band spans: the row steps once per this much health.</summary>
    internal const int HealthPerPaletteBand = 2048;

    /// <summary>The bands health counts through before they repeat (health bit 15 does not select a row).</summary>
    internal const int HealthPaletteBands = 16;

    /// <summary>The last palette row; every row of the upper half (health bit 14 set) uses it.</summary>
    internal const int LastHealthPaletteRow = 7;
}
