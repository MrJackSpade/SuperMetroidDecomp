namespace SuperMetroid.Core.Hardware;

/// <summary>
/// A bounded, typed view of consecutive CGRAM colours (#630). Callers address colours by their
/// index within the named range and exchange <see cref="RgbColor"/> values; the CGRAM slot, the
/// native word encoding and bit-15 masking stay behind the view. A struct over the backend, so
/// palette fades and render lookups allocate nothing.
/// </summary>
[PpuLayoutBoundary("rendering view: a bounded CGRAM colour range")]
public readonly struct CgramPalette
{
    /// <summary>Underlying colour table to which writes must propagate: CGRAM or a screen palette.</summary>
    internal readonly IPaletteColors cgram;
    /// <summary>First CGRAM color slot represented by this bounded view.</summary>
    internal readonly int first;

    /// <summary>Binds the CGRAM colours described by <paramref name="cell"/>.</summary>
    [PpuLayoutAccess]
    public CgramPalette(IPaletteColors cgram, PpuLayoutCell cell)
        : this(Ensure.NotNull(cgram), Ensure.NotNull(cell).Region == PpuLayoutRegion.Cgram
            ? cell.Offset
            : throw new ArgumentException($"{cell} is not a CGRAM range.", nameof(cell)), cell.Count)
    {
    }

    /// <summary>Creates a validated subview using a parent range's CGRAM offset and color count.</summary>
    private CgramPalette(IPaletteColors cgram, int first, int count)
    {
        this.cgram = cgram;
        this.first = first;
        Count = count;
    }

    /// <summary>The number of colours in the range.</summary>
    public int Count { get; }

    /// <summary>The range's colours, in CGRAM order.</summary>
    public ReadOnlySpan<RgbColor> Colors => cgram.Colors.Slice(first, Count);

    /// <summary>Writes one colour.</summary>
    public void Write(int index, RgbColor color) => cgram.SetColor(first + Index(index), color);

    /// <summary>Writes <paramref name="colors"/> starting at <paramref name="start"/>.</summary>
    public void CopyFrom(ReadOnlySpan<RgbColor> colors, int start = 0)
    {
        CheckSpan(start, colors.Length);
        for (int index = 0; index < colors.Length; index++)
            cgram.SetColor(first + start + index, colors[index]);
    }

    /// <summary>
    /// Streams little-endian colour words through the CGDATA port ($2122) starting at
    /// <paramref name="start"/>; bit 15 of each word is not stored, as on hardware.
    /// </summary>
    public void LoadPortBytes(ReadOnlySpan<byte> littleEndianWords, int start = 0)
    {
        if ((littleEndianWords.Length & 1) != 0)
            throw new ArgumentException("CGRAM data must contain complete two-byte colours.", nameof(littleEndianWords));
        CheckSpan(start, littleEndianWords.Length / RgbColor.ByteCount);
        cgram.LoadBytes(littleEndianWords, first + start);
    }

    /// <summary>A sub-range of <paramref name="count"/> colours starting at <paramref name="start"/>.</summary>
    public CgramPalette Slice(int start, int count)
    {
        CheckSpan(start, count);
        return new(cgram, first + start, count);
    }

    /// <summary>Rejects a color index outside this view before adding its CGRAM base offset.</summary>
    internal int Index(int index) => (uint)index < (uint)Count
        ? index
        : throw new ArgumentOutOfRangeException(nameof(index), index, $"The palette range holds {Count} colours.");

    /// <summary>Guards a requested color span against the bounds of this palette view.</summary>
    private void CheckSpan(int start, int count)
    {
        if (start < 0 || count < 0 || start + count > Count)
            throw new ArgumentOutOfRangeException(nameof(start), start, $"Colours {start}+{count} exceed the {Count}-colour range.");
    }
}
