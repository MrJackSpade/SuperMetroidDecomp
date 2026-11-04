using SuperMetroid.Core.Assets;
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
using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>The installed map, pause, HUD and escape-timer presentation parts an area-map presentation catalog is built from (#633).</summary>
[ArchitectureReview("context-bag", "The import's one hand-off of the installed map, pause, HUD and frontend presentation catalogs to AreaMapPresentation; every member is immutable installed presentation data, not a service.")]
internal sealed record AreaMapPresentationParts
{
    /// <summary>Areas.</summary>
    public required IAreaMapView[] Areas { get; init; }

    /// <summary>The cells the map screens show in place of cells nothing reveals.</summary>
    public required AreaMapHiddenCells HiddenCells { get; init; }

    /// <summary>Uppercase SHA-256 identity of length-framed stock map rules, station masks, and selected presentation resources in fixed order; override edits affect identity without changing stock provenance.</summary>
    public required string ContentIdentity { get; init; }

    /// <summary>Selected highlight color frames used by map presentation; animation progression remains engine-owned.</summary>
    public required MapHighlightCycle HighlightCycle { get; init; }

    /// <summary>The solid colour the pause, file-select and world-map screens show where nothing is drawn (#1512).</summary>
    public required MenuBackdrops Backdrops { get; init; }

    /// <summary>World-map area-name drawing anchors, independent of area selection and map exploration.</summary>
    public required WorldMapLabelPositions Labels { get; init; }

    /// <summary>Map-station label drawing positions; moving these does not change compiled discovery cells or station reveal masks.</summary>
    public required MapStationPositions Stations { get; init; }

    /// <summary>Boss, elevator, and gunship drawing anchors, independent of progression and destination rules.</summary>
    public required MapLandmarkPositions Landmarks { get; init; }

    /// <summary>File-select save-marker visual coordinate overrides, independent of load targets and scrolling.</summary>
    public required MapSaveMarkerPositions SaveMarkers { get; init; }

    /// <summary>Map-arrow artwork and presentation frames for scrolling indicators.</summary>
    public required MapArrowPresentation Arrows { get; init; }


    /// <summary>Selected foreground and background PNG artwork for the world-map screen.</summary>
    public required WorldMapArtwork WorldArtwork { get; init; }

    /// <summary>Pause selection-indicator artwork and presentation parameters; inventory and selection logic remain engine-owned.</summary>
    public required PauseSelectorPresentation PauseSelectors { get; init; }

    /// <summary>Reserve-strip artwork and drawing origins, independent of reserve energy and fill selection.</summary>
    public required PauseReserveTankPresentation PauseReserveTanks { get; init; }

    /// <summary>Escape countdown visual layout and digit presentation.</summary>
    public required EscapeTimerPresentation EscapeTimer { get; init; }

    /// <summary>Gameplay HUD tilemap and display presentation, separate from the standard HUD character atlas.</summary>
    public required GameplayHudPresentation GameplayHud { get; init; }

    /// <summary>Selected game-over screen presentation resources.</summary>
    public required GameOverPresentation GameOver { get; init; }

    /// <summary>Selected options-menu artwork and display presentation; option values remain runtime state.</summary>
    public required GameOptionsPresentation GameOptions { get; init; }

    /// <summary>Selected file-select screen presentation, separate from per-area maps and save-marker coordinates.</summary>
    public required FileSelectPresentation FileSelect { get; init; }

    /// <summary>Selected escape-sequence typewriter text and visual presentation.</summary>
    public required EscapeTypewriterPresentation EscapeTypewriter { get; init; }

    /// <summary>Opening narration text and presentation, separate from its selected font characters.</summary>
    public required IntroNarrationPresentation IntroNarration { get; init; }

    /// <summary>Selected opening-narration character artwork used with <see cref="IntroNarration"/>.</summary>
    public required IntroFontAtlas IntroFont { get; init; }

    /// <summary>The characters the ending's typewriter types, compiled by the import from the installed ending text (#1467).</summary>
    public required EndingTypewriterText EndingText { get; init; }

    /// <summary>Selected character artwork for ending text and its font transfers.</summary>
    public required EndingFontAtlas EndingFont { get; init; }


    /// <summary>Title Mode 7 characters and tilemap, OBJ characters, and Baby character artwork selected from their extracted files.</summary>
    public required TitleGraphicsPresentation TitleGraphics { get; init; }

    /// <summary>The colour the title shows behind its floor and objects (#1512).</summary>
    public required RgbColor TitleBackdrop { get; init; }

    /// <summary>Title-screen gradient presentation, separate from its static palette and character artwork.</summary>
    public required TitleGradientPresentation TitleGradient { get; init; }

    /// <summary>Room and cinematic palette-animation colors; step timing, colour destinations, and side effects remain compiled.</summary>
    public required RoomPaletteFxPresentation RoomPaletteFx { get; init; }

    /// <summary>Mother Brain health-selected palette artwork, separate from rainbow and room-color families.</summary>
    public required MotherBrainHealthPalettePresentation MotherBrainHealthPalette { get; init; }

    /// <summary>Mother Brain rainbow, drain, revival, normal-restoration, and fake-death colors; older overrides inherit newly introduced colour tables from stock.</summary>
    public required MotherBrainRainbowPalettePresentation MotherBrainRainbowPalette { get; init; }

    /// <summary>Mother Brain arena color presentation, separate from boss health and rainbow palettes.</summary>
    public required MotherBrainRoomColorPresentation MotherBrainRoomColors { get; init; }

    /// <summary>Selected animated room-FX character artwork, separate from layer-3 tilemaps and palette blending.</summary>
    public required RoomFxAnimatedTileAtlas RoomFxAnimatedTiles { get; init; }

    /// <summary>Selected room-FX palette-blend presentation, separate from layer-3 tilemap geometry and animated character bytes.</summary>
    public required RoomFxBlendInks RoomFxPaletteBlends { get; init; }

    /// <summary>Fixed RGB5 colors for Power Bomb, Crystal Flash, and Ceres explosion effects, separate from Samus palette colors.</summary>
    public required InstalledPowerBombFixedColors PowerBombFixedColors { get; init; }



    /// <summary>The Speed Booster delays the getaway's wing frames leave behind for the unauthored cadence.</summary>
    public required CeresRidleyGetawayBoostDelays CeresRidleyGetawayBoostDelays { get; init; }
}
