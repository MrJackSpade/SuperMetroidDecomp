namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Fixed words of Torizo's damage handling: the awakening shot guard and the health palette's row bits.</summary>
internal static class TorizoCombatDefinitions
{
    /// <summary>The native awakening guard value that rejects Bomb Torizo hits until the list clears it.</summary>
    internal const ushort AwakeningShotGuard = 0x7777;

    /// <summary>Health bits 11-14, as bits 3-6 of its high byte: Golden Torizo's health palette row, eight bytes apart.</summary>
    internal const int HealthPaletteRowBits = 0x78;

    /// <summary>Health bit 14, as bit 6 of its high byte: the upper half of the rows, which all use the last row.</summary>
    internal const int HealthPaletteUpperHalf = 0x40;
}
