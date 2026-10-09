using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    /// <summary>Installed source for authored ending palette colors used by transfers and fade targets.</summary>
    [NonSerialized] private EndingPaletteCatalog? paletteArtwork;
    /// <summary>Installed editable colors consumed by future ending palette-FX interpreter steps.</summary>
    [NonSerialized] private RoomPaletteFxPresentation? paletteFxArtwork;

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

    /// <summary>
    /// Uses the same editable bank-$8D color payloads as gameplay while leaving
    /// ending-specific object allocation, palette destinations and timing native.
    /// Rebinding affects future interpreter reads without resetting a live fade.
    /// </summary>
    internal void BindPaletteFxColors(RoomPaletteFxPresentation? value)
    {
        paletteFxArtwork = value;
    }

    /// <summary>Native scene boundaries clear slots but retain installed color ownership.</summary>
    private void ResetPaletteFx()
    {
        paletteFx = new RoomPaletteFxSystem();
    }

    /// <summary>Loads a selected range of authored ending colors into the live CGRAM image.</summary>
    /// <param name="id">Ending palette resource to transfer from.</param>
    /// <param name="sourceColor">First color index in the selected palette resource.</param>
    /// <param name="count">Number of consecutive colors to transfer.</param>
    /// <param name="destinationColor">First destination color index in CGRAM.</param>
    private void LoadStaticPalette(EndingPaletteId id, int sourceColor, int count,
        int destinationColor)
    {
        (paletteArtwork ?? throw new InvalidOperationException(
            "Ending palettes require installed artwork."))[id].LoadTo(cgram, sourceColor, count, destinationColor);
    }

    /// <summary>Reads an authored ending palette color for use as a transition target.</summary>
    /// <param name="id">Ending palette resource to read from.</param>
    /// <param name="color">Color index within the selected palette.</param>
    /// <returns>The 15-bit SNES color word at that index.</returns>
    private ushort StaticPaletteColor(EndingPaletteId id, int color)
        => (paletteArtwork ?? throw new InvalidOperationException(
            "Ending palettes require installed artwork."))[id].Color(color);
}
