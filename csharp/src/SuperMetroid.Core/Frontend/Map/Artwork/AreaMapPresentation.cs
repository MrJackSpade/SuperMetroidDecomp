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
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// Immutable map content snapshot, as installed; edits in place never alter exploration rules. It holds only what the game
/// reads: the import keeps the characters, pages and compositions its screens, the HUD, the world map, the title and the
/// opening and ending text are pictured from to itself and never hands them to the game (#1466, #1583, #1616).
/// </summary>
public sealed class AreaMapPresentation : IGameplayPresentationContent
{
    /// <summary>One validated map view per area.</summary>
    private readonly IReadOnlyDictionary<AreaId, IAreaMapView> areas;
    /// <summary>Captures the selected maps and shared presentation resources as one immutable snapshot.</summary>
    internal AreaMapPresentation(AreaMapPresentationParts parts)
    {
        this.areas = parts.Areas;
        ContentIdentity = parts.ContentIdentity;
        HighlightCycle = parts.HighlightCycle;
        Backdrops = parts.Backdrops;
        Labels = parts.Labels;
        Stations = parts.Stations;
        Landmarks = parts.Landmarks;
        SaveMarkers = parts.SaveMarkers;
        Arrows = parts.Arrows;
        PauseSelectors = parts.PauseSelectors;
        PauseReserveTanks = parts.PauseReserveTanks;
        EscapeTimer = parts.EscapeTimer;
        GameplayHud = parts.GameplayHud;
        GameOver = parts.GameOver;
        GameOptions = parts.GameOptions;
        FileSelect = parts.FileSelect;
        EscapeTypewriter = parts.EscapeTypewriter;
        IntroNarration = parts.IntroNarration;
        EndingText = parts.EndingText;
        TitleBackdrop = parts.TitleBackdrop;
        TitleGradient = parts.TitleGradient;
        MotherBrainRainbowPalette = parts.MotherBrainRainbowPalette;
        CeresHaze = parts.CeresHaze;
        PowerBombGlowColors = parts.PowerBombGlowColors;
        CeresRidleyGetawayBoostDelays = parts.CeresRidleyGetawayBoostDelays;
    }

    /// <summary>Uppercase SHA-256 identity of length-framed map rules, station masks, and installed presentation resources in fixed order; an edit in place changes the identity.</summary>
    public string ContentIdentity { get; }
    /// <summary>Selected highlight color frames used by map presentation; animation progression remains engine-owned.</summary>
    public MapHighlightCycle HighlightCycle { get; }
    /// <summary>The solid colour the pause, file-select and world-map screens show where nothing is drawn (#1512).</summary>
    public MenuBackdrops Backdrops { get; }
    /// <summary>World-map area-name drawing anchors, independent of area selection and map exploration.</summary>
    public WorldMapLabelPositions Labels { get; }
    /// <summary>Map-station label drawing positions; moving these does not change compiled discovery cells or station reveal masks.</summary>
    public MapStationPositions Stations { get; }
    /// <summary>Boss, elevator, and gunship drawing anchors, independent of progression and destination rules.</summary>
    public MapLandmarkPositions Landmarks { get; }
    /// <summary>The installed file-select save-marker drawing anchors, independent of load targets and scrolling.</summary>
    public MapSaveMarkerPositions SaveMarkers { get; }
    /// <summary>Map-arrow artwork and presentation frames for scrolling indicators.</summary>
    public MapArrowPresentation Arrows { get; }
    /// <summary>Pause selection-indicator artwork and presentation parameters; inventory and selection logic remain engine-owned.</summary>
    public PauseSelectorPresentation PauseSelectors { get; }
    /// <summary>Reserve-strip artwork and drawing origins, independent of reserve energy and fill selection.</summary>
    public PauseReserveTankPresentation PauseReserveTanks { get; }
    /// <summary>Escape countdown visual layout and digit presentation.</summary>
    public EscapeTimerPresentation EscapeTimer { get; }
    /// <summary>Gameplay HUD tilemap and display presentation, separate from the standard HUD character atlas.</summary>
    public GameplayHudPresentation GameplayHud { get; }
    /// <summary>Selected game-over screen presentation resources.</summary>
    public GameOverPresentation GameOver { get; }
    /// <summary>Selected options-menu artwork and display presentation; option values remain runtime state.</summary>
    public GameOptionsPresentation GameOptions { get; }
    /// <summary>Selected file-select screen presentation, separate from per-area maps and save-marker coordinates.</summary>
    public FileSelectPresentation FileSelect { get; }
    /// <summary>Selected escape-sequence typewriter text and visual presentation.</summary>
    public EscapeTypewriterPresentation EscapeTypewriter { get; }
    /// <summary>Opening narration text and presentation, separate from its selected font characters.</summary>
    public IntroNarrationPresentation IntroNarration { get; }
    /// <summary>The characters the ending's typewriter types, compiled by the import from the installed ending text (#1467).</summary>
    public EndingTypewriterText EndingText { get; }
    /// <summary>
    /// The colour the title shows behind its floor and objects (#1512); the title's other colours are pre-coloured into its
    /// sheets by the import.
    /// </summary>
    public RgbColor TitleBackdrop { get; }
    /// <summary>Title-screen gradient presentation, separate from its static palette and character artwork.</summary>
    public TitleGradientPresentation TitleGradient { get; }
    /// <summary>Mother Brain's rainbow beam backdrop colours.</summary>
    public MotherBrainRainbowPalettePresentation MotherBrainRainbowPalette { get; }
    /// <summary>The Ceres haze's installed tints.</summary>
    public CeresHazeTints CeresHaze { get; }
    /// <summary>The glow colours Power Bomb, Crystal Flash, and Ceres explosions add to the scene, separate from Samus palette colors.</summary>
    public InstalledPowerBombGlowColors PowerBombGlowColors { get; }

    /// <summary>The Speed Booster delays the getaway's wing frames leave behind for the unauthored cadence.</summary>
    public CeresRidleyGetawayBoostDelays CeresRidleyGetawayBoostDelays { get; }

    /// <summary>Returns an area's selected map presentation with verified stock exploration and station-reveal rules.</summary>
    /// <param name="area">One of the seven areas, including Ceres.</param>
    /// <returns>The immutable view compiled when this catalog was loaded.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not an area.</exception>
    public IAreaMapView Get(AreaId area) => areas[AreaIds.Require(area)];
}
