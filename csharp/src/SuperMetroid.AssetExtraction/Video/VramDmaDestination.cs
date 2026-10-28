using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// Where a queued VRAM DMA lands (#630): a VMADD word and the VMAIN increment. Super Metroid's
/// queue stores both in one word, bit 15 selecting the 32-word column increment; that encoding
/// stays inside this type, the queue and the PPU backend.
/// </summary>
/// <remarks>
/// This type is the codec of that word: <c>At(PpuLayoutCell, int, VramIncrement)</c> encodes it, and
/// <see cref="Word"/> and <see cref="WordIncrement"/> are the only decoders, so the word it stores is the encoding.
/// </remarks>
[EncodingOwner("queued video transfer destination word")]
public readonly record struct VramDmaDestination
{
    /// <summary>Native DMA queue bit 15, selecting 32-word column increments.</summary>
    internal const ushort ColumnFlag = 1 << 15;
    /// <summary>Native fifteen-bit VMADD address field.</summary>
    internal const ushort WordMask = SnesPpuLayout.VramWordCount - 1;
    /// <summary>The words a column increment advances.</summary>
    private const int ColumnIncrementWords = 32;

    /// <summary>Preserves an already encoded native DMA destination word.</summary>
    internal VramDmaDestination(ushort encoded) => Encoded = encoded;

    /// <summary>The native queue word: VMADD in bits 0-14, bit 15 set for the column increment.</summary>
    internal ushort Encoded { get; }

    /// <summary>The VMADD word the transfer starts at.</summary>
    internal int Word => Encoded & WordMask;

    /// <summary>The words VMADD advances after each high-byte write: one for a row, 32 for a column.</summary>
    internal int WordIncrement => (Encoded & ColumnFlag) == 0 ? 1 : ColumnIncrementWords;

    /// <summary>Decodes a native queue word read from cartridge or WRAM data.</summary>
    internal static VramDmaDestination FromNative(ushort encoded) => new(encoded);
}
