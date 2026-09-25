using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    [NonSerialized] private EndingPaletteCatalog? paletteArtwork;

    /// <summary>
    /// Rebinds authored ending colors after state restoration. The current CGRAM image
    /// remains serialized live state: overwriting it here would discard a partially
    /// completed native fade or palette-FX step. Future native palette transfers and
    /// target-color reads use the newly selected resource.
    /// </summary>
    internal void BindPaletteArtwork(EndingPaletteCatalog? value)
    {
        paletteArtwork = value;
        endingLogo?.BindPaletteArtwork(value);
    }

    private void LoadStaticPalette(EndingPaletteId id, int sourceColor, int count,
        int destinationColor)
    {
        if (paletteArtwork is { } artwork)
            artwork[id].LoadTo(cgram, sourceColor, count, destinationColor);
        else
            cgram.LoadFromBus(bus, EndingPaletteDefinitions.SourceAddress(id) +
                sourceColor * sizeof(ushort), count, destinationColor);
    }

    private ushort StaticPaletteColor(EndingPaletteId id, int color)
        => paletteArtwork is { } artwork
            ? artwork[id].Color(color)
            : RomDataReader.ReadWordFixedBank(bus,
                EndingPaletteDefinitions.SourceAddress(id) + color * sizeof(ushort));
}
