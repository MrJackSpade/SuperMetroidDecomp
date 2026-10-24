using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Editable four-bit characters of Samus's suit explosion as she dies, from which its pre-coloured sheet is composed.</summary>
/// <remarks>The remaining 151 source tiles retain only the reviewed selected anatomical,
/// hair, suit-rupture and spark contours/pen membership of the extracted explosion. Pose and
/// timer select this drawing; generating its exact human figure and shading would
/// require re-embedding the chosen image. Six blank allocation tiles and three shared
/// patches calculate separately. This narrow bitmap exception excludes palettes,
/// OAM placement, sequence timing and every other atlas.</remarks>
public sealed class SamusDeathTileAtlas
{
    /// <summary>Independently supplied source bytes; padding and repeated patches are resolved on demand.</summary>
    internal readonly Dictionary<int, byte> sourceBytes;

    /// <summary>Validates the five pages and retains bytes that differ from their source relation.</summary>
    private SamusDeathTileAtlas(byte[] planar)
    {
        if (planar.Length != SamusDeathTileAtlasFormat.TotalByteCount)
            throw new InvalidDataException("Samus death tile atlas has the wrong size.");
        sourceBytes = Enumerable.Range(0, planar.Length)
            .Where(index => SamusDeathTileAtlasFormat.SourceByte(index) is int source &&
                (source == index || planar[index] != (source < 0 ? 0 : planar[source])))
            .ToDictionary(index => index, index => planar[index]);
    }

    /// <summary>SHA-256 of all five selected death-explosion character pages.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusDeathTileAtlas),
        content => content.Append("death characters", ReadRange(0, SamusDeathTileAtlasFormat.TotalByteCount)));

    /// <summary>Resolves an edited character byte through named repeats or transparent padding.</summary>
    private byte ReadByte(int index)
    {
        if (sourceBytes.TryGetValue(index, out byte value)) return value;
        int source = SamusDeathTileAtlasFormat.SourceByte(index);
        return source < 0 ? (byte)0 : source != index ? ReadByte(source)
            : throw new InvalidDataException("Death artwork source byte is unavailable.");
    }

    /// <summary>Materializes a contiguous run of atlas bytes.</summary>
    private byte[] ReadRange(int first, int count)
    {
        var bytes = new byte[count];
        for (int index = 0; index < count; index++) bytes[index] = ReadByte(first + index);
        return bytes;
    }

    /// <summary>Loads the 64-by-160 indexed PNG and encodes its palette indices as 4-bpp characters in atlas order, retaining independently edited source bytes.</summary>
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
            characters[SamusDeathTileAtlasFormat.ExplosionByte(index)] = ReadByte(index);
        return new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.SamusDeathExplosion), characters);
    }
}
