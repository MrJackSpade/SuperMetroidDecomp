using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Frontend.GameOver.Artwork;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The map presentation as the import reads it (#1544, #1636, #1638, #1693): the game's presentation with the characters,
/// the documents whose entries are the original's packed tile placements and the staff credits, from the component's import
/// data, and the colours only the import pre-colours sheets and pictures from. The game reads none of them; its own read of
/// the map presentation (<see cref="MapPresentationFiles.Load"/>) reads no import data.
/// </summary>
[ArchitectureReview("context-bag", "The import's characters and colours of the map presentation; every member is immutable artwork data the import composes and pre-colours pictures from, not a service.")]
public sealed class ImportAreaMapPresentation : InstalledAreaMapPresentation
{
    /// <summary>Takes the catalog the game binds and the characters, colours and documents only the import reads.</summary>
    internal ImportAreaMapPresentation(AreaMapPresentation game, ImportPresentationCharacters characters,
        ImportPresentationColours colours, ImportPresentationDocuments documents)
        : base(game)
    {
        Ensure.NotNull(documents);
        StaffCredits = Ensure.NotNull(documents.StaffCredits);
        FileSelect = Ensure.NotNull(documents.FileSelect);
        Pause = Ensure.NotNull(documents.Pause);
        RoomFxPages = Ensure.NotNull(documents.RoomFxPages);
        Messages = Ensure.NotNull(documents.Messages);
        GameplayHud = Ensure.NotNull(documents.GameplayHud);
        Screens = Ensure.NotNull(documents.Screens);
        Ensure.NotNull(documents.Menus);
        PauseSelectors = Ensure.NotNull(documents.Menus.PauseSelectors);
        PauseReserveTanks = Ensure.NotNull(documents.Menus.PauseReserveTanks);
        GameOverSource = Ensure.NotNull(documents.Menus.GameOver);
        GameOptions = Ensure.NotNull(documents.Menus.GameOptions);
        EndingTextPresentation = Ensure.NotNull(documents.Menus.EndingText);
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
        HighlightColours = Ensure.NotNull(colours.HighlightColours);
    }

    /// <summary>The staff credits, import data (#1693), which the import pictures into the ending's credits rows (#1467).</summary>
    public CreditsPresentation StaffCredits { get; }

    /// <summary>
    /// The map highlight cycle's colours, import data (#1693), which the map sprite sheet's highlight shades are pre-coloured
    /// from (#1512); the game takes each frame's hold from the installed cycle (<see cref="AreaMapPresentation.HighlightCycle"/>).
    /// </summary>
    public MapPaletteCycle HighlightColours { get; }

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

    /// <summary>The room effects' BG3 pages, import data (#1638), which the import pictures into each tileset's BG3 sheet (#1466, #1467).</summary>
    public RoomFxLayer3Pages RoomFxPages { get; }

    /// <summary>The gameplay messages, import data (#1638), whose words the import composes each message box picture from (#1466).</summary>
    public GameplayMessageArtwork Messages { get; }

    /// <summary>
    /// The gameplay HUD's tiles, import data (#1638), which the import pictures every HUD image from, with the installed layout
    /// the game binds (<see cref="AreaMapPresentation.GameplayHud"/>).
    /// </summary>
    public GameplayHudArtwork GameplayHud { get; }

    /// <summary>The map screens' pages, import data (#1638), which the import pictures for the world map and the room maps (#1467).</summary>
    public MapScreenPresentation Screens { get; }

    /// <summary>
    /// The equipment selectors' frame compositions, import data (#1638), which the import composes the pause sheet from, with
    /// the anchors and timing the installed layout holds for the game (<see cref="AreaMapPresentation.PauseSelectors"/>).
    /// </summary>
    public PauseSelectorSourcePresentation PauseSelectors { get; }

    /// <summary>
    /// The reserve strip's frame compositions, import data (#1638), which the import composes the pause sheet from, with the
    /// anchors the installed layout holds for the game (<see cref="AreaMapPresentation.PauseReserveTanks"/>).
    /// </summary>
    public PauseReserveTankSourcePresentation PauseReserveTanks { get; }

    /// <summary>The game-over page's tiles, compositions and palette selectors, import data (#1616, #1638).</summary>
    public GameOverSourcePresentation GameOverSource { get; }

    /// <summary>
    /// The options screen's pages, labels and compositions, import data (#1638), which the import composes the options and
    /// menu sheets from, with the anchors the installed layout holds for the game (<see cref="AreaMapPresentation.GameOptions"/>).
    /// </summary>
    public GameOptionsSourcePresentation GameOptions { get; }

    /// <summary>The ending text's panels and subtitle, import data (#1638), which the import pictures (#1467).</summary>
    public EndingTextPresentation EndingTextPresentation { get; }

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
    /// and body layer from (#1512). The game works with her beam backdrop's colours alone, installed by purpose
    /// (<see cref="AreaMapPresentation.MotherBrainRainbowPalette"/>, #1637).
    /// </summary>
    public InstalledMotherBrainRainbowPalette MotherBrainRainbowColours { get; }

    /// <summary>
    /// The palette-effect colours, which the import pre-colours the sheets each palette-effect record shows from (#1512,
    /// #1583). The game shows only the backdrops of the space behind the exploding planet, installed with the ending's
    /// backdrops (#1637).
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
