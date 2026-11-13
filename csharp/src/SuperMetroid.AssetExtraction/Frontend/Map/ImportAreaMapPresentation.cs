using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The map presentation as the import reads it (#1544, #1636, #1638): the game's presentation with the characters and the
/// documents whose entries are the original's packed tile placements, from the component's import data, and the colours only
/// the import pre-colours sheets and pictures from. The game reads none of them; its own read of the map presentation
/// (<see cref="MapPresentationFiles.Load"/>) reads no import data and leaves the colour documents unread.
/// </summary>
[ArchitectureReview("context-bag", "The import's characters and colours of the map presentation; every member is immutable artwork data the import composes and pre-colours pictures from, not a service.")]
public sealed class ImportAreaMapPresentation : InstalledAreaMapPresentation
{
    /// <summary>
    /// Takes the catalog the game binds, the installed documents, and the characters, colours and documents only the import
    /// reads.
    /// </summary>
    internal ImportAreaMapPresentation(AreaMapPresentation game, InstalledPresentationSources sources, ImportPresentationCharacters characters,
        ImportPresentationColours colours, ImportPresentationDocuments documents)
        : base(game, sources)
    {
        Ensure.NotNull(documents);
        FileSelect = Ensure.NotNull(documents.FileSelect);
        Pause = Ensure.NotNull(documents.Pause);
        Ensure.NotNull(characters);
        Characters = Ensure.NotNull(characters.Characters);
        Sprites = Ensure.NotNull(characters.Sprites);
        EscapeTimerTiles = Ensure.NotNull(characters.EscapeTimerTiles);
        RoomFxAnimatedTiles = Ensure.NotNull(characters.RoomFxAnimatedTiles);
        WorldCharacters = Ensure.NotNull(characters.WorldCharacters);
        IntroFont = Ensure.NotNull(characters.IntroFont);
        EndingFont = Ensure.NotNull(characters.EndingFont);
        TitleGraphics = Ensure.NotNull(characters.TitleGraphics);
        Ensure.NotNull(colours);
        SamusColours = Ensure.NotNull(colours.SamusColours);
        TitlePalette = Ensure.NotNull(colours.TitlePalette);
        Palettes = Ensure.NotNull(colours.Palettes);
        CeresRidleyColors = Ensure.NotNull(colours.CeresRidleyColors);
        CeresRidleyFloorColors = Ensure.NotNull(colours.CeresRidleyFloorColors);
        MotherBrainHealthPalette = Ensure.NotNull(colours.MotherBrainHealthPalette);
        MotherBrainRoomColors = Ensure.NotNull(colours.MotherBrainRoomColors);
        RoomFxPaletteBlends = Ensure.NotNull(colours.RoomFxPaletteBlends);
        RoomPaletteFx = Ensure.NotNull(colours.RoomPaletteFx);
        MotherBrainRainbowColours = Ensure.NotNull(colours.MotherBrainRainbow);
    }

    /// <summary>
    /// The file-select screen's pages, fields and compositions, import data (#1638), which the import composes the file-select
    /// and menu sheets from, with the installed layout the game binds (<see cref="AreaMapPresentation.FileSelect"/>).
    /// </summary>
    public FileSelectSourcePresentation FileSelect { get; }

    /// <summary>
    /// The pause screen's backdrops, wireframes and equipment-page artwork, import data (#1638), which the import pictures into
    /// the pause sheet (#1467).
    /// </summary>
    public PauseScreenArtwork Pause { get; }

    /// <summary>The characters the map screens, the pause interface and the HUD are pictured from.</summary>
    public MapCharacterArtwork Characters { get; }

    /// <summary>The map and menu sprites; the game draws them from the map sprite sheet, which the import composes from these (#1463).</summary>
    public MapSpriteCompositions Sprites { get; }

    /// <summary>The escape timer's characters, which the import composes the escape encounters' sheets from (#1513).</summary>
    public EscapeTimerCharacters EscapeTimerTiles { get; }

    /// <summary>
    /// The animated tiles' characters, which the import composes room sheets and the Tourian statues' pictures from (#1527).
    /// </summary>
    public RoomFxAnimatedTileAtlas RoomFxAnimatedTiles { get; }

    /// <summary>The world map's characters, which the import composes the world-map and menu picture sheets from (#1583).</summary>
    public WorldMapCharacters WorldCharacters { get; }

    /// <summary>The opening narration's font, which the import composes the narration pictures from (#1583).</summary>
    public IntroFontCharacters IntroFont { get; }

    /// <summary>The ending's font, which the import composes the ending's text and explosion pictures from (#1583).</summary>
    public EndingFontCharacters EndingFont { get; }

    /// <summary>The title's characters and compositions, which the import composes the title sheets from (#1583).</summary>
    public TitleGraphicsCharacters TitleGraphics { get; }

    /// <summary>
    /// Mother Brain's rainbow, drain, revival, fake-death and restoration colours, which the import pre-colours her sheets
    /// and body layer from (#1512). The game keeps only the beam backdrop's colours taken from them
    /// (<see cref="AreaMapPresentation.MotherBrainRainbowPalette"/>).
    /// </summary>
    public InstalledMotherBrainRainbowPalette MotherBrainRainbowColours { get; }

    /// <summary>
    /// The palette-effect colours, which the import pre-colours the sheets each palette-effect record shows from (#1512,
    /// #1583). The game keeps only the backdrops of the space behind the exploding planet taken from them
    /// (<see cref="AreaMapPresentation.EndingSpaceBackdrops"/>).
    /// </summary>
    public RoomPaletteFxPresentation RoomPaletteFx { get; }

    /// <summary>Mother Brain's health-band colours, which the import pre-colours her sheets and body layer from (#1527).</summary>
    public MotherBrainHealthPalettePresentation MotherBrainHealthPalette { get; }

    /// <summary>Mother Brain's room colours, which the import pre-colours her room's pictures and sheets from (#1527).</summary>
    public MotherBrainRoomColorPresentation MotherBrainRoomColors { get; }

    /// <summary>The room-FX blends, which the import pre-colours the effect overlay pictures with (#1527).</summary>
    public RoomFxBlendInks RoomFxPaletteBlends { get; }

    /// <summary>
    /// Ceres Ridley's colours, the shared Norfair Ridley health colours and the Ceres Baby's colours, which the import
    /// pre-colours their sheets and Ceres Ridley's room from (#1512).
    /// </summary>
    public CeresRidleyColorCatalog CeresRidleyColors { get; }

    /// <summary>Ceres Ridley's Mode 7 floor colours for each getaway zoom step, which the import pre-colours the floor's shades from (#1512).</summary>
    public CeresRidleyFloorColorCatalog CeresRidleyFloorColors { get; }

    /// <summary>The colours each map and menu screen loads, which the import pre-colours the screens' sheets from (#1512).</summary>
    public MapStaticPalettes Palettes { get; }

    /// <summary>The title's selected colours, which the import pre-colours the title's sheets from (#1512).</summary>
    public TitlePalettePresentation TitlePalette { get; }

    /// <summary>Samus's selected colour tables, which the import pre-colours her sheets from (#1512).</summary>
    public SamusMapColourTables SamusColours { get; }

}
