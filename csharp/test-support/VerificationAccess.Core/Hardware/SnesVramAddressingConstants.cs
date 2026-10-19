using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Hardware;

/// <summary>Cartridge values and helpers of <see cref="SnesVramAddressing"/> that only verification reads.</summary>
internal static class SnesVramAddressingConstants
{
    /// <summary>Reads the little-endian VRAM word at a (wrapped) word address from a snapshot of VRAM bytes.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadWord(ReadOnlySpan<byte> vram, int wordAddress)
    {
        int byteOffset = SnesVramAddressing.ByteOffsetOfWord(wordAddress);
        return SnesLittleEndian.Word(vram[byteOffset], vram[byteOffset + 1]);
    }
}
