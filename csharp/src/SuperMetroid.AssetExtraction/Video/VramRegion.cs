using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>
/// A bounded, typed view of consecutive VRAM words (#630): a tilemap, a character block or
/// another named allocation from a layout catalog. Callers address it by word or byte offset
/// within the region and exchange tilemap words, character bytes or port word streams; the
/// VMADD base and the byte-address arithmetic stay behind the view. A struct over the backend,
/// so uploads and reads allocate nothing.
/// </summary>
internal readonly struct VramRegion
{
    /// <summary>Backend VRAM whose words are exposed through this bounded view.</summary>
    internal readonly SnesVram vram;
    /// <summary>First physical VMADD word in the region.</summary>
    internal readonly int first;

    /// <summary>Binds the VRAM words described by <paramref name="cell"/>.</summary>
    public VramRegion(SnesVram vram, PpuLayoutCell cell)
        : this(Ensure.NotNull(vram), Ensure.NotNull(cell).Region == PpuLayoutRegion.Vram
            ? cell.Offset
            : throw new ArgumentException($"{cell} is not a VRAM region.", nameof(cell)), cell.Count)
    {
    }

    /// <summary>Creates a subview using an already validated physical word range.</summary>
    internal VramRegion(SnesVram vram, int first, int wordCount)
    {
        this.vram = vram;
        this.first = first;
        WordCount = wordCount;
    }

    /// <summary>The region's size in words.</summary>
    public int WordCount { get; }

    /// <summary>The region's size in bytes.</summary>
    public int ByteCount => WordCount * sizeof(ushort);

    /// <summary>Copies consecutive bytes in, as the VRAM-targeting decompressor or a byte DMA does.</summary>
    public void Load(ReadOnlySpan<byte> bytes, int byteOffset = 0)
    {
        CheckBytes(byteOffset, bytes.Length);
        vram.LoadBytes(first * sizeof(ushort) + byteOffset, bytes);
    }

    /// <summary>
    /// Writes a word stream starting <paramref name="wordOffset"/> words in, advancing <paramref name="wordStep"/> words
    /// after each word: one along a row, a row's width down a column.
    /// </summary>
    public void Write(ReadOnlySpan<ushort> words, int wordOffset = 0, int wordStep = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(wordStep);
        if (words.Length > 0)
            CheckWords(wordOffset, (words.Length - 1) * wordStep + 1);
        vram.ExecuteWordTransfer(words, (ushort)(first + wordOffset), wordStep);
    }

    /// <summary>Rejects a word span that crosses this region's physical allocation.</summary>
    internal void CheckWords(int wordOffset, int wordCount)
    {
        if (wordOffset < 0 || wordCount < 0 || wordOffset + wordCount > WordCount)
            throw new ArgumentOutOfRangeException(nameof(wordOffset), wordOffset,
                $"Words {wordOffset}+{wordCount} exceed the {WordCount}-word VRAM region at ${first:X4}.");
    }

    /// <summary>Rejects a byte span that crosses this region's physical allocation.</summary>
    private void CheckBytes(int byteOffset, int byteCount)
    {
        if (byteOffset < 0 || byteCount < 0 || byteOffset + byteCount > ByteCount)
            throw new ArgumentOutOfRangeException(nameof(byteOffset), byteOffset,
                $"Bytes {byteOffset}+{byteCount} exceed the {ByteCount}-byte VRAM region at ${first:X4}.");
    }
}
