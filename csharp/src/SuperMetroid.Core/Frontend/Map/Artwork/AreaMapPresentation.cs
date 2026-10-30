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
/// Immutable map content snapshot. Overrides never alter stock provenance or exploration rules. The import reads it with
/// the characters its screens and the HUD are pictured from, which the game never reads (#1466).
/// </summary>
public class AreaMapPresentation : IInstalledArtworkContent, IGameplayPresentationContent
{
    /// <summary>One validated map view per retail area in area-ID order.</summary>
    private readonly IAreaMapView[] areas;
    /// <summary>Captures the selected maps and shared presentation resources as one immutable snapshot.</summary>
    internal AreaMapPresentation(AreaMapPresentationParts parts)
    {
        this.areas = parts.Areas;
        HiddenCells = parts.HiddenCells;
        ContentIdentity = parts.ContentIdentity;
        HighlightCycle = parts.HighlightCycle;
        Palettes = parts.Palettes;
        Labels = parts.Labels;
        Stations = parts.Stations;
        Landmarks = parts.Landmarks;
        SaveMarkers = parts.SaveMarkers;
        Arrows = parts.Arrows;
        WorldArtwork = parts.WorldArtwork;
        Sprites = parts.Sprites;
        PauseSelectors = parts.PauseSelectors;
        PauseReserveTanks = parts.PauseReserveTanks;
        PauseReserveArrow = parts.PauseReserveArrow;
        EscapeTimer = parts.EscapeTimer;
        EscapeTimerTiles = parts.EscapeTimerTiles;
        GameplayHud = parts.GameplayHud;
        GameOver = parts.GameOver;
        GameOptions = parts.GameOptions;
        FileSelect = parts.FileSelect;
        EscapeTypewriter = parts.EscapeTypewriter;
        IntroNarration = parts.IntroNarration;
        IntroFont = parts.IntroFont;
        EndingText = parts.EndingText;
        EndingFont = parts.EndingFont;
        TitleGraphics = parts.TitleGraphics;
        TitlePalette = parts.TitlePalette;
        TitleGradient = parts.TitleGradient;
        RoomPaletteFx = parts.RoomPaletteFx;
        MotherBrainHealthPalette = parts.MotherBrainHealthPalette;
        MotherBrainRainbowPalette = parts.MotherBrainRainbowPalette;
        MotherBrainRoomColors = parts.MotherBrainRoomColors;
        RoomFxAnimatedTiles = parts.RoomFxAnimatedTiles;
        RoomFxPaletteBlends = parts.RoomFxPaletteBlends;
        PowerBombFixedColors = parts.PowerBombFixedColors;
        SamusVisorColors = parts.SamusVisorColors;
        SamusHurtColors = parts.SamusHurtColors;
        SamusSuitColors = parts.SamusSuitColors;
        SamusFullBodyCycleColors = parts.SamusFullBodyCycleColors;
        CrystalFlashColors = parts.CrystalFlashColors;
        SamusChargeColors = parts.SamusChargeColors;
        CeresRidleyColors = parts.CeresRidleyColors;
        CeresRidleyFloorColors = parts.CeresRidleyFloorColors;
        CeresRidleyFloorUpdates = parts.CeresRidleyFloorUpdates;
        SamusHyperBeamColors = parts.SamusHyperBeamColors;
    }

