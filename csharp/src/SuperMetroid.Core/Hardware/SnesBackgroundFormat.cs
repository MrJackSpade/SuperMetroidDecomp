using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Hardware;

/// <summary>VRAM address arithmetic (#223): 15-bit word addresses over 64 KiB of byte storage.</summary>
[PpuLayoutAccess]
[SnesEncodingOwner]
public static class SnesVramAddressing
{
    /// <summary>Bytes in one VRAM word; the PPU addresses VRAM in 16-bit words.</summary>
    public const int BytesPerWord = 2;

    /// <summary>Wraps a word address onto the physical 15-bit VMADD bus, as the PPU does.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WrapWord(int wordAddress) => wordAddress & (SnesPpuLayout.VramWordCount - 1);

    /// <summary>Reads the little-endian VRAM word at a (wrapped) word address from a snapshot of VRAM bytes.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadWord(ReadOnlySpan<byte> vram, int wordAddress)
    {
        int byteOffset = ByteOffsetOfWord(wordAddress);
        return SnesLittleEndian.Word(vram[byteOffset], vram[byteOffset + 1]);
    }

    /// <summary>The byte offset of a (wrapped) word address in VRAM storage; the word's low byte comes first.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ByteOffsetOfWord(int wordAddress) => WrapWord(wordAddress) * BytesPerWord;
}

/// <summary>Converts background pixel coordinates to the 8×8 characters that cover them (#223).</summary>
[SnesEncodingOwner]
public static class BgCharacterGrid
{
    /// <summary>The character (tile) column or row containing a non-negative pixel coordinate.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CharacterOf(int pixel) => pixel / SnesPpuLayout.BackgroundTileSizePixels;

    /// <summary>The pixel's position within its character, zero through seven.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PixelWithinCharacter(int pixel) => pixel % SnesPpuLayout.BackgroundTileSizePixels;

    /// <summary>The source row or column of a character pixel, mirrored when the entry flips that axis.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SourcePixel(int pixel, bool flipped) =>
        flipped ? SnesPpuLayout.BackgroundTileSizePixels - 1 - pixel : pixel;
}

/// <summary>
/// A background tilemap's geometry as BGnSC selects it (#223): one, two (horizontal or vertical) or four
/// 32×32-character screens. Each screen is a contiguous <see cref="SnesPpuLayout.TilemapPageWordCount"/>-word
/// block stored row by row; screens are ordered left to right, then top to bottom.
/// </summary>
[PpuLayoutAccess]
[SnesEncodingOwner]
public readonly record struct BgTilemapGeometry
{
    /// <summary>Validates a tilemap of 32 or 64 characters on each axis.</summary>
    public BgTilemapGeometry(Extent16<TilemapCells> sizeInCharacters)
    {
        int widthInCharacters = sizeInCharacters.X, heightInCharacters = sizeInCharacters.Y;
        if (widthInCharacters is not (SnesPpuLayout.TilemapPageWidthInTiles or 2 * SnesPpuLayout.TilemapPageWidthInTiles))
            throw new ArgumentOutOfRangeException(nameof(widthInCharacters), widthInCharacters, "A BGSC tilemap is one or two screens wide.");
        if (heightInCharacters is not (SnesPpuLayout.TilemapPageWidthInTiles or 2 * SnesPpuLayout.TilemapPageWidthInTiles))
            throw new ArgumentOutOfRangeException(nameof(heightInCharacters), heightInCharacters, "A BGSC tilemap is one or two screens tall.");
        SizeInCharacters = sizeInCharacters;
    }

    /// <summary>Characters across and down the whole map.</summary>
    public Extent16<TilemapCells> SizeInCharacters { get; }

    /// <summary>Wraps a scrolled horizontal pixel into the map; scrolling past either edge repeats the map.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WrapX(int pixelX) => pixelX & (SizeInCharacters.X * SnesPpuLayout.BackgroundTileSizePixels - 1);

    /// <summary>Wraps a scrolled vertical pixel into the map.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int WrapY(int pixelY) => pixelY & (SizeInCharacters.Y * SnesPpuLayout.BackgroundTileSizePixels - 1);

    /// <summary>
    /// The VRAM word holding the entry for a character position: the screen containing it, then the
    /// entry's row and column within that screen, wrapped onto the VRAM bus.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int EntryWordAddress(int tilemapBaseWord, Position16<TilemapCells> character)
    {
        const int screenSize = SnesPpuLayout.TilemapPageWidthInTiles;
        int characterX = character.X, characterY = character.Y;
        int screensPerRow = SizeInCharacters.X / screenSize;
        int screen = characterY / screenSize * screensPerRow + characterX / screenSize;
        int withinScreen = characterY % screenSize * screenSize + characterX % screenSize;
        return SnesVramAddressing.WrapWord(tilemapBaseWord + screen * SnesPpuLayout.TilemapPageWordCount + withinScreen);
    }
}

