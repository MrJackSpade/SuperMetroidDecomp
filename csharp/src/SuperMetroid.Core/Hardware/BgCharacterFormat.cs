using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Hardware;

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