    /// <summary>Uppercase SHA-256 identity of length-framed stock map rules, station masks, and selected presentation resources in fixed order; override edits affect identity without changing stock provenance.</summary>
    public string ContentIdentity { get; }
    /// <summary>The cells the map screens show in place of cells nothing reveals.</summary>
    public AreaMapHiddenCells HiddenCells { get; }
    /// <summary>Selected highlight color frames used by map presentation; animation progression remains engine-owned.</summary>
    public MapPaletteCycle HighlightCycle { get; }
    /// <summary>Complete static palettes for pause, file-select, and each world-map area, separate from the highlight cycle.</summary>
    public MapStaticPalettes Palettes { get; }
    /// <summary>World-map area-name drawing anchors, independent of area selection and map exploration.</summary>
    public WorldMapLabelPositions Labels { get; }
    /// <summary>Map-station label drawing positions; moving these does not change compiled discovery cells or station reveal masks.</summary>
    public MapStationPositions Stations { get; }
    /// <summary>Boss, elevator, and gunship drawing anchors, independent of progression and destination rules.</summary>
    public MapLandmarkPositions Landmarks { get; }
    /// <summary>File-select save-marker visual coordinate overrides, independent of native load targets and scrolling.</summary>
    public MapSaveMarkerPositions SaveMarkers { get; }
    /// <summary>Map-arrow artwork and presentation frames for scrolling indicators.</summary>
    public MapArrowPresentation Arrows { get; }
    /// <summary>Selected foreground and background PNG artwork for the world-map screen.</summary>
    public WorldMapArtwork WorldArtwork { get; }
    /// <summary>The installed map and menu sprites; the game draws them from the map sprite sheet, and their compositions belong to the import pipeline (#1463).</summary>
    public MapSpriteCompositions Sprites { get; }
    /// <summary>Pause selection-indicator artwork and presentation parameters; inventory and selection logic remain engine-owned.</summary>
    public PauseSelectorPresentation PauseSelectors { get; }
    /// <summary>Reserve-strip artwork and drawing origins, independent of reserve energy and fill selection.</summary>
    public PauseReserveTankPresentation PauseReserveTanks { get; }
    /// <summary>The reserve arrow's solid and pulse colours; its labels, digits and tiles are the pause sheet's pictures (#1467).</summary>
    public PauseReserveArrowColours PauseReserveArrow { get; }
    /// <summary>Escape countdown visual layout and digit presentation, separate from the character bytes in <see cref="EscapeTimerTiles"/>.</summary>
    public EscapeTimerPresentation EscapeTimer { get; }
    /// <summary>Selected character bytes for the two escape-timer VRAM transfers.</summary>
    public EscapeTimerTileAtlas EscapeTimerTiles { get; }
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
    /// <summary>Selected opening-narration character artwork used with <see cref="IntroNarration"/>.</summary>
    public IntroFontAtlas IntroFont { get; }
    /// <summary>The characters the ending's typewriter types, compiled by the import from the installed ending text (#1467).</summary>
    public EndingTypewriterText EndingText { get; }
    /// <summary>Selected character artwork for ending text and its font transfers.</summary>
    public EndingFontAtlas EndingFont { get; }
    /// <summary>Title Mode 7 characters and tilemap, OBJ characters, and Baby character artwork selected from their extracted files.</summary>
    public TitleGraphicsPresentation TitleGraphics { get; }
    /// <summary>Title-screen palette colors, separate from the title gradient and graphics transfers.</summary>
    public TitlePalettePresentation TitlePalette { get; }
    /// <summary>Title-screen gradient presentation, separate from its static palette and character artwork.</summary>
    public TitleGradientPresentation TitleGradient { get; }
    /// <summary>Room and cinematic palette-animation colors; instruction timing, CGRAM destinations, and side effects remain compiled.</summary>
    public RoomPaletteFxPresentation RoomPaletteFx { get; }
    /// <summary>Mother Brain health-selected palette artwork, separate from rainbow and room-color families.</summary>
    public MotherBrainHealthPalettePresentation MotherBrainHealthPalette { get; }
    /// <summary>Mother Brain rainbow, drain, revival, normal-restoration, and fake-death colors; older overrides inherit newly introduced rows from stock.</summary>
    public MotherBrainRainbowPalettePresentation MotherBrainRainbowPalette { get; }
    /// <summary>Mother Brain arena color presentation, separate from boss health and rainbow palettes; older overrides inherit new color families from verified stock.</summary>
    public MotherBrainRoomColorPresentation MotherBrainRoomColors { get; }
    /// <summary>Selected animated room-FX character artwork, separate from layer-3 tilemaps and palette blending.</summary>
    public RoomFxAnimatedTileAtlas RoomFxAnimatedTiles { get; }
    /// <summary>Selected layer-3 room-FX tilemaps, separate from animated characters and palette-blend colors.</summary>
    /// <summary>Selected room-FX palette-blend presentation, separate from layer-3 tilemap geometry and animated character bytes.</summary>
    public RoomFxBlendInks RoomFxPaletteBlends { get; }
    /// <summary>Fixed RGB5 colors for Power Bomb, Crystal Flash, and Ceres explosion effects, separate from Samus palette colors.</summary>
    public InstalledPowerBombFixedColors PowerBombFixedColors { get; }
    /// <summary>Visor colors shared by X-ray and room palette cycling, separate from complete suit palettes.</summary>
    public InstalledSamusVisorColors SamusVisorColors { get; }
    /// <summary>Samus hurt-flash and cinematic-restoration color artwork.</summary>
    public InstalledSamusHurtColors SamusHurtColors { get; }
    /// <summary>Base Power, Varia, and Gravity Suit colors, separate from transient full-body and charge cycles.</summary>
    public InstalledSamusSuitColors SamusSuitColors { get; }
    /// <summary>Samus full-body palette-cycle inputs, separate from static suit, visor, and charged-beam colors.</summary>
    public SamusFullBodyCycleColours SamusFullBodyCycleColors { get; }
    /// <summary>Crystal Flash body and bubble colors; native timing, frame selection, and completion remain engine-owned.</summary>
    public InstalledCrystalFlashColors CrystalFlashColors { get; }
    /// <summary>Charged-beam and pseudo-Screw-Attack palette inputs, separate from base suit colors.</summary>
    public SamusChargeColours SamusChargeColors { get; }
    /// <summary>Ceres Ridley colors, shared Norfair Ridley health colors, and private Baby drawing colors; Mode 7 zoom shades are supplied separately.</summary>
    public CeresRidleyColorCatalog CeresRidleyColors { get; }
    /// <summary>Ceres Ridley Mode 7 zoom shades, separate from ordinary Ridley colors and compiled movement and rotation.</summary>
    public CeresRidleyFloorColorCatalog CeresRidleyFloorColors { get; }
    /// <summary>Where the getaway's floor runs take their tile numbers from, which the floor updates it queues record.</summary>
    public CeresRidleyFloorUpdateSources CeresRidleyFloorUpdates { get; }
    /// <summary>Samus Hyper Beam palette-cycle colors, separate from charged-beam presentation and other suit cycles.</summary>
    public SamusHyperBeamColours SamusHyperBeamColors { get; }
    /// <summary>Resolves installed escape-timer character bytes for a VRAM transfer.</summary>
    /// <param name="asset">The transfer identity; this provider supports only the escape-timer identities in its catalog.</param>
    /// <returns>Read-only planar character bytes for the requested transfer.</returns>
    /// <exception cref="InvalidDataException">The requested identity is not supported by this provider.</exception>
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset) => asset switch
    {
        InstalledArtworkId.EscapeTimerFirstTiles or InstalledArtworkId.EscapeTimerSecondTiles => EscapeTimerTiles.Resolve(asset),
        InstalledArtworkId.StandardHudTiles or
            InstalledArtworkId.BeamPowerTiles or
            InstalledArtworkId.BeamWaveTiles or
            InstalledArtworkId.BeamIceTiles or
            InstalledArtworkId.BeamIceWaveTiles or
            InstalledArtworkId.BeamSpazerTiles or
            InstalledArtworkId.BeamSpazerWaveTiles or
            InstalledArtworkId.BeamSpazerIceTiles or
            InstalledArtworkId.BeamSpazerIceWaveTiles or
            InstalledArtworkId.BeamPlasmaTiles or
            InstalledArtworkId.BeamPlasmaWaveTiles or
            InstalledArtworkId.BeamPlasmaIceTiles or
            InstalledArtworkId.BeamPlasmaIceWaveTiles or
            InstalledArtworkId.ProjectileIceWaveTrailTiles or
            InstalledArtworkId.ProjectileMissileTrailTiles or
            InstalledArtworkId.GrapplePointFirstTiles or
            InstalledArtworkId.GrapplePointSecondTiles or
            InstalledArtworkId.GrapplePointThirdTiles or
            InstalledArtworkId.GrapplePointFourthTiles or
            InstalledArtworkId.GrappleHorizontalSegmentTiles or
            InstalledArtworkId.GrappleDiagonalSegmentTiles or
            InstalledArtworkId.GrappleVerticalSegmentTiles or
            InstalledArtworkId.GunshipLiftoffFirstTiles or
            InstalledArtworkId.GunshipLiftoffSecondTiles or
            InstalledArtworkId.GunshipLiftoffThirdTiles or
            InstalledArtworkId.GunshipLiftoffFourthTiles or
            InstalledArtworkId.GunshipLiftoffFifthTiles or
            InstalledArtworkId.BeamChainsawTiles or
            InstalledArtworkId.BeamSpacetimeTiles or
            InstalledArtworkId.StandardObjectTiles or
            InstalledArtworkId.HudTopRowTilemap => throw new InvalidDataException($"Map catalog cannot resolve VRAM asset {asset}."),
        _ => throw new InvalidDataException($"Map catalog cannot resolve VRAM asset {asset}."),
    };
    /// <summary>Returns an area's selected map presentation with verified stock exploration and station-reveal rules.</summary>
    /// <param name="area">One of the seven retail area identities, including Ceres.</param>
    /// <returns>The immutable view compiled when this catalog was loaded.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not a retail area identity.</exception>
    public IAreaMapView Get(AreaId area) => areas[AreaIds.ToIndex(area)];
}
