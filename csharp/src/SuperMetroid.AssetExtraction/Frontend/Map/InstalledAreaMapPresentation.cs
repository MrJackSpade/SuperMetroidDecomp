using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// The installed map presentation as the import reads it (#1466): what the game reads, with the characters its screens
/// and the HUD are pictured from, which only the import composes from.
/// </summary>
public sealed class InstalledAreaMapPresentation : AreaMapPresentation
{
    /// <summary>Takes the presentation's parts and the characters it was read with.</summary>
    internal InstalledAreaMapPresentation(AreaMapPresentationParts parts, MapCharacterArtwork characters) : base(parts) =>
        Characters = Ensure.NotNull(characters);

    /// <summary>The characters the map screens, the pause interface and the HUD are pictured from.</summary>
    public MapCharacterArtwork Characters { get; }
}
