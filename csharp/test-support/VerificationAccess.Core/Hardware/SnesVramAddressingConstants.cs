using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="SnesVramAddressing"/> that only verification reads.</summary>
internal static class SnesVramAddressingConstants
{
    /// <summary>Reads the little-endian VRAM word at a (wrapped) word address from a snapshot of VRAM bytes.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadWord(ReadOnlySpan<byte> vram, int wordAddress)
    {
        int byteOffset = SnesVramAddressingConstants.ByteOffsetOfWord(wordAddress);
        return SnesLittleEndian.Word(vram[byteOffset], vram[byteOffset + 1]);
    }
    /// <summary>Bytes in one VRAM word; the PPU addresses VRAM in 16-bit words.</summary>
    public const int BytesPerWord = 2;
    /// <summary>The byte offset of a (wrapped) word address in VRAM storage; the word's low byte comes first.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ByteOffsetOfWord(int wordAddress) => SnesVramAddressing.WrapWord(wordAddress) * BytesPerWord;
}
