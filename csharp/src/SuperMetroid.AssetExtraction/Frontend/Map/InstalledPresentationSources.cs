using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed documents an installed map presentation was read with that the import composes pictures from (#1466,
/// #1467) and the game names its picture sheets' frames from: the game reads none of them, only the catalogs the selector,
/// reserve-strip, HUD, options and file-select sources build for it (#1616). The colours only the import pre-colours sheets
/// from are not among them: the game's read leaves them unread (#1544, <see cref="ImportPresentationColours"/>), and the
/// characters are import data the installation never holds (#1636, <see cref="ImportPresentationCharacters"/>).
/// </summary>
/// <param name="StaffCredits">The editable staff credits, pictured into the ending's credits rows.</param>
/// <param name="EndingText">The editable ending text, whose pages and glyphs are pictured and whose typewriter characters are compiled.</param>
/// <param name="Screens">The map screens' pages, pictured for the world map and the room maps.</param>
/// <param name="HighlightColours">
/// The map highlight cycle with its colours, which the map sprite sheet's highlight shades are pre-coloured from (#1512).
/// The game reads the same document for each frame's hold: a frame's hold and its look are one record of the effect.
/// </param>
/// <param name="PauseSelectors">The equipment selectors' frame compositions with the anchors and timing the game binds (#1616).</param>
/// <param name="PauseReserveTanks">The reserve strip's frame compositions with the anchors the game binds (#1616).</param>
/// <param name="GameplayHud">The gameplay HUD's tiles with the layout the game binds (#1616).</param>
/// <param name="GameOptions">The options screen's pages, labels and compositions with the anchors the game binds (#1616).</param>
[ArchitectureReview("context-bag", "The installed documents an installed map presentation was read with; every member is immutable artwork data the import composes pictures from, not a service.")]
internal sealed record InstalledPresentationSources(
    CreditsPresentation StaffCredits,
    EndingTextPresentation EndingText,
    MapScreenPresentation Screens,
    MapPaletteCycle HighlightColours,
    PauseSelectorSourcePresentation PauseSelectors,
    PauseReserveTankSourcePresentation PauseReserveTanks,
    GameplayHudArtwork GameplayHud,
    GameOptionsSourcePresentation GameOptions)
{
    /// <summary>The game-over page's tiles, compositions and palette selectors, which the menus' pictures and the game-over sheet are composed from (#1616).</summary>
    public required GameOverSourcePresentation GameOver { get; init; }
}
