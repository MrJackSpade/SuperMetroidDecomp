using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Map.Artwork;
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>The installed map, pause, HUD and escape-timer presentation parts an area-map presentation catalog is built from (#633).</summary>
internal sealed record AreaMapPresentationParts
{
    /// <summary>Areas.</summary>
    public required IAreaMapView[] Areas { get; init; }

    /// <summary>Uppercase SHA-256 identity of length-framed stock map rules, station masks, and selected presentation resources in fixed order; override edits affect identity without changing stock provenance.</summary>
    public required string ContentIdentity { get; init; }

    /// <summary>Selected map-screen character atlas; pause-menu artwork is supplied separately by <see cref="PauseTiles"/>.</summary>
    public required MapTileAtlas Tiles { get; init; }

    /// <summary>Standard gameplay HUD character transfers, including the four BG3 quarters restored after Kraid.</summary>
    public required HudTileAtlas HudTiles { get; init; }

    /// <summary>Selected highlight color frames used by map presentation; animation progression remains engine-owned.</summary>
    public required MapPaletteCycle HighlightCycle { get; init; }

    /// <summary>Complete static palettes for pause, file-select, and each world-map area, separate from the highlight cycle.</summary>
    public required MapStaticPalettes Palettes { get; init; }

    /// <summary>World-map area-name drawing anchors, independent of area selection and map exploration.</summary>
    public required WorldMapLabelPositions Labels { get; init; }

    /// <summary>Map-station label drawing positions; moving these does not change compiled discovery cells or station reveal masks.</summary>
    public required MapStationPositions Stations { get; init; }

    /// <summary>Boss, elevator, and gunship drawing anchors, independent of progression and destination rules.</summary>
    public required MapLandmarkPositions Landmarks { get; init; }

    /// <summary>File-select save-marker visual coordinate overrides, independent of native load targets and scrolling.</summary>
    public required MapSaveMarkerPositions SaveMarkers { get; init; }

    /// <summary>Map-arrow artwork and presentation frames for scrolling indicators.</summary>
    public required MapArrowPresentation Arrows { get; init; }

    /// <summary>Named compiled map-screen tilemap layers, separate from the character atlas and per-area map cells.</summary>
    public required MapScreenPresentation Screens { get; init; }

    /// <summary>Selected foreground and background PNG artwork for the world-map screen.</summary>
    public required WorldMapArtwork WorldArtwork { get; init; }

    /// <summary>Selected map sprite shapes and indexed PNG artwork used to draw map icons.</summary>
    public required MapSpriteCompositions Sprites { get; init; }

    /// <summary>Selected pause-menu character atlas, separate from map-screen characters in <see cref="Tiles"/>.</summary>
    public required MapTileAtlas PauseTiles { get; init; }

    /// <summary>Pause-screen backdrop tilemap presentation, independent of equipment and map state.</summary>
    public required PauseBackdropPresentation PauseBackdrops { get; init; }

    /// <summary>Samus suit wireframe artwork for the pause equipment screen.</summary>
    public required PauseWireframePresentation PauseWireframes { get; init; }

    /// <summary>Pause selection-indicator artwork and presentation parameters; inventory and selection logic remain engine-owned.</summary>
    public required PauseSelectorPresentation PauseSelectors { get; init; }

    /// <summary>Reserve-strip artwork and drawing origins, independent of reserve energy and fill selection.</summary>
    public required PauseReserveTankPresentation PauseReserveTanks { get; init; }

    /// <summary>Reserve labels, digits, and arrow appearances, separate from the tank strip and compiled reserve-mode behavior.</summary>
    public required PauseReserveUiPresentation PauseReserveUi { get; init; }

    /// <summary>Equipment-page base tilemap artwork, before live inventory and reserve patches are applied.</summary>
    public required PauseEquipmentBasePresentation PauseEquipmentBase { get; init; }

    /// <summary>Equipment-page label artwork and placement, separate from the base tilemap and live inventory decisions.</summary>
    public required PauseEquipmentLabelPresentation PauseEquipmentLabels { get; init; }

    /// <summary>Escape countdown visual layout and digit presentation, separate from the character bytes in <see cref="EscapeTimerTiles"/>.</summary>
    public required EscapeTimerPresentation EscapeTimer { get; init; }

    /// <summary>Selected character bytes for the two escape-timer VRAM transfers.</summary>
    public required EscapeTimerTileAtlas EscapeTimerTiles { get; init; }

    /// <summary>Gameplay HUD tilemap and display presentation, separate from the standard HUD character atlas.</summary>
    public required GameplayHudPresentation GameplayHud { get; init; }

    /// <summary>Selected game-over screen presentation resources.</summary>
    public required GameOverPresentation GameOver { get; init; }

    /// <summary>Selected options-menu artwork and display presentation; option values remain runtime state.</summary>
    public required GameOptionsPresentation GameOptions { get; init; }

    /// <summary>Selected file-select screen presentation, separate from per-area maps and save-marker coordinates.</summary>
    public required FileSelectPresentation FileSelect { get; init; }

    /// <summary>Gameplay-message title text and tile presentation, separate from message panels and short notices.</summary>
    public required GameplayMessageTitlePresentation GameplayMessageTitles { get; init; }

    /// <summary>Gameplay-message panel artwork and layout, separate from title content and short notice presentation.</summary>
    public required GameplayMessagePanelPresentation GameplayMessagePanels { get; init; }

    /// <summary>Short gameplay-notice presentation, separate from full message titles and panels.</summary>
    public required GameplayMessageNoticePresentation GameplayMessageNotices { get; init; }

    /// <summary>Selected escape-sequence typewriter text and visual presentation.</summary>
    public required EscapeTypewriterPresentation EscapeTypewriter { get; init; }

    /// <summary>Opening narration text and presentation, separate from its selected font characters.</summary>
    public required IntroNarrationPresentation IntroNarration { get; init; }

    /// <summary>Selected opening-narration character artwork used with <see cref="IntroNarration"/>.</summary>
    public required IntroFontAtlas IntroFont { get; init; }

    /// <summary>Ending text and display presentation, separate from staff-credit content.</summary>
    public required EndingTextPresentation EndingText { get; init; }

    /// <summary>Selected character artwork for ending text and its font transfers.</summary>
    public required EndingFontAtlas EndingFont { get; init; }

    /// <summary>Selected staff-credit text and presentation, separate from the other ending text.</summary>
    public required CreditsPresentation StaffCredits { get; init; }

    /// <summary>Title Mode 7 characters and tilemap, OBJ characters, and Baby character artwork selected from their extracted files.</summary>
    public required TitleGraphicsPresentation TitleGraphics { get; init; }

    /// <summary>Title-screen palette colors, separate from the title gradient and graphics transfers.</summary>
    public required TitlePalettePresentation TitlePalette { get; init; }

    /// <summary>Title-screen gradient presentation, separate from its static palette and character artwork.</summary>
    public required TitleGradientPresentation TitleGradient { get; init; }

    /// <summary>Room and cinematic palette-animation colors; instruction timing, CGRAM destinations, and side effects remain compiled.</summary>
    public required RoomPaletteFxPresentation RoomPaletteFx { get; init; }

    /// <summary>Mother Brain health-selected palette artwork, separate from rainbow and room-color families.</summary>
    public required MotherBrainHealthPalettePresentation MotherBrainHealthPalette { get; init; }

    /// <summary>Mother Brain rainbow, drain, revival, normal-restoration, and fake-death colors; older overrides inherit newly introduced rows from stock.</summary>
    public required MotherBrainRainbowPalettePresentation MotherBrainRainbowPalette { get; init; }

    /// <summary>Mother Brain arena color presentation, separate from boss health and rainbow palettes.</summary>
    public required MotherBrainRoomColorPresentation MotherBrainRoomColors { get; init; }

    /// <summary>Selected animated room-FX character artwork, separate from layer-3 tilemaps and palette blending.</summary>
    public required RoomFxAnimatedTileAtlas RoomFxAnimatedTiles { get; init; }

    /// <summary>Selected layer-3 room-FX tilemaps, separate from animated characters and palette-blend colors.</summary>
    public required RoomFxLayer3Pages RoomFxLayer3Tilemaps { get; init; }

    /// <summary>Selected room-FX palette-blend presentation, separate from layer-3 tilemap geometry and animated character bytes.</summary>
    public required RoomFxBlendInks RoomFxPaletteBlends { get; init; }

    /// <summary>Fixed RGB5 colors for Power Bomb, Crystal Flash, and Ceres explosion effects, separate from Samus palette colors.</summary>
    public required InstalledPowerBombFixedColors PowerBombFixedColors { get; init; }

    /// <summary>Visor colors shared by X-ray and room palette cycling, separate from complete suit palettes.</summary>
    public required InstalledSamusVisorColors SamusVisorColors { get; init; }

    /// <summary>Samus hurt-flash and cinematic-restoration color artwork.</summary>
    public required InstalledSamusHurtColors SamusHurtColors { get; init; }

    /// <summary>Base Power, Varia, and Gravity Suit colors, separate from transient full-body and charge cycles.</summary>
    public required InstalledSamusSuitColors SamusSuitColors { get; init; }

    /// <summary>Samus full-body palette-cycle inputs, separate from static suit, visor, and charged-beam colors.</summary>
    public required SamusFullBodyCycleColours SamusFullBodyCycleColors { get; init; }

    /// <summary>Crystal Flash body and bubble colors; native timing, frame selection, and completion remain engine-owned.</summary>
    public required InstalledCrystalFlashColors CrystalFlashColors { get; init; }

    /// <summary>Charged-beam and pseudo-Screw-Attack palette inputs, separate from base suit colors.</summary>
    public required SamusChargeColours SamusChargeColors { get; init; }

    /// <summary>Ceres Ridley colors, shared Norfair Ridley health colors, and private Baby drawing colors; Mode 7 zoom shades are supplied separately.</summary>
    public required CeresRidleyColorCatalog CeresRidleyColors { get; init; }

    /// <summary>Ceres Ridley Mode 7 zoom shades, separate from ordinary Ridley colors and compiled movement and rotation.</summary>
    public required CeresRidleyFloorColorCatalog CeresRidleyFloorColors { get; init; }

    /// <summary>Where the getaway's floor runs take their tile numbers from, which the floor updates it queues record.</summary>
    public required CeresRidleyFloorUpdateSources CeresRidleyFloorUpdates { get; init; }

    /// <summary>Samus Hyper Beam palette-cycle colors, separate from charged-beam presentation and other suit cycles.</summary>
    public required SamusHyperBeamColours SamusHyperBeamColors { get; init; }
}
