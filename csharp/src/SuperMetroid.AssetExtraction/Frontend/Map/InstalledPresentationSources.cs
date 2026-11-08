using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The source artwork an installed map presentation was read with, which only the import composes pictures from (#1466,
/// #1467): the game reads none of it. The colours only the import pre-colours sheets from are not among them: the game's
/// read leaves them unread (#1544, <see cref="ImportPresentationColours"/>).
/// </summary>
/// <param name="Characters">The characters the map screens, the pause interface and the HUD are pictured from.</param>
/// <param name="Messages">The editable gameplay messages, whose words compose each message box image.</param>
/// <param name="RoomFxPages">The editable room effects' BG3 pages, pictured into each tileset's BG3 sheet.</param>
/// <param name="StaffCredits">The editable staff credits, pictured into the ending's credits rows.</param>
/// <param name="EndingText">The editable ending text, whose pages and glyphs are pictured and whose typewriter characters are compiled.</param>
/// <param name="Screens">The map screens' pages, pictured for the world map and the room maps.</param>
/// <param name="Pause">The pause screen's backdrops, wireframes and equipment-page artwork, pictured into the pause sheet.</param>
/// <param name="Sprites">The map and menu sprite compositions, pictured into the map sprite sheet and the menu sheets.</param>
/// <param name="HighlightColours">
/// The map highlight cycle with its colours, which the map sprite sheet's highlight shades are pre-coloured from (#1512).
/// The game reads the same document for each frame's hold: a frame's hold and its look are one record of the effect.
/// </param>
/// <param name="EscapeTimerTiles">The escape timer's characters, which the escape encounters' sheets are composed from (#1513).</param>
/// <param name="RoomFxAnimatedTiles">The animated tiles' installed characters, which room sheets and the Tourian statues' pictures are composed from (#1527).</param>
internal sealed record InstalledPresentationSources(
    MapCharacterArtwork Characters,
    GameplayMessageArtwork Messages,
    RoomFxLayer3Pages RoomFxPages,
    CreditsPresentation StaffCredits,
    EndingTextPresentation EndingText,
    MapScreenPresentation Screens,
    PauseScreenArtwork Pause,
    MapSpriteCompositions Sprites,
    MapPaletteCycle HighlightColours,
    EscapeTimerCharacters EscapeTimerTiles,
    RoomFxAnimatedTileAtlas RoomFxAnimatedTiles);
