using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The source artwork an installed map presentation was read with, which only the import composes pictures from (#1466,
/// #1467): the game reads none of it.
/// </summary>
/// <param name="Characters">The characters the map screens, the pause interface and the HUD are pictured from.</param>
/// <param name="Messages">The editable gameplay messages, whose words compose each message box image.</param>
/// <param name="RoomFxPages">The editable room effects' BG3 pages, pictured into each tileset's BG3 sheet.</param>
/// <param name="StaffCredits">The editable staff credits, pictured into the ending's credits rows.</param>
/// <param name="EndingText">The editable ending text, whose pages and glyphs are pictured and whose typewriter characters are compiled.</param>
/// <param name="Screens">The map screens' pages, pictured for the world map and the room maps.</param>
/// <param name="Pause">The pause screen's backdrops, wireframes and equipment-page artwork, pictured into the pause sheet.</param>
/// <param name="Sprites">The map and menu sprite compositions, pictured into the map sprite sheet and the menu sheets.</param>
/// <param name="SamusColours">Samus's selected colour tables, which her sheets are pre-coloured from (#1512).</param>
/// <param name="TitlePalette">The title's selected colours, which the title's sheets are pre-coloured from (#1512).</param>
/// <param name="HighlightColours">The map highlight cycle with its colours, which the map sprite sheet's highlight shades are pre-coloured from (#1512).</param>
/// <param name="Palettes">The colours each map and menu screen loads, which the screens' sheets and their drawn shades are pre-coloured from (#1512).</param>
/// <param name="CeresRidleyColors">Ceres Ridley's, the shared Norfair Ridley health and the Ceres Baby's colours, which their sheets and Ceres Ridley's room are pre-coloured from (#1512).</param>
/// <param name="CeresRidleyFloorColors">Ceres Ridley's Mode 7 floor colours for each getaway zoom step, which the floor's shades are pre-coloured from (#1512).</param>
/// <param name="EscapeTimerTiles">The escape timer's characters, which the escape encounters' sheets are composed from (#1513).</param>
/// <param name="RoomFxAnimatedTiles">The animated tiles' installed characters, which room sheets and the Tourian statues' pictures are composed from (#1527).</param>
/// <param name="MotherBrainHealthPalette">Mother Brain's health-band colours, which her sheets and body layer are pre-coloured from (#1527).</param>
/// <param name="MotherBrainRoomColors">Mother Brain's room colours, which her room's pictures and sheets are pre-coloured from (#1527).</param>
internal sealed record InstalledPresentationSources(
    MapCharacterArtwork Characters,
    GameplayMessageArtwork Messages,
    RoomFxLayer3Pages RoomFxPages,
    CreditsPresentation StaffCredits,
    EndingTextPresentation EndingText,
    MapScreenPresentation Screens,
    PauseScreenArtwork Pause,
    MapSpriteCompositions Sprites,
    SamusMapColourTables SamusColours,
    TitlePalettePresentation TitlePalette,
    MapPaletteCycle HighlightColours,
    MapStaticPalettes Palettes,
    CeresRidleyColorCatalog CeresRidleyColors,
    CeresRidleyFloorColorCatalog CeresRidleyFloorColors,
    EscapeTimerCharacters EscapeTimerTiles,
    RoomFxAnimatedTileAtlas RoomFxAnimatedTiles,
    MotherBrainHealthPalettePresentation MotherBrainHealthPalette,
    MotherBrainRoomColorPresentation MotherBrainRoomColors);