/// <summary>
/// A planar background character format (#223). A character stores its rows as plane pairs: each row's
/// planes 0 and 1 occupy one word, the eight rows of that pair are contiguous, and the next pair (planes 2
/// and 3 at four bits per pixel) follows. A pixel's color index takes one bit from each plane, the
/// leftmost pixel in each byte's most significant bit, plane <c>n</c> contributing <c>2^n</c>.
/// </summary>
[PpuLayoutAccess]
[SnesEncodingOwner]
public sealed class BgCharacterFormat
{
    /// <summary>The color index every background treats as transparent.</summary>
    public const int TransparentColorIndex = 0;

    /// <summary>Palettes a tilemap entry's three-bit palette field can select.</summary>
    public const int PaletteCount = 8;

    /// <summary>Bit planes interleaved in one row word: the pair's even plane in the low byte, its odd plane in the high byte.</summary>
    public const int PlanesPerPair = 2;

    /// <summary>Bytes one character row of a plane pair occupies: one byte per plane.</summary>
    public const int BytesPerPlanePairRow = PlanesPerPair;

    /// <summary>Bytes of one plane pair across the character's eight rows; the next pair starts this far on.</summary>
    public const int BytesPerPlanePair = SnesPpuLayout.BackgroundTileSizePixels * BytesPerPlanePairRow;

    /// <summary>Captures the native two- or four-plane character depth used for palette and row strides.</summary>
    private BgCharacterFormat(int bitsPerPixel) => BitsPerPixel = bitsPerPixel;

    /// <summary>Two bits per pixel: four colors per palette (BG3 in mode 1).</summary>
    public static BgCharacterFormat TwoBitsPerPixel { get; } = new(2);

    /// <summary>Four bits per pixel: sixteen colors per palette (BG1 and BG2 in mode 1).</summary>
    public static BgCharacterFormat FourBitsPerPixel { get; } = new(FourBitsPerPixelDepth);

    /// <summary>Four planes occupy two eight-row plane pairs for mode-1 BG1/BG2 characters.</summary>
    private const int FourBitsPerPixelDepth = 4;

    /// <summary>Bit planes per pixel.</summary>
    public int BitsPerPixel { get; }

    /// <summary>Colors in one palette of this format, which is also the palette stride in CGRAM.</summary>
    public int ColorsPerPalette => 1 << BitsPerPixel;

    /// <summary>VRAM words one character occupies.</summary>
    public int WordsPerCharacter => BitsPerPixel / PlanesPerPair * BytesPerPlanePair / SnesVramAddressing.BytesPerWord;

    /// <summary>The VRAM byte offset of a character row's first plane pair.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int RowByteOffset(int characterBaseWord, int character, int row) =>
        SnesVramAddressing.ByteOffsetOfWord(characterBaseWord + character * WordsPerCharacter) + row * BytesPerPlanePairRow;

    /// <summary>The color index of one pixel of a character row, read from every bit plane.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ColorIndexAt(ReadOnlySpan<byte> vram, int rowByteOffset, int column)
    {
        // The leftmost pixel is each plane byte's most significant bit.
        int bit = SnesPpuLayout.BackgroundTileSizePixels - 1 - column;
        int color = (vram[rowByteOffset] >> bit & 1) | (vram[rowByteOffset + 1] >> bit & 1) << 1;
        if (BitsPerPixel == FourBitsPerPixelDepth)
        {
            int upperPair = rowByteOffset + BytesPerPlanePair;
            color |= (vram[upperPair] >> bit & 1) << 2 | (vram[upperPair + 1] >> bit & 1) << 3;
        }
        return color;
    }

    /// <summary>The CGRAM color of a palette's color index.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CgramColor(int palette, int colorIndex) => palette * ColorsPerPalette + colorIndex;
}
