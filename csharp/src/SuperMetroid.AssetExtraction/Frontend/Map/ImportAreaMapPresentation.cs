using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;
using SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The map presentation as the import reads it (#1544, #1636): the game's presentation with the characters, from the
/// component's import data, and the colours only the import pre-colours sheets and pictures from. The game reads neither;
/// its own read of the map presentation (<see cref="MapPresentationFiles.Load"/>) reads no character page and leaves the
/// colour documents unread.
/// </summary>
public sealed class ImportAreaMapPresentation : InstalledAreaMapPresentation
{
    /// <summary>Takes the game's presentation parts, the installed documents, the characters and the colours only the import reads.</summary>
    internal ImportAreaMapPresentation(AreaMapPresentationParts parts, InstalledPresentationSources sources, ImportPresentationCharacters characters,
        ImportPresentationColours colours)
        : base(parts, sources)
    {
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
    }

    /// <summary>The characters the map screens, the pause interface and the HUD are pictured from.</summary>
    public MapCharacterArtwork Characters { get; }

    /// <summary>The map and menu sprites; the game draws them from the map sprite sheet, which the import composes from these (#1463).</summary>
    public MapSpriteCompositions Sprites { get; }

    /// <summary>The map and menu sprites the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static MapSpriteCompositions SpritesOf(AreaMapPresentation maps) => Imported(maps, "map sprites").Sprites;

    /// <summary>The escape timer's characters, which the import composes the escape encounters' sheets from (#1513).</summary>
    public EscapeTimerCharacters EscapeTimerTiles { get; }

    /// <summary>
    /// The animated tiles' characters, which the import composes room sheets and the Tourian statues' pictures from (#1527).
    /// </summary>
    public RoomFxAnimatedTileAtlas RoomFxAnimatedTiles { get; }

    /// <summary>The animated tiles' characters the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static RoomFxAnimatedTileAtlas RoomFxAnimatedTilesOf(IGameplayPresentationContent maps) => Imported(maps, "animated tiles").RoomFxAnimatedTiles;

    /// <summary>The world map's characters, which the import composes the world-map and menu picture sheets from (#1583).</summary>
    public WorldMapCharacters WorldCharacters { get; }

    /// <summary>The world-map characters the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static WorldMapCharacters WorldCharactersOf(AreaMapPresentation maps) => Imported(maps, "world-map characters").WorldCharacters;

    /// <summary>The opening narration's font, which the import composes the narration pictures from (#1583).</summary>
    public IntroFontCharacters IntroFont { get; }

    /// <summary>The opening font the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static IntroFontCharacters IntroFontOf(AreaMapPresentation maps) => Imported(maps, "opening font").IntroFont;

    /// <summary>The ending's font, which the import composes the ending's text and explosion pictures from (#1583).</summary>
    public EndingFontCharacters EndingFont { get; }

    /// <summary>The ending font the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static EndingFontCharacters EndingFontOf(AreaMapPresentation maps) => Imported(maps, "ending font").EndingFont;

    /// <summary>The title's characters and compositions, which the import composes the title sheets from (#1583).</summary>
    public TitleGraphicsCharacters TitleGraphics { get; }

    /// <summary>The title graphics the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static TitleGraphicsCharacters TitleGraphicsOf(AreaMapPresentation maps) => Imported(maps, "title graphics").TitleGraphics;

    /// <summary>
    /// The palette-effect colours, which the import pre-colours the sheets each palette-effect record shows from (#1512,
    /// #1583). The game keeps only the backdrops of the space behind the exploding planet taken from them
    /// (<see cref="AreaMapPresentation.EndingSpaceBackdrops"/>).
    /// </summary>
    public RoomPaletteFxPresentation RoomPaletteFx { get; }

    /// <summary>The palette-effect colours the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static RoomPaletteFxPresentation RoomPaletteFxOf(IGameplayPresentationContent maps) => Imported(maps, "palette-effect colours").RoomPaletteFx;

    /// <summary>Mother Brain's health-band colours, which the import pre-colours her sheets and body layer from (#1527).</summary>
    public MotherBrainHealthPalettePresentation MotherBrainHealthPalette { get; }

    /// <summary>The Mother Brain health colours the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static MotherBrainHealthPalettePresentation MotherBrainHealthPaletteOf(AreaMapPresentation maps) => Imported(maps, "Mother Brain health colours").MotherBrainHealthPalette;

    /// <summary>Mother Brain's room colours, which the import pre-colours her room's pictures and sheets from (#1527).</summary>
    public MotherBrainRoomColorPresentation MotherBrainRoomColors { get; }

    /// <summary>The Mother Brain room colours the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static MotherBrainRoomColorPresentation MotherBrainRoomColorsOf(AreaMapPresentation maps) => Imported(maps, "Mother Brain room colours").MotherBrainRoomColors;

    /// <summary>The room-FX blends, which the import pre-colours the effect overlay pictures with (#1527).</summary>
    public RoomFxBlendInks RoomFxPaletteBlends { get; }

    /// <summary>The room-FX blends the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static RoomFxBlendInks RoomFxPaletteBlendsOf(IGameplayPresentationContent maps) => Imported(maps, "room-FX blends").RoomFxPaletteBlends;

    /// <summary>
    /// Ceres Ridley's colours, the shared Norfair Ridley health colours and the Ceres Baby's colours, which the import
    /// pre-colours their sheets and Ceres Ridley's room from (#1512).
    /// </summary>
    public CeresRidleyColorCatalog CeresRidleyColors { get; }

    /// <summary>The Ceres Ridley colours the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static CeresRidleyColorCatalog CeresRidleyColorsOf(AreaMapPresentation maps) => Imported(maps, "Ceres Ridley colours").CeresRidleyColors;

    /// <summary>Ceres Ridley's Mode 7 floor colours for each getaway zoom step, which the import pre-colours the floor's shades from (#1512).</summary>
    public CeresRidleyFloorColorCatalog CeresRidleyFloorColors { get; }

    /// <summary>The Ceres Ridley floor colours the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static CeresRidleyFloorColorCatalog CeresRidleyFloorColorsOf(AreaMapPresentation maps) => Imported(maps, "Ceres Ridley floor colours").CeresRidleyFloorColors;

    /// <summary>The colours each map and menu screen loads, which the import pre-colours the screens' sheets from (#1512).</summary>
    public MapStaticPalettes Palettes { get; }

    /// <summary>The screen colours the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static MapStaticPalettes PalettesOf(AreaMapPresentation maps) => Imported(maps, "screen colours").Palettes;

    /// <summary>The title's selected colours, which the import pre-colours the title's sheets from (#1512).</summary>
    public TitlePalettePresentation TitlePalette { get; }

    /// <summary>The title colours the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static TitlePalettePresentation TitlePaletteOf(AreaMapPresentation maps) => Imported(maps, "title colours").TitlePalette;

    /// <summary>Samus's selected colour tables, which the import pre-colours her sheets from (#1512).</summary>
    public SamusMapColourTables SamusColours { get; }

    /// <summary>Samus's colour tables the import's map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    public static SamusMapColourTables SamusColoursOf(AreaMapPresentation maps) => Imported(maps, "Samus colour tables").SamusColours;

    /// <summary><paramref name="maps"/> as the import read it.</summary>
    /// <param name="maps">The map presentation an import step was handed.</param>
    /// <param name="colours">The colours or characters the step asked for, for the failure's message.</param>
    /// <exception cref="InvalidOperationException">The presentation was not read for the import.</exception>
    internal static ImportAreaMapPresentation Imported(IGameplayPresentationContent maps, string colours) =>
        Ensure.NotNull(maps) as ImportAreaMapPresentation
        ?? throw new InvalidOperationException($"The map presentation was not read for the import, so it has no {colours}.");
}
