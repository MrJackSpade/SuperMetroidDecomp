using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;

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
        RoomFxLayer3Pages roomFxPages) : base(parts)
    {
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

    /// <summary>The room effects' pages the installed map presentation <paramref name="maps"/> was read with.</summary>
    /// <exception cref="InvalidOperationException">The presentation was not read from an installation.</exception>
    public static RoomFxLayer3Pages RoomFxPagesOf(AreaMapPresentation maps) => (Ensure.NotNull(maps) as InstalledAreaMapPresentation)?.RoomFxPages
        ?? throw new InvalidOperationException("The map presentation was not read from an installation, so it has no room effect pages.");
}
