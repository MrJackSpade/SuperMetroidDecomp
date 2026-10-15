namespace SuperMetroid.Core.Assets;

/// <summary>
/// Immutable owned sprite artwork: a sheet of eight-by-eight four-bit characters that whole-frame draw entries
/// read directly, without a VRAM upload. Characters are laid out sixteen to a row, so a part larger than one
/// character reads character <c>c + row * 16 + column</c>, the same grid the SNES OBJ name table uses.
/// </summary>
public sealed class SpriteArtwork
{
    /// <summary>Encoded bytes of one four-bit planar character.</summary>
    public const int CharacterByteCount = 32;
    /// <summary>Characters per row of the sheet's grid.</summary>
    public const int CharactersPerRow = 16;
    /// <summary>Edge of one character in pixels.</summary>
    public const int CharacterPixels = 8;

    /// <summary>Owned planar character bytes.</summary>
    private readonly byte[] characters;

    /// <summary>Copies the planar characters so later edits to the source cannot change published frames.</summary>
    /// <param name="id">The artwork's owned identity.</param>
    /// <param name="characters">SNES four-bit planar characters, 32 bytes each.</param>
    /// <exception cref="ArgumentException">The bytes are empty or not a whole number of characters.</exception>
    public SpriteArtwork(SpriteArtworkId id, ReadOnlySpan<byte> characters)
    {
        if (characters.IsEmpty || characters.Length % CharacterByteCount != 0)
            throw new ArgumentException($"Sprite artwork {id} needs whole {CharacterByteCount}-byte characters; got {characters.Length} bytes.", nameof(characters));
        Id = id;
        this.characters = characters.ToArray();
    }

    /// <summary>The artwork's owned identity.</summary>
    public SpriteArtworkId Id { get; }

    /// <summary>Number of characters in the sheet.</summary>
    public int CharacterCount => characters.Length / CharacterByteCount;

    /// <summary>The planar character bytes in sheet order.</summary>
    public ReadOnlySpan<byte> Characters => characters;

    /// <summary>Palette index (zero is transparent) of one pixel of one character.</summary>
    /// <param name="character">Character index within the sheet.</param>
    /// <param name="pixel">Pixel inside the character, each coordinate 0..7 from the top-left.</param>
    public byte ColorIndex(int character, Position16<CharacterPixels> pixel)
    {
        // Planes 0/1 interleave in bytes 0..15 and planes 2/3 repeat that layout in bytes 16..31;
        // the leftmost pixel is bit 7.
        int row = character * CharacterByteCount + pixel.Y * 2;
        int mask = 1 << (7 - pixel.X);
        return (byte)(
            ((characters[row] & mask) != 0 ? 1 : 0)
            | ((characters[row + 1] & mask) != 0 ? 2 : 0)
            | ((characters[row + 16] & mask) != 0 ? 4 : 0)
            | ((characters[row + 17] & mask) != 0 ? 8 : 0));
    }
}
