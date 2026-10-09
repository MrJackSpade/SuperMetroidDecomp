using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Ports $AD:E3D5's strict health comparisons and three palette copies.</summary>
public static class MotherBrainHealthPalette
{
    /// <summary>Selects the damage band from <c>MotherBrainHealthBasedPaletteHandling</c> at $AD:E3D5 and writes installed body, brain, and rear-leg colors to CGRAM.</summary>
    /// <param name="cgram">Mutable destination: body/brain colors replace indices 65 through 79 and 145 through 159, and rear-leg colors replace 177 through 191; transparent slots are preserved.</param>
    /// <param name="health">Unsigned health from Mother Brain's head slot: bands are at least 9000, 5400 through 8999, 1800 through 5399, and below 1800.</param>
    /// <param name="presentation">Required installed four-band palette artwork; the optional signature does not supply a fallback when this is <see langword="null"/>.</param>
    /// <remarks>The caller owns the native recovered-corpse-state gate; this method applies the selected band on every call without checking encounter phase.</remarks>
    /// <exception cref="InvalidOperationException"><paramref name="presentation"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="cgram"/> is <see langword="null"/> and presentation artwork is available.</exception>
    public static void Apply(SnesCgram cgram, ushort health,
        MotherBrainHealthPalettePresentation? presentation = null)
    {
        int index = health >= MotherBrainHealthPaletteRomData.FirstThreshold ? 0 :
            health >= MotherBrainHealthPaletteRomData.SecondThreshold ? 1 :
            health >= MotherBrainHealthPaletteRomData.FinalThreshold ? 2 : 3;
        (presentation ?? throw new InvalidOperationException(
            "Mother Brain health palettes require installed presentation assets."))
            .Apply(cgram, index);
    }
}
