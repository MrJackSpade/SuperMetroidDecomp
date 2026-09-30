using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class EndingCreditsState
{
    [NonSerialized] private EndingPaletteCatalog? paletteArtwork;
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

    private void LoadStaticPalette(EndingPaletteId id, int sourceColor, int count,
        int destinationColor)
    {
        (paletteArtwork ?? throw new InvalidOperationException(
            "Ending palettes require installed artwork."))[id].LoadTo(cgram, sourceColor, count, destinationColor);
    }

    private ushort StaticPaletteColor(EndingPaletteId id, int color)
        => (paletteArtwork ?? throw new InvalidOperationException(
            "Ending palettes require installed artwork."))[id].Color(color);
}
