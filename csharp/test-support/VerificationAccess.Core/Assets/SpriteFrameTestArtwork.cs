namespace SuperMetroid.Core.Assets;

/// <summary>Owned artwork fixtures for draws whose geometry and bindings a test checks, not their pixels.</summary>
internal static class SpriteFrameTestArtwork
{
    /// <summary>A blank standard OBJ page with every nine-bit OBJ character plus large-part overhang, like the installed one.</summary>
    internal static SpriteArtwork StandardObjects { get; } = new(new SpriteArtworkId(SpriteArtworkKind.StandardObjects),
        new byte[(2 * 256 + SpriteArtwork.CharactersPerRow + 1) * SpriteArtwork.CharacterByteCount]);

    /// <summary>Installed Samus spritemaps bound to <see cref="StandardObjects"/>, which holds every character any of them names.</summary>
    internal static SuperMetroid.Core.Samus.Drawing.SamusSpritemapArtwork Spritemaps(
        SuperMetroid.Core.Samus.Artwork.InstalledSamusSpritemaps spritemaps) =>
        new(spritemaps, StandardObjects, () => StandardObjects);
}
