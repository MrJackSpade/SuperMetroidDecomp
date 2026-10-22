using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// A menu screen drawn from the menu presentation and the installed sprite sheets (#1392): it rebinds the presentation
/// when it changes and the sheets with every binding.
/// </summary>
internal interface IMenuFrontendScreen : IFrontendScreen
{
    /// <summary>Rebinds the menu presentation the screen's tilemaps, palettes and sprites come from.</summary>
    void BindMapPresentation(AreaMapPresentationCatalog? catalog);

    /// <summary>Binds the installed sprite sheets the screen's sprites draw from.</summary>
    void BindSpriteSheets(InstalledSpriteSheets? sheets);

    /// <inheritdoc/>
    void IFrontendScreen.BindArtwork(GameArtwork previous, GameArtwork artwork, SuperMetroidGameState state)
    {
        Ensure.NotNull(previous);
        Ensure.NotNull(artwork);
        if (!ReferenceEquals(previous.MapPresentation, artwork.MapPresentation))
            BindMapPresentation(artwork.MapPresentation);
        BindSpriteSheets(artwork.SpriteSheets);
    }
}
