using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Lossless view of the five-byte spritemap record's encoded X word. Its low nine bits are
/// the modular X offset and bit fifteen independently selects the large OBJ size.
/// </summary>
/// <param name="Raw">The complete native X word, including any unused upper bits.</param>
[EncodingOwner("spritemap part X word")]
public readonly record struct SnesSpritemapXWord(ushort Raw)
{
    /// <summary>The low nine bits encode the sprite's modular horizontal displacement.</summary>
    internal const ushort OffsetMask = 0x01ff;
    /// <summary>The top bit carries the spritemap's large-object selector.</summary>
    private const ushort LargeObjectMask = 0x8000;

    /// <summary>Gets whether bit 15 selects the larger of the PPU's configured OBJ sizes.</summary>
    public bool IsLarge => (Raw & LargeObjectMask) != 0;
    /// <summary>Signed nine-bit visual displacement; unrelated upper bits never reach OAM X.</summary>
    public int SignedOffset => unchecked((short)((Raw & OffsetMask) << 7)) >> 7;
    /// <summary>Encodes a signed nine-bit displacement and size selector, clearing unused upper bits.</summary>
    /// <param name="offset">The horizontal displacement from the sprite origin, from -256 through 255 pixels.</param>
    /// <param name="large">Whether the sprite selects the larger configured OBJ size.</param>
    /// <exception cref="ArgumentOutOfRangeException">The displacement does not fit a signed nine-bit value.</exception>
    public static SnesSpritemapXWord Create(int offset, bool large)
    {
        if (offset is < -256 or > 255) throw new ArgumentOutOfRangeException(nameof(offset));
        return new((ushort)((offset & OffsetMask) | (large ? LargeObjectMask : 0)));
    }
}
