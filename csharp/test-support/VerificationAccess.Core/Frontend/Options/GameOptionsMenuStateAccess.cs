using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;

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
        /// from <paramref name="maps"/>, the import's read of the presentation the menu was built with (#1616).
        /// </summary>
        internal byte[] ShownPageBytes(SuperMetroid.AssetExtraction.Frontend.Map.ImportAreaMapPresentation maps)
        {
            GameOptionsSourcePresentation presentation = Ensure.NotNull(maps).GameOptions;
            ShownOptionsPage shown = self.ShownPage;
            BackgroundTile[] tiles = presentation.CreatePage(shown.Page);
            switch (shown.Page)
            {
                case GameOptionsScreen.Primary:
                    presentation.ApplyLanguage(tiles, shown.JapaneseText);
                    break;
                case GameOptionsScreen.ControllerEnglish:
                case GameOptionsScreen.ControllerJapanese:
                    for (int row = 0; row < GameOptionsLayout.ControllerActionRows.Length; row++)
                        presentation.ApplyControllerLabel(tiles, row,
                            Input.ControllerBindings.AssignableButtonIndex(shown.Bindings[GameOptionsLayout.ControllerActionRows[row]]) ?? 0);
                    break;
                case GameOptionsScreen.SpecialEnglish:
                case GameOptionsScreen.SpecialJapanese:
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
