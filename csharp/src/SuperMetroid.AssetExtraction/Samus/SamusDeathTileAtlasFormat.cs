namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The installed death-explosion atlas: the explosion's 160 characters as five 32-character pages, each four
/// 64-pixel-wide tile rows. The file stacks the pages starting from the explosion's second page, with its first page
/// last, which is the order the extracted file was written in.
/// </summary>
public static class SamusDeathTileAtlasFormat
{
    /// <summary>
    /// The byte of the explosion's characters, in the order its spritemaps name them, that atlas byte
    /// <paramref name="atlasIndex"/> holds.
    /// </summary>
    public static int ExplosionByte(int atlasIndex)
    {
        if ((uint)atlasIndex >= TotalByteCount) throw new ArgumentOutOfRangeException(nameof(atlasIndex));
        return ExplosionPageOfAtlasPage(atlasIndex / PageByteCount) * PageByteCount + atlasIndex % PageByteCount;
    }

    /// <summary>The explosion page that atlas page <paramref name="atlasPage"/> holds: the atlas begins at the explosion's second page.</summary>
    public static int ExplosionPageOfAtlasPage(int atlasPage)
    {
        if ((uint)atlasPage >= SegmentCount) throw new ArgumentOutOfRangeException(nameof(atlasPage));
        return (atlasPage + 1) % SegmentCount;
    }

    /// <summary>Indexed PNG filename for the death-explosion characters.</summary>
    public const string ArtworkFileName = "samus-death-explosion.png";
    /// <summary>Four planar bits per pixel, allowing sixteen palette-index pens and encoding each 8-by-8 character in 32 bytes.</summary>
    public const int BitsPerPixel = 4;
    /// <summary>Bytes encoding one eight-by-eight character.</summary>
    private const int CharacterByteCount = 8 * BitsPerPixel;
    /// <summary>PNG width in pixels: eight 8-pixel characters per row.</summary>
    public const int Width = 64;
    /// <summary>The explosion's five 32-character pages.</summary>
    public const int SegmentCount = 5;
    /// <summary>Encoded bytes of one 32-character page.</summary>
    public const int PageByteCount = 32 * CharacterByteCount;
    /// <summary>Total encoded character bytes, covering all 160 characters.</summary>
    public const int TotalByteCount = SegmentCount * PageByteCount;
    /// <summary>PNG height in pixels, 160: five pages of four eight-pixel tile rows each.</summary>
    public const int Height = TotalByteCount / CharacterByteCount / (Width / 8) * 8;
}
