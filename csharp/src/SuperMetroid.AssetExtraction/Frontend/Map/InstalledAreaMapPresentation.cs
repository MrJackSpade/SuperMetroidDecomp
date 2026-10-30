using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed map presentation as the import reads it (#1466): what the game reads, with the characters its screens
/// and the HUD are pictured from, which only the import composes from.
/// </summary>
public sealed class InstalledAreaMapPresentation : AreaMapPresentation
{
    /// <summary>
    /// Takes the presentation's parts, the characters it was read with, the editable gameplay messages and the room
    /// effects' BG3 pages.
    /// </summary>
    internal InstalledAreaMapPresentation(AreaMapPresentationParts parts, MapCharacterArtwork characters, GameplayMessageArtwork messages,
        RoomFxLayer3Pages roomFxPages, CreditsPresentation staffCredits, EndingTextPresentation endingText, MapScreenPresentation screens) : base(parts)
    {
        Screens = Ensure.NotNull(screens);
        StaffCredits = Ensure.NotNull(staffCredits);
        EndingTextPresentation = Ensure.NotNull(endingText);
        Characters = Ensure.NotNull(characters);
        Messages = Ensure.NotNull(messages);
        RoomFxPages = Ensure.NotNull(roomFxPages);
    }

    /// <summary>The characters the map screens, the pause interface and the HUD are pictured from.</summary>
    public MapCharacterArtwork Characters { get; }

    /// <summary>The editable gameplay messages, whose words the import composes each message box image from (#1466).</summary>
    public GameplayMessageArtwork Messages { get; }

    /// <summary>The editable room effects' BG3 pages, which the import pictures into each tileset's BG3 sheet (#1466, #1467).</summary>
    public RoomFxLayer3Pages RoomFxPages { get; }

    /// <summary>The map screens' pages, which the import pictures for the world map and the room maps (#1467).</summary>
    public MapScreenPresentation Screens { get; }

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
