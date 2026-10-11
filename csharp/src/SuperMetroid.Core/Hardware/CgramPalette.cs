using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Hardware;

/// <summary>
/// A bounded, typed view of consecutive CGRAM colours (#630). Callers address colours by their
/// index within the named range and exchange <see cref="Bgr555"/> values; the CGRAM slot, the
/// native word encoding and bit-15 masking stay behind the view. A struct over the backend, so
/// palette fades and render lookups allocate nothing.
/// </summary>
[PpuLayoutBoundary("rendering view: a bounded CGRAM colour range")]
public readonly struct CgramPalette
{
    private readonly SnesCgram cgram;
    private readonly int first;

    /// <summary>Binds the CGRAM colours described by <paramref name="cell"/>.</summary>
    [PpuLayoutAccess]
    public CgramPalette(SnesCgram cgram, PpuLayoutCell cell)
        : this(Ensure.NotNull(cgram), Ensure.NotNull(cell).Region == PpuLayoutRegion.Cgram
            ? cell.Offset
            : throw new ArgumentException($"{cell} is not a CGRAM range.", nameof(cell)), cell.Count)
    {
    }

    private CgramPalette(SnesCgram cgram, int first, int count)
    {
        this.cgram = cgram;
        this.first = first;
        Count = count;
    }

    /// <summary>The number of colours in the range.</summary>
    public int Count { get; }

    /// <summary>The colour at <paramref name="index"/> within the range.</summary>
    public Bgr555 this[int index] => cgram.Colors[first + Index(index)];

    /// <summary>The range's colours, in CGRAM order.</summary>
    public ReadOnlySpan<Bgr555> Colors => cgram.Colors.Slice(first, Count);

    /// <summary>Writes one colour.</summary>
    public void Write(int index, Bgr555 color) => cgram.SetColor(first + Index(index), color);

    /// <summary>The colour at <paramref name="index"/> expanded to 8-bit RGBA.</summary>
    public Rgba32 Rgba(int index) => cgram.GetRgba(first + Index(index));

    /// <summary>Writes <paramref name="colors"/> starting at <paramref name="start"/>.</summary>
    public void CopyFrom(ReadOnlySpan<Bgr555> colors, int start = 0)
    {
        CheckSpan(start, colors.Length);
        for (int index = 0; index < colors.Length; index++)
            cgram.SetColor(first + start + index, colors[index]);
    }

    /// <summary>Copies the range's colours into <paramref name="destination"/>, which must hold exactly <see cref="Count"/>.</summary>
    public void CopyTo(Span<Bgr555> destination)
    {
        if (destination.Length != Count)
            throw new ArgumentException($"The destination holds {destination.Length} colours; the range has {Count}.", nameof(destination));
        Colors.CopyTo(destination);
    }

    /// <summary>Writes <paramref name="color"/> to every colour in the range.</summary>
    public void Fill(Bgr555 color)
    {
        for (int index = 0; index < Count; index++)
            cgram.SetColor(first + index, color);
    }

    /// <summary>
    /// Streams little-endian colour words through the CGDATA port ($2122) starting at
    /// <paramref name="start"/>; bit 15 of each word is not stored, as on hardware.
    /// </summary>
    public void LoadPortBytes(ReadOnlySpan<byte> littleEndianWords, int start = 0)
    {
        if ((littleEndianWords.Length & 1) != 0)
            throw new ArgumentException("CGRAM data must contain complete two-byte colours.", nameof(littleEndianWords));
        CheckSpan(start, littleEndianWords.Length / Bgr555.ByteCount);
        cgram.LoadBytes(littleEndianWords, first + start);
    }

    /// <summary>A sub-range of <paramref name="count"/> colours starting at <paramref name="start"/>.</summary>
    public CgramPalette Slice(int start, int count)
    {
        CheckSpan(start, count);
        return new(cgram, first + start, count);
    }

    private int Index(int index) => (uint)index < (uint)Count
        ? index
        : throw new ArgumentOutOfRangeException(nameof(index), index, $"The palette range holds {Count} colours.");

    private void CheckSpan(int start, int count)
    {
        if (start < 0 || count < 0 || start + count > Count)
            throw new ArgumentOutOfRangeException(nameof(start), start, $"Colours {start}+{count} exceed the {Count}-colour range.");
    }
}

/// <summary>
/// Hardware-level palette views of a whole CGRAM (#630): the complete colour table, the
/// background and OBJ banks and their 16-colour rows. Domain code reaches its own named ranges
/// through its catalog's view instead.
/// </summary>
[PpuLayoutBoundary("rendering view: whole-CGRAM banks and rows")]
public readonly struct CgramPalettes(SnesCgram cgram)
{
    /// <summary>Colours in a complete CGRAM image.</summary>
    public const int ColorCount = SnesPpuLayout.CgramColorCount;

    /// <summary>Bytes in a complete CGRAM image of little-endian colour words.</summary>
    public const int ByteCount = SnesPpuLayout.CgramByteCount;

    /// <summary>All 256 colours.</summary>
    public CgramPalette All => new(cgram, PpuMemoryLayout.Cgram);

    /// <summary>The 128 background colours.</summary>
    public CgramPalette Background => new(cgram, PpuMemoryLayout.BackgroundPalettes);

    /// <summary>The 128 OBJ colours.</summary>
    public CgramPalette Objects => new(cgram, PpuMemoryLayout.ObjectPalettes);

    /// <summary>The backdrop: background palette 0, colour 0.</summary>
    public Bgr555 Backdrop => All[0];

    /// <summary>Background palette row <paramref name="row"/> (0-7).</summary>
    public CgramPalette BackgroundRow(int row) => Background.Slice(RowStart(row), PpuMemoryLayout.ColorsPerRow);

    /// <summary>OBJ palette row <paramref name="row"/> (0-7).</summary>
    public CgramPalette ObjectRow(int row) => Objects.Slice(RowStart(row), PpuMemoryLayout.ColorsPerRow);

    private static int RowStart(int row) => (uint)row < PpuMemoryLayout.PaletteRowCount
        ? row * PpuMemoryLayout.ColorsPerRow
        : throw new ArgumentOutOfRangeException(nameof(row), row, "A palette bank has eight rows.");
}
