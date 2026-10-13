namespace SuperMetroid.Core.Hardware;

/// <summary>How VMADD advances after each word of a VRAM port transfer (VMAIN bit 0-1).</summary>
public enum VramIncrement
{
    /// <summary>VMAIN=$80: one word, along a tilemap row or through consecutive character data.</summary>
    Row,

    /// <summary>VMAIN=$81: 32 words, down one 32-wide tilemap column.</summary>
    Column,
}

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
    private readonly SnesVram vram;
    /// <summary>First physical VMADD word in the region.</summary>
    private readonly int first;

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

    /// <summary>Copies another region's bytes in, starting <paramref name="byteOffset"/> bytes into this region.</summary>
    public void Load(VramRegion source, int byteOffset = 0)
    {
        CheckBytes(byteOffset, source.ByteCount);
        vram.LoadBytes(first * sizeof(ushort) + byteOffset, source.vram.Bytes.Slice(source.first * sizeof(ushort), source.ByteCount));
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

    /// <summary>The word at <paramref name="wordOffset"/>.</summary>
    public ushort ReadWord(int wordOffset)
    {
        CheckWords(wordOffset, 1);
        return vram.ReadWord(first + wordOffset);
    }

    /// <summary>The VMADD step for <paramref name="increment"/>.</summary>
    internal static int Step(VramIncrement increment) => increment switch
    {
        VramIncrement.Row => 1,
        VramIncrement.Column => 32,
        _ => throw new ArgumentOutOfRangeException(nameof(increment), increment, "Undefined VRAM increment."),
    };

    /// <summary>Rejects a word span that crosses this region's physical allocation.</summary>
    private void CheckWords(int wordOffset, int wordCount)
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

/// <summary>
/// Where a queued VRAM DMA lands (#630): a VMADD word and the VMAIN increment. Super Metroid's
/// queue stores both in one word, bit 15 selecting the 32-word column increment; that encoding
/// stays inside this type, the queue and the PPU backend.
/// </summary>
[PpuLayoutBoundary("codec: queued VRAM DMA destination word")]
public readonly record struct VramDmaDestination
{
    /// <summary>Native DMA queue bit selecting 32-word column increments.</summary>
    private const ushort ColumnFlag = 0x8000;
    /// <summary>Native fifteen-bit VMADD address field.</summary>
    private const ushort WordMask = 0x7fff;

    /// <summary>Preserves an already encoded native DMA destination word.</summary>
    private VramDmaDestination(ushort encoded) => Encoded = encoded;

    /// <summary>The native queue word: VMADD in bits 0-14, bit 15 set for the column increment.</summary>
    [PpuLayoutAccess]
    public ushort Encoded { get; }

    /// <summary>The destination <paramref name="wordOffset"/> words into the VRAM described by <paramref name="cell"/>.</summary>
    [PpuLayoutAccess]
    public static VramDmaDestination At(PpuLayoutCell cell, int wordOffset = 0, VramIncrement increment = VramIncrement.Row)
    {
        Ensure.NotNull(cell);
        if (cell.Region != PpuLayoutRegion.Vram)
            throw new ArgumentException($"{cell} is not a VRAM region.", nameof(cell));
        if ((uint)wordOffset >= (uint)cell.Count)
            throw new ArgumentOutOfRangeException(nameof(wordOffset), wordOffset, $"{cell.Name} holds {cell.Count} words.");
        return At(cell.Offset + wordOffset, increment);
    }

    /// <summary>Decodes a native queue word read from cartridge or WRAM data.</summary>
    [PpuLayoutAccess]
    public static VramDmaDestination FromNative(ushort encoded) => new(encoded);

    /// <summary>The destination at physical VMADD <paramref name="word"/>.</summary>
    internal static VramDmaDestination At(int word, VramIncrement increment) =>
        (uint)word <= WordMask
            ? new((ushort)(word | (increment == VramIncrement.Column ? ColumnFlag : 0)))
            : throw new ArgumentOutOfRangeException(nameof(word), word, "VMADD holds 15 bits.");
}
