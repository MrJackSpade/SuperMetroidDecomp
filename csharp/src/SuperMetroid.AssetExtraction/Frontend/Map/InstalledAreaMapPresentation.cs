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
    /// <summary>Takes the presentation's parts, the characters it was read with and the editable gameplay messages.</summary>
    internal InstalledAreaMapPresentation(AreaMapPresentationParts parts, MapCharacterArtwork characters, GameplayMessageArtwork messages) : base(parts)
    {
        Characters = Ensure.NotNull(characters);
        Messages = Ensure.NotNull(messages);
    }

    /// <summary>The characters the map screens, the pause interface and the HUD are pictured from.</summary>
    public MapCharacterArtwork Characters { get; }

    /// <summary>The editable gameplay messages, whose words the import composes each message box image from (#1466).</summary>
    public GameplayMessageArtwork Messages { get; }
}
