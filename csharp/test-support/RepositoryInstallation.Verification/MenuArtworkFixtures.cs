using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1392): rebinds a fixture's menu artwork as the game rebinds an installation's, the presentation
/// together with the sprite sheets that presentation composes, so a restored or re-pointed menu draws what an
/// installation of that presentation holds.
/// </summary>
internal static class MenuArtworkFixtures
{
    extension(IMenuFrontendScreen self)
    {
        /// <summary>Binds <paramref name="maps"/> and the sprite sheets it composes.</summary>
        internal void BindFixtureArtwork(AreaMapPresentationCatalog maps)
        {
            self.BindMapPresentation(maps);
            self.BindSpriteSheets(MenuSheetFixtures.For(maps));
        }
    }

    extension(FileSelectRoomMapGraphics self)
    {
        /// <summary>Binds <paramref name="maps"/> and the map sprite sheet its sprites compose.</summary>
        internal void BindFixtureArtwork(AreaMapPresentationCatalog maps)
        {
            self.BindMapPresentation(maps);
            self.BindMapSheet(MenuSheetFixtures.MapSprites(maps.Sprites));
        }
    }

    extension(FileSelectAreaMapGraphics self)
    {
        /// <summary>Binds <paramref name="maps"/> and the map sprite sheet its sprites compose.</summary>
        internal void BindFixtureArtwork(AreaMapPresentationCatalog maps)
        {
            self.BindMapPresentation(maps);
            self.BindMapSheet(MenuSheetFixtures.MapSprites(maps.Sprites));
        }
    }
}
