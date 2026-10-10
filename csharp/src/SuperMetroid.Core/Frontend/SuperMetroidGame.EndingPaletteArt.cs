using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    /// <summary>Runtime-bound ending palette catalog forwarded to the active or next credits sequence.</summary>
    [NonSerialized] private EndingPaletteCatalog? endingPaletteArt;

    /// <summary>Attaches ending colors without changing cinematic phase or palette-FX rules.</summary>
    public void BindEndingPaletteArt(EndingPaletteCatalog? palettes)
    {
        endingPaletteArt = palettes;
        endingCredits?.BindPaletteArtwork(palettes);
    }
}
