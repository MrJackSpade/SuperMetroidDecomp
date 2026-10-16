namespace SuperMetroid.Core.Assets;

/// <summary>
/// Owned background characters: planar 8x8 characters of two or four bits per pixel, read by background layers in
/// place of the BG character half of VRAM.
/// </summary>
public sealed class BgArtwork
{
    /// <summary>Pixels along each side of a character.</summary>
    public const int CharacterPixels = 8;

    /// <summary>The planar character bytes.</summary>
    private readonly byte[] characters;

    /// <summary>Copies whole characters of <paramref name="bitsPerPixel"/> bits.</summary>
    /// <param name="id">The set's identity.</param>
    /// <param name="bitsPerPixel">Two or four.</param>
    /// <param name="characters">Planar characters: two interleaved bit planes per sixteen bytes.</param>
    /// <exception cref="ArgumentOutOfRangeException">The depth is not two or four bits.</exception>
    /// <exception cref="ArgumentException">The bytes are not whole characters.</exception>
    public BgArtwork(BgArtworkId id, int bitsPerPixel, ReadOnlySpan<byte> characters)
    {
        if (bitsPerPixel is not (2 or 4))
            throw new ArgumentOutOfRangeException(nameof(bitsPerPixel), bitsPerPixel, "Background characters have two or four bits per pixel.");
        int characterBytes = bitsPerPixel * CharacterPixels;
        if (characters.IsEmpty || characters.Length % characterBytes != 0)
            throw new ArgumentException($"Background artwork {id} needs whole {characterBytes}-byte characters; got {characters.Length} bytes.", nameof(characters));
        Id = id;
        BitsPerPixel = bitsPerPixel;
        this.characters = characters.ToArray();
    }

    /// <summary>The set's identity.</summary>
    public BgArtworkId Id { get; }

    /// <summary>Two or four bits per pixel.</summary>
    public int BitsPerPixel { get; }

    /// <summary>Bytes in one character.</summary>
    public int CharacterByteCount => BitsPerPixel * CharacterPixels;

    /// <summary>The number of characters.</summary>
    public int CharacterCount => characters.Length / CharacterByteCount;

    /// <summary>The planar character bytes.</summary>
    public ReadOnlySpan<byte> Characters => characters;

    /// <summary>The color index (0 transparent) of one character pixel, before any flip.</summary>
    /// <param name="character">The character.</param>
    /// <param name="pixel">The pixel within it; the leftmost pixel is bit 7 of each plane byte.</param>
    public byte ColorIndex(int character, Position16<CharacterPixels> pixel)
    {
        // Planes 0/1 interleave in bytes 0..15; planes 2/3 repeat that layout in bytes 16..31.
        int row = character * CharacterByteCount + pixel.Y * 2;
        int mask = 1 << (CharacterPixels - 1 - pixel.X);
        int color = ((characters[row] & mask) != 0 ? 1 : 0) | ((characters[row + 1] & mask) != 0 ? 2 : 0);
        if (BitsPerPixel == 4)
            color |= ((characters[row + 16] & mask) != 0 ? 4 : 0) | ((characters[row + 17] & mask) != 0 ? 8 : 0);
        return (byte)color;
    }
}
