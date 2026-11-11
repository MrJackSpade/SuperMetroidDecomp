using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed map presentation as the game reads it (#1466, #1544): what the game binds, with the characters its
/// screens and the HUD are pictured from, which only the import composes from. The game's read leaves the colours only the
/// import reads unread; the import reads them with <see cref="ImportAreaMapPresentation"/>.
/// </summary>
public class InstalledAreaMapPresentation : AreaMapPresentation
{
    /// <summary>Takes the presentation's parts and the source artwork it was read with, which only the import composes from.</summary>
    internal InstalledAreaMapPresentation(AreaMapPresentationParts parts, InstalledPresentationSources sources) : base(parts)
    {
        Ensure.NotNull(sources);
        Screens = Ensure.NotNull(sources.Screens);
        Pause = Ensure.NotNull(sources.Pause);
        StaffCredits = Ensure.NotNull(sources.StaffCredits);
        EndingTextPresentation = Ensure.NotNull(sources.EndingText);
        Characters = Ensure.NotNull(sources.Characters);
        Messages = Ensure.NotNull(sources.Messages);
        RoomFxPages = Ensure.NotNull(sources.RoomFxPages);
        Sprites = Ensure.NotNull(sources.Sprites);
        HighlightColours = Ensure.NotNull(sources.HighlightColours);
        EscapeTimerTiles = Ensure.NotNull(sources.EscapeTimerTiles);
        RoomFxAnimatedTiles = Ensure.NotNull(sources.RoomFxAnimatedTiles);
        WorldCharacters = Ensure.NotNull(sources.WorldCharacters);
        IntroFont = Ensure.NotNull(sources.IntroFont);
        EndingFont = Ensure.NotNull(sources.EndingFont);
        TitleGraphics = Ensure.NotNull(sources.TitleGraphics);
        GameOverSource = Ensure.NotNull(sources.GameOver);
    }

    /// <summary>The game-over page's tiles, compositions and palette selectors, which only the import reads (#1616).</summary>
    public GameOverSourcePresentation GameOverSource { get; }

    /// <summary>The game-over source presentation the installed presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static GameOverSourcePresentation GameOverSourceOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.GameOverSource
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no game-over source presentation.");

    /// <summary>The world map's characters, which only the import reads to compose the world-map and menu picture sheets (#1583).</summary>
    public WorldMapCharacters WorldCharacters { get; }

    /// <summary>The world-map characters the installed presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static WorldMapCharacters WorldCharactersOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.WorldCharacters
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no world-map characters.");

    /// <summary>The opening narration's font, which only the import reads to compose the narration pictures (#1583).</summary>
    public IntroFontCharacters IntroFont { get; }

    /// <summary>The opening font the installed presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static IntroFontCharacters IntroFontOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.IntroFont
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no opening font.");

    /// <summary>The ending's font, which only the import reads to compose the ending's text and explosion pictures (#1583).</summary>
    public EndingFontCharacters EndingFont { get; }

    /// <summary>The ending font the installed presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static EndingFontCharacters EndingFontOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.EndingFont
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no ending font.");

    /// <summary>The title's characters and compositions, which only the import reads to compose the title sheets (#1583).</summary>
    public TitleGraphicsCharacters TitleGraphics { get; }

    /// <summary>The title graphics the installed presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static TitleGraphicsCharacters TitleGraphicsOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.TitleGraphics
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no title graphics.");

    /// <summary>
    /// The animated tiles' installed characters, which only the import reads to compose room sheets and the Tourian
    /// statues' pictures (#1527).
    /// </summary>
    public RoomFxAnimatedTileAtlas RoomFxAnimatedTiles { get; }

    /// <summary>The animated tiles' characters the installed presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static RoomFxAnimatedTileAtlas RoomFxAnimatedTilesOf(IGameplayPresentationContent maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.RoomFxAnimatedTiles
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no animated tiles.");

    /// <summary>The escape timer's characters, which only the import reads to compose the escape encounters' sheets (#1513).</summary>
    public EscapeTimerCharacters EscapeTimerTiles { get; }

    /// <summary>
    /// The map highlight cycle with each frame's colours (#1512): only the import reads the colours, and the game takes each
    /// frame's hold from the same frames (<see cref="AreaMapPresentation.HighlightCycle"/>).
    /// </summary>
    public MapPaletteCycle HighlightColours { get; }

    /// <summary>The highlight cycle the installed map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static MapPaletteCycle HighlightColoursOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.HighlightColours
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no highlight colours.");

    /// <summary>The installed map and menu sprites; the game draws them from the map sprite sheet, which the import composes from these (#1463).</summary>
    public MapSpriteCompositions Sprites { get; }

    /// <summary>The map and menu sprites the installed map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static MapSpriteCompositions SpritesOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.Sprites
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no map sprites.");

    /// <summary>The characters the map screens, the pause interface and the HUD are pictured from.</summary>
    public MapCharacterArtwork Characters { get; }

    /// <summary>The editable gameplay messages, whose words the import composes each message box image from (#1466).</summary>
    public GameplayMessageArtwork Messages { get; }

    /// <summary>The editable room effects' BG3 pages, which the import pictures into each tileset's BG3 sheet (#1466, #1467).</summary>
    public RoomFxLayer3Pages RoomFxPages { get; }

    /// <summary>The map screens' pages, which the import pictures for the world map and the room maps (#1467).</summary>
    public MapScreenPresentation Screens { get; }

    /// <summary>The pause screen's backdrops, wireframes and equipment-page artwork, which the import pictures into the pause sheet (#1467).</summary>
    public PauseScreenArtwork Pause { get; }

    /// <summary>The pause screen's artwork the installed map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static PauseScreenArtwork PauseOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.Pause
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no pause screen artwork.");

    /// <summary>The map screens' pages the installed map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static MapScreenPresentation ScreensOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.Screens
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no map screens.");

    /// <summary>The editable staff credits, which the import pictures into the ending's credits rows (#1467).</summary>
    public CreditsPresentation StaffCredits { get; }

    /// <summary>The editable ending text, whose pages and glyphs the import pictures and whose typewriter characters it compiles (#1467).</summary>
    public EndingTextPresentation EndingTextPresentation { get; }

    /// <summary>The staff credits the installed map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static CreditsPresentation StaffCreditsOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.StaffCredits
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no staff credits.");

    /// <summary>The ending text the installed map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static EndingTextPresentation EndingTextOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.EndingTextPresentation
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no ending text.");

    /// <summary>The room effects' pages the installed map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static RoomFxLayer3Pages RoomFxPagesOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.RoomFxPages
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no room effect pages.");
}
