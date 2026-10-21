using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Bounded adjacent-table and instruction data consumed by Chainsaw beam selection.</summary>
internal static class ChainsawBeamGraphicsDefinitions
{
    /// <summary>$000D: Plasma/Spazer/Wave indexes two words beyond the twelve-entry tile table at $90:C3B1.</summary>
    internal const SamusBeamCombination Selection = SamusBeamCombination.SpazerPlasmaWave;
    /// <summary>$9A:C421: $90:C3CB (the second palette-pointer word) reinterpreted by $90:AC8D as the tile pointer.</summary>
    internal const int TileSource = 0x9ac421;

    /// <summary>
    /// The sixteen colours of the glitched Chainsaw beam palette, in colour order, as red, green and blue five-bit
    /// channels. Its palette pointer lands just before Samus_Animate, so the loader reads an open-bus byte followed
    /// by that routine's instruction bytes; these are those words as CGRAM keeps them, without the bit it discards.
    /// The verification derives every colour from the cartridge bytes.
    /// </summary>
    internal static readonly IReadOnlyList<Bgr555> Palette = Array.AsReadOnly<Bgr555>(
    [
        new(16, 4, 2),
        new(2, 6, 12),
        new(2, 1, 22),
        new(12, 7, 4),
        new(13, 21, 27),
        new(25, 8, 10),
        new(15, 0, 0),
        new(10, 5, 31),
        new(7, 3, 0),
        new(13, 5, 7),
        new(10, 8, 18),
        new(13, 2, 0),
        new(16, 15, 6),
        new(9, 22, 19),
        new(0, 0, 28),
        new(20, 8, 11),
    ]);
}
