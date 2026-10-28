namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The health bits that select Golden Torizo's hurt palette row.</summary>
internal static class TorizoCombatDefinitions
{
    /// <summary>Health bits 11-14: Golden Torizo's health palette row.</summary>
    internal const int HealthPaletteRowBits = 0x7800;

    /// <summary>The lowest health bit of the palette row.</summary>
    internal const int HealthPaletteRowShift = 11;

    /// <summary>The last palette row; every row of the upper half (health bit 14 set) uses it.</summary>
    internal const int LastHealthPaletteRow = 7;
}
