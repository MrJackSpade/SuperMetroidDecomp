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
        /// <summary>Puts the menu at <paramref name="phase"/>, for a check that starts at that step without running the updates that reach it.</summary>
        internal void SkipToPhase(GameOptionsPhase phase) =>
            PrivateFrontendState.Set(self, nameof(GameOptionsMenuState.Phase), phase);

        /// <summary>
        /// The page the menu shows, laid out as the cartridge's little-endian tilemap words from the import's sources of the
        /// pictures it places (#1467): the page with its language rows, controller labels or toggle choices as shown, read
        /// from <paramref name="maps"/>, the import's read of the presentation the menu was built with (#1616).
        /// </summary>
        internal byte[] ShownPageBytes(SuperMetroid.AssetExtraction.Frontend.Map.ImportAreaMapPresentation maps)
        {
            GameOptionsSourcePresentation presentation = Ensure.NotNull(maps).GameOptions;
            ShownOptionsPage shown = self.ShownPage;
            BackgroundTile[] tiles = presentation.CreatePage(shown.Screen);
            switch (shown.Page)
            {
                case GameOptionsPage.Primary:
                    presentation.ApplyLanguage(tiles, shown.Language);
                    break;
                case GameOptionsPage.Controller:
                    for (int row = 0; row < GameOptionsPageRows.ControllerActions.Count; row++)
                        presentation.ApplyControllerLabel(tiles, GameOptionsPageRows.ControllerActions[row], shown.Bindings[GameOptionsPageRows.ControllerActions[row]]);
                    break;
                case GameOptionsPage.Special:
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
