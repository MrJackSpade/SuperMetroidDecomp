using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>
/// A menu screen drawn from its part of the installed screen content and the installed sprite sheets (#1392, #2203): it rebinds the content
/// when it changes and the sheets with every binding.
/// </summary>
internal interface IMenuFrontendScreen : IFrontendScreen
{
    /// <summary>Rebinds the installed screen content whose part the screen's pictures, colours and sprites come from.</summary>
    void BindScreenContent(InstalledScreenContent? catalog);

    /// <summary>Binds the installed sprite sheets the screen's sprites draw from.</summary>
    void BindSpriteSheets(InstalledSpriteSheets? sheets);

    /// <inheritdoc/>
    void IFrontendScreen.BindArtwork(GameArtwork previous, GameArtwork artwork, SuperMetroidGameState state)
    {
        Ensure.NotNull(previous);
        Ensure.NotNull(artwork);
        if (!ReferenceEquals(previous.ScreenContent, artwork.ScreenContent))
            BindScreenContent(artwork.ScreenContent);
        BindSpriteSheets(artwork.SpriteSheets);
    }
}
