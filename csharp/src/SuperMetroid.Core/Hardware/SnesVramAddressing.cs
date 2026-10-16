using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Hardware;

/// <summary>VRAM address arithmetic (#223): 15-bit word addresses over 64 KiB of byte storage.</summary>
[PpuLayoutAccess]
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
