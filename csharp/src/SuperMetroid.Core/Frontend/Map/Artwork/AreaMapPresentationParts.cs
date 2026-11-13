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

    /// <summary>Uppercase SHA-256 identity of length-framed map rules, station masks, and installed presentation resources in fixed order; an edit in place changes the identity.</summary>
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

    /// <summary>The characters the ending's typewriter types, compiled by the import from the installed ending text (#1467).</summary>
    public required EndingTypewriterText EndingText { get; init; }


    /// <summary>The colour the title shows behind its floor and objects (#1512).</summary>
    public required RgbColor TitleBackdrop { get; init; }

    /// <summary>Title-screen gradient presentation, separate from its static palette and character artwork.</summary>
    public required TitleGradientPresentation TitleGradient { get; init; }

    /// <summary>The backdrop each record of the space behind the exploding planet shows (#1512, #1583).</summary>
    public required EndingSpaceBackdrops EndingSpaceBackdrops { get; init; }

    /// <summary>Mother Brain's rainbow beam backdrop colours.</summary>
    public required MotherBrainRainbowPalettePresentation MotherBrainRainbowPalette { get; init; }

    /// <summary>The Ceres haze's installed tints.</summary>
    public required CeresHazeTints CeresHaze { get; init; }

    /// <summary>The glow colours Power Bomb, Crystal Flash, and Ceres explosions add to the scene, separate from Samus palette colors.</summary>
    public required InstalledPowerBombGlowColors PowerBombGlowColors { get; init; }



    /// <summary>The Speed Booster delays the getaway's wing frames leave behind for the unauthored cadence.</summary>
    public required CeresRidleyGetawayBoostDelays CeresRidleyGetawayBoostDelays { get; init; }
}
