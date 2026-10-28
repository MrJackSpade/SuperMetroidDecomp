namespace SuperMetroid.Core.Assets;

/// <summary>Installed artwork shown over the sprite characters: the standard sprites, an escape timer run or a gunship takeoff run.</summary>
/// <param name="Artwork">The installed artwork.</param>
public sealed record InstalledArtworkCharacters(InstalledArtworkId Artwork) : SpriteArtworkSource
{
    /// <summary>Names the source for diagnostics and for the pictures keyed by it.</summary>
    public override string ToString() => $"asset {Artwork}";
}
