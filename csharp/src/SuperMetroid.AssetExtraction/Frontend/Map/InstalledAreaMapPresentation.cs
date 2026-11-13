using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed map presentation (#1466, #1544, #1616, #1636): the catalog the game binds (<see cref="Game"/>), and beside
/// it the installed documents its picture sheets' frames are named from and the import composes from, which the game never
/// receives. It reads no character page: the characters are import data the installation never holds, which the import
/// reads with <see cref="ImportAreaMapPresentation"/>, as it does the colours only the import reads.
/// </summary>
[ArchitectureReview("context-bag", "The installed map presentation's documents beside the catalog the game binds; every member is immutable artwork data the import composes pictures from, not a service.")]
public class InstalledAreaMapPresentation
{
    /// <summary>Takes the catalog the game binds and the installed documents it was read with.</summary>
    internal InstalledAreaMapPresentation(AreaMapPresentation game, InstalledPresentationSources sources)
    {
        Game = Ensure.NotNull(game);
        Ensure.NotNull(sources);
        Screens = Ensure.NotNull(sources.Screens);
        Pause = Ensure.NotNull(sources.Pause);
        StaffCredits = Ensure.NotNull(sources.StaffCredits);
        EndingTextPresentation = Ensure.NotNull(sources.EndingText);
        Messages = Ensure.NotNull(sources.Messages);
        RoomFxPages = Ensure.NotNull(sources.RoomFxPages);
        HighlightColours = Ensure.NotNull(sources.HighlightColours);
        PauseSelectors = Ensure.NotNull(sources.PauseSelectors);
        PauseReserveTanks = Ensure.NotNull(sources.PauseReserveTanks);
        GameplayHud = Ensure.NotNull(sources.GameplayHud);
        GameOptions = Ensure.NotNull(sources.GameOptions);
        FileSelect = Ensure.NotNull(sources.FileSelect);
        GameOverSource = Ensure.NotNull(sources.GameOver);
    }

    /// <summary>The catalog the game binds: only what the game reads of the installed presentation (#1616).</summary>
    public AreaMapPresentation Game { get; }

    /// <summary>The game-over page's tiles, compositions and palette selectors, which only the import reads (#1616).</summary>
    public GameOverSourcePresentation GameOverSource { get; }

    /// <summary>
    /// The map highlight cycle with each frame's colours (#1512): only the import reads the colours, and the game takes each
    /// frame's hold from the same frames (<see cref="AreaMapPresentation.HighlightCycle"/>).
    /// </summary>
    public MapPaletteCycle HighlightColours { get; }

    /// <summary>The editable gameplay messages, whose words the import composes each message box image from (#1466).</summary>
    public GameplayMessageArtwork Messages { get; }

    /// <summary>The editable room effects' BG3 pages, which the import pictures into each tileset's BG3 sheet (#1466, #1467).</summary>
    public RoomFxLayer3Pages RoomFxPages { get; }

    /// <summary>The map screens' pages, which the import pictures for the world map and the room maps (#1467).</summary>
    public MapScreenPresentation Screens { get; }

    /// <summary>The pause screen's backdrops, wireframes and equipment-page artwork, which the import pictures into the pause sheet (#1467).</summary>
    public PauseScreenArtwork Pause { get; }

    /// <summary>The editable staff credits, which the import pictures into the ending's credits rows (#1467).</summary>
    public CreditsPresentation StaffCredits { get; }

    /// <summary>The editable ending text, whose pages and glyphs the import pictures and whose typewriter characters it compiles (#1467).</summary>
    public EndingTextPresentation EndingTextPresentation { get; }

    /// <summary>
    /// The equipment selectors' frame compositions, which the import composes the pause sheet from, with the anchors and
    /// timing the game binds (<see cref="AreaMapPresentation.PauseSelectors"/>).
    /// </summary>
    public PauseSelectorSourcePresentation PauseSelectors { get; }

    /// <summary>
    /// The reserve strip's frame compositions, which the import composes the pause sheet from, with the anchors the game
    /// binds (<see cref="AreaMapPresentation.PauseReserveTanks"/>).
    /// </summary>
    public PauseReserveTankSourcePresentation PauseReserveTanks { get; }

    /// <summary>
    /// The gameplay HUD's tiles, which the import pictures every HUD image from, with the layout the game binds
    /// (<see cref="AreaMapPresentation.GameplayHud"/>).
    /// </summary>
    public GameplayHudArtwork GameplayHud { get; }

    /// <summary>
    /// The options screen's pages, labels and compositions, which the import composes the options and menu sheets from, with
    /// the anchors the game binds (<see cref="AreaMapPresentation.GameOptions"/>).
    /// </summary>
    public GameOptionsSourcePresentation GameOptions { get; }

    /// <summary>
    /// The file-select screen's pages, fields and compositions, which the import composes the file-select and menu sheets
    /// from, with the layout the game binds (<see cref="AreaMapPresentation.FileSelect"/>).
    /// </summary>
    public FileSelectSourcePresentation FileSelect { get; }
}
