using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Core.Assets;

/// <summary>Owned artwork fixtures for draws whose geometry and bindings a test checks, not their pixels.</summary>
internal static class SpriteFrameTestArtwork
{
    /// <summary>A blank standard OBJ page with every nine-bit OBJ character plus large-part overhang, like the installed one.</summary>
    internal static SpriteArtwork StandardObjects { get; } = new(new SpriteArtworkId(SpriteArtworkKind.StandardObjects),
        new byte[(2 * 256 + SpriteArtwork.CharactersPerRow + 1) * SpriteArtwork.CharacterByteCount]);

    /// <summary>A grapple atlas of blank characters, for checks of the beam's geometry and cadence, not its pixels.</summary>
    internal static SuperMetroid.Core.Samus.Artwork.GrappleTileAtlas BlankGrapple { get; } = LoadBlankGrapple();

    /// <summary>Compiles an all-zero indexed PNG of the grapple atlas's size.</summary>
    private static SuperMetroid.Core.Samus.Artwork.GrappleTileAtlas LoadBlankGrapple()
    {
        using var png = new MemoryStream();
        IndexedPng.Write(png, new(SuperMetroid.Core.Samus.Artwork.GrappleTileDefinitions.Width,
                SuperMetroid.Core.Samus.Artwork.GrappleTileDefinitions.Height),
            new byte[SuperMetroid.Core.Samus.Artwork.GrappleTileDefinitions.Width * SuperMetroid.Core.Samus.Artwork.GrappleTileDefinitions.Height],
            SnesGraphics.DiagnosticPalette(16));
        png.Position = 0;
        return SuperMetroid.Core.Samus.Artwork.GrappleTileAtlas.Load(png);
    }

    /// <summary>Installed Samus spritemaps bound to <see cref="StandardObjects"/>, which holds every character any of them names.</summary>
    internal static SuperMetroid.Core.Samus.Drawing.SamusSpritemapArtwork Spritemaps(
        SuperMetroid.Core.Samus.Artwork.SamusSpritemapArtworkCatalog spritemaps) =>
        new(spritemaps, StandardObjects, () => StandardObjects);
}
