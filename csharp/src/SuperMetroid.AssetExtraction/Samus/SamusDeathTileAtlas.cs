using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable four-bit characters of Samus's suit explosion as she dies, from which its pre-coloured sheet is composed.</summary>
public sealed class SamusDeathTileAtlas
{
    /// <summary>The installed explosion characters, in atlas order.</summary>
    private readonly byte[] planar;

    /// <summary>Validates the five pages and keeps every installed byte.</summary>
    private SamusDeathTileAtlas(byte[] planar)
    {
        if (planar.Length != SamusDeathTileAtlasFormat.TotalByteCount)
            throw new InvalidDataException("Samus death tile atlas has the wrong size.");
        this.planar = planar;
    }

    /// <summary>SHA-256 of all five selected death-explosion character pages.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusDeathTileAtlas),
        content => content.Append("death characters", planar));

    /// <summary>Loads the 64-by-160 indexed PNG and encodes its palette indices as 4-bpp characters in atlas order.</summary>
    /// <param name="png">Caller-owned artwork stream; pixel indices are pens 0..15, not final colours.</param>
    /// <returns>Installed explosion characters, without changing animation timing or palettes.</returns>
    public static SamusDeathTileAtlas Load(Stream png)
    {
        IndexedPngImage image = IndexedPng.Read(png,
            new(SamusDeathTileAtlasFormat.Width, SamusDeathTileAtlasFormat.Height));
        return new SamusDeathTileAtlas(SnesPlanarTileEncoder.Encode(image.Pixels,
            image.Size, SamusDeathTileAtlasFormat.BitsPerPixel));
    }

    /// <summary>The 160 explosion characters in the order the explosion's spritemaps name them.</summary>
    public SpriteArtwork Explosion => field ??= ComposeExplosion();

    /// <summary>Reorders the atlas's pages into the explosion's character order.</summary>
    private SpriteArtwork ComposeExplosion()
    {
        var characters = new byte[SamusDeathTileAtlasFormat.TotalByteCount];
        for (int index = 0; index < characters.Length; index++)
            characters[SamusDeathTileAtlasFormat.ExplosionByte(index)] = planar[index];
        return new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.SamusDeathExplosion), characters);
    }
}
