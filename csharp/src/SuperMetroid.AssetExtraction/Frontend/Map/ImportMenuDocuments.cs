using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The menus' extracted documents, import data (#1638): each mixes the layout the game binds, which the installation holds
/// alone, with compositions and pages whose entries are the original's packed tile and sprite placements, which the import
/// composes the menus' sheets and pictures from.
/// </summary>
/// <param name="PauseSelectors">The equipment selectors' frame compositions, anchors and timing.</param>
/// <param name="PauseReserveTanks">The reserve strip's frame compositions and anchors.</param>
/// <param name="GameOver">The game-over page's tiles, compositions, palette selectors and anchors.</param>
/// <param name="GameOptions">The options screen's pages, labels, compositions and anchors.</param>
/// <param name="EndingText">The ending text's panels, subtitle and labels.</param>
internal sealed record ImportMenuDocuments(PauseSelectorSourcePresentation PauseSelectors, PauseReserveTankSourcePresentation PauseReserveTanks,
    GameOverSourcePresentation GameOver, GameOptionsSourcePresentation GameOptions, EndingTextPresentation EndingText);
