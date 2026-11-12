using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Fx;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed map presentation as the game reads it (#1466, #1544, #1636): what the game binds, with the installed
/// documents its picture sheets' frames are named from and the import composes from. It reads no character page: the
/// characters are import data the installation never holds, which the import reads with
/// <see cref="ImportAreaMapPresentation"/>, as it does the colours only the import reads.
/// </summary>
public class InstalledAreaMapPresentation : AreaMapPresentation
{
    /// <summary>Takes the presentation's parts and the installed documents it was read with.</summary>
    internal InstalledAreaMapPresentation(AreaMapPresentationParts parts, InstalledPresentationSources sources) : base(parts)
    {
        Ensure.NotNull(sources);
        Screens = Ensure.NotNull(sources.Screens);
        Pause = Ensure.NotNull(sources.Pause);
        StaffCredits = Ensure.NotNull(sources.StaffCredits);
        EndingTextPresentation = Ensure.NotNull(sources.EndingText);
        Messages = Ensure.NotNull(sources.Messages);
        RoomFxPages = Ensure.NotNull(sources.RoomFxPages);
        HighlightColours = Ensure.NotNull(sources.HighlightColours);
    }

    /// <summary>
    /// The map highlight cycle with each frame's colours (#1512): only the import reads the colours, and the game takes each
    /// frame's hold from the same frames (<see cref="AreaMapPresentation.HighlightCycle"/>).
    /// </summary>
    public MapPaletteCycle HighlightColours { get; }

    /// <summary>The highlight cycle the installed map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static MapPaletteCycle HighlightColoursOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.HighlightColours
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no highlight colours.");

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
