using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed colours only the import reads (#1544): it pre-colours sheets and pictures from them, and the game's read
/// of the map presentation leaves every one of these documents unread.
/// </summary>
/// <param name="SamusColours">Samus's selected colour tables, which her sheets are pre-coloured from (#1512).</param>
/// <param name="TitlePalette">The title's selected colours, which the title's sheets are pre-coloured from (#1512).</param>
/// <param name="Palettes">The colours each map and menu screen loads, which the screens' sheets and their drawn shades are pre-coloured from (#1512).</param>
/// <param name="CeresRidleyColors">Ceres Ridley's, the shared Norfair Ridley health and the Ceres Baby's colours, which their sheets and Ceres Ridley's room are pre-coloured from (#1512).</param>
/// <param name="CeresRidleyFloorColors">Ceres Ridley's Mode 7 floor colours for each getaway zoom step, which the floor's shades are pre-coloured from (#1512).</param>
/// <param name="MotherBrainHealthPalette">Mother Brain's health-band colours, which her sheets and body layer are pre-coloured from (#1527).</param>
/// <param name="MotherBrainRoomColors">Mother Brain's room colours, which her room's pictures and sheets are pre-coloured from (#1527).</param>
/// <param name="RoomFxPaletteBlends">The room-FX blends, which the effect overlay pictures are pre-coloured with (#1527).</param>
/// <param name="RoomPaletteFx">
/// The palette-effect colours, which the sheets each palette-effect record shows are pre-coloured from (#1512, #1583); the
/// backdrops of the space behind the exploding planet the game shows are installed with the ending's backdrops (#1637).
/// </param>
/// <param name="MotherBrainRainbow">
/// Mother Brain's rainbow, drain, revival, fake-death and restoration colours, which her sheets and body layer are
/// pre-coloured from (#1512); her beam backdrop's colours the game works with are installed by purpose (#1637).
/// </param>
/// <param name="HighlightColours">
/// The map highlight cycle's colours, which the map sprite sheet's highlight shades are pre-coloured from (#1512, #1693).
/// </param>
internal sealed record ImportPresentationColours(
    SamusMapColourTables SamusColours,
    TitlePalettePresentation TitlePalette,
    MapStaticPalettes Palettes,
    CeresRidleyColorCatalog CeresRidleyColors,
    CeresRidleyFloorColorCatalog CeresRidleyFloorColors,
    MotherBrainHealthPalettePresentation MotherBrainHealthPalette,
    MotherBrainRoomColorPresentation MotherBrainRoomColors,
    RoomFxBlendInks RoomFxPaletteBlends,
    RoomPaletteFxPresentation RoomPaletteFx,
    InstalledMotherBrainRainbowPalette MotherBrainRainbow,
    MapPaletteCycle HighlightColours);
