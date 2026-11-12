using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed documents an installed map presentation was read with that the import composes pictures from (#1466,
/// #1467) and the game names its picture sheets' frames from. The colours only the import pre-colours sheets from are not
/// among them: the game's read leaves them unread (#1544, <see cref="ImportPresentationColours"/>), and the characters are
/// import data the installation never holds (#1636, <see cref="ImportPresentationCharacters"/>).
/// </summary>
/// <param name="Messages">The editable gameplay messages, whose words compose each message box image.</param>
/// <param name="RoomFxPages">The editable room effects' BG3 pages, pictured into each tileset's BG3 sheet.</param>
/// <param name="StaffCredits">The editable staff credits, pictured into the ending's credits rows.</param>
/// <param name="EndingText">The editable ending text, whose pages and glyphs are pictured and whose typewriter characters are compiled.</param>
/// <param name="Screens">The map screens' pages, pictured for the world map and the room maps.</param>
/// <param name="Pause">The pause screen's backdrops, wireframes and equipment-page artwork, pictured into the pause sheet.</param>
/// <param name="HighlightColours">
/// The map highlight cycle with its colours, which the map sprite sheet's highlight shades are pre-coloured from (#1512).
/// The game reads the same document for each frame's hold: a frame's hold and its look are one record of the effect.
/// </param>
internal sealed record InstalledPresentationSources(
    GameplayMessageArtwork Messages,
    RoomFxLayer3Pages RoomFxPages,
    CreditsPresentation StaffCredits,
    EndingTextPresentation EndingText,
    MapScreenPresentation Screens,
    PauseScreenArtwork Pause,
    MapPaletteCycle HighlightColours);
