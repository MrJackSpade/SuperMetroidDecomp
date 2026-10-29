namespace SuperMetroid.Core.Assets;

/// <summary>Installed artwork shown as a sprite look: the standard sprites, an escape timer page or a gunship takeoff page.</summary>
/// <param name="Artwork">The installed artwork.</param>
public sealed record InstalledArtworkCharacters(InstalledArtworkId Artwork) : SpriteArtworkSource
{
    /// <summary>Names the source for diagnostics and for the pictures keyed by it.</summary>
    public override string ToString() => $"asset {Artwork}";
}
