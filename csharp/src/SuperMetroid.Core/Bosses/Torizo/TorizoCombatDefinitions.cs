namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The health bits that select Golden Torizo's hurt palette row.</summary>
internal static class TorizoCombatDefinitions
{
    /// <summary>Health bits 11-14, as bits 3-6 of its high byte: Golden Torizo's health palette row, eight bytes apart.</summary>
    internal const int HealthPaletteRowBits = 0x78;

    /// <summary>Health bit 14, as bit 6 of its high byte: the upper half of the rows, which all use the last row.</summary>
    internal const int HealthPaletteUpperHalf = 0x40;
}
