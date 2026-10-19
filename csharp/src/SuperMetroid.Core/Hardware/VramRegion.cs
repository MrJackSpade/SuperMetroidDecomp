namespace SuperMetroid.Core.Hardware;

/// <summary>
/// A bounded, typed view of consecutive VRAM words (#630): a tilemap, a character block or
/// another named allocation from a layout catalog. Callers address it by word or byte offset
/// within the region and exchange tilemap words, character bytes or port word streams; the
/// VMADD base and the byte-address arithmetic stay behind the view. A struct over the backend,
/// so uploads and reads allocate nothing.
/// </summary>
[PpuLayoutBoundary("rendering view: a bounded VRAM word range")]
public readonly struct VramRegion
{
    /// <summary>Backend VRAM whose words are exposed through this bounded view.</summary>
    internal readonly SnesVram vram;
    /// <summary>First physical VMADD word in the region.</summary>
    internal readonly int first;

    /// <summary>Binds the VRAM words described by <paramref name="cell"/>.</summary>
    [PpuLayoutAccess]
    public VramRegion(SnesVram vram, PpuLayoutCell cell)
        : this(Ensure.NotNull(vram), Ensure.NotNull(cell).Region == PpuLayoutRegion.Vram
            ? cell.Offset
            : throw new ArgumentException($"{cell} is not a VRAM region.", nameof(cell)), cell.Count)
    {
    }

    /// <summary>Creates a subview using an already validated physical word range.</summary>
    private VramRegion(SnesVram vram, int first, int wordCount)
    {
        this.vram = vram;
        this.first = first;
        WordCount = wordCount;
    }

    /// <summary>The region's size in words.</summary>
    public int WordCount { get; }

    /// <summary>The region's size in bytes.</summary>
    public int ByteCount => WordCount * sizeof(ushort);

    /// <summary>A sub-region of <paramref name="wordCount"/> words starting <paramref name="wordOffset"/> words in.</summary>
    public VramRegion Slice(int wordOffset, int wordCount)
    {
        CheckWords(wordOffset, wordCount);
        return new(vram, first + wordOffset, wordCount);
    }

    /// <summary>Copies consecutive bytes in, as the VRAM-targeting decompressor or a byte DMA does.</summary>
    public void Load(ReadOnlySpan<byte> bytes, int byteOffset = 0)
    {
        CheckBytes(byteOffset, bytes.Length);
        vram.LoadBytes(first * sizeof(ushort) + byteOffset, bytes);
    }

    /// <summary>
    /// Writes a word stream through the VMDATA port starting <paramref name="wordOffset"/> words in,
    /// advancing VMADD by <paramref name="increment"/> after each word.
    /// </summary>
    public void Write(ReadOnlySpan<ushort> words, int wordOffset = 0, VramIncrement increment = VramIncrement.Row)
    {
        int step = Step(increment);
        if (words.Length > 0)
            CheckWords(wordOffset, (words.Length - 1) * step + 1);
        vram.ExecuteWordTransfer(words, (ushort)(first + wordOffset), step);
    }

    /// <summary>Writes one word at <paramref name="wordOffset"/>.</summary>
    public void Write(int wordOffset, ushort word)
    {
        ReadOnlySpan<ushort> words = [word];
        Write(words, wordOffset);
    }

    /// <summary>The VMADD step for <paramref name="increment"/>.</summary>
    internal static int Step(VramIncrement increment) => increment switch
    {
        VramIncrement.Row => 1,
        VramIncrement.Column => 32,
        _ => throw new ArgumentOutOfRangeException(nameof(increment), increment, "Undefined VRAM increment."),
    };

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
