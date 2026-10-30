using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Verification access to <see cref="GameOptionsMenuState"/> members production does not use.</summary>
internal static class GameOptionsMenuStateAccess
{
    /// <summary>Reads the page the menu shows as the cartridge's tilemap words.</summary>
    extension(GameOptionsMenuState self)
    {
        /// <summary>
        /// The page the menu shows, laid out as the cartridge's little-endian tilemap words from the import's sources of the
        /// pictures it places (#1467): the page with its language rows, controller labels or toggle choices as shown, read
        /// from <paramref name="maps"/>, the presentation the menu was built with.
        /// </summary>
        internal byte[] ShownPageBytes(AreaMapPresentation maps)
        {
            GameOptionsSourcePresentation presentation = GameOptionsSourcePresentation.Of(Ensure.NotNull(maps).GameOptions);
            ShownOptionsPage shown = self.ShownPage;
            BackgroundTile[] tiles = presentation.CreatePage(shown.Page);
            switch (shown.Page)
            {
                case GameOptionsTilemap.Primary:
                    presentation.ApplyLanguage(tiles, shown.JapaneseText);
                    break;
                case GameOptionsTilemap.ControllerEnglish:
                case GameOptionsTilemap.ControllerJapanese:
                    for (int action = 0; action < GameOptionsDefinitions.Rows.ControllerActionCount; action++)
                        presentation.ApplyControllerLabel(tiles, action, Input.ControllerBindings.AssignableButtonIndex(shown.Bindings[action]) ?? 0);
                    break;
                case GameOptionsTilemap.SpecialEnglish:
                case GameOptionsTilemap.SpecialJapanese:
                    presentation.ApplySpecialToggle(tiles, GameOptionsToggle.IconCancel, shown.IconCancelEnabled);
                    presentation.ApplySpecialToggle(tiles, GameOptionsToggle.Moonwalk, shown.MoonwalkEnabled);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined options page {shown.Page}.");
            }
            return BackgroundTileWordBytes.Of(tiles);
        }
    }
}
