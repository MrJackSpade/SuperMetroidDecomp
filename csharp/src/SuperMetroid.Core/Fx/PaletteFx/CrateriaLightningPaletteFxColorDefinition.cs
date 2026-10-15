namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Calculated setup, two repeated flash groups and neutral intervals for one lightning owner.</summary>
public sealed class CrateriaLightningPaletteFxColorDefinition
{
    /// <summary>Calculates fixed layout addresses and timed records for the selected lightning owner.</summary>
    internal CrateriaLightningPaletteFxColorDefinition(CrateriaLightningPaletteOwner owner)
    {
        Owner = owner;
        Frames = new CalculatedList<CrateriaLightningPaletteFrame>(12 + NeutralCount, Frame);
    }

    /// <summary>Program identity selecting either the live $F765 surface lightning or retained unused $F769 dark-lightning mechanics and color resource.</summary>
    public CrateriaLightningPaletteOwner Owner { get; }
    /// <summary>Determines which cartridge layout, CGRAM destination, and record width apply.</summary>
    internal bool IsSurface => Owner == CrateriaLightningPaletteOwner.SurfaceLightning;
    /// <summary>$8D:EB3B/$EC6E setup starts: pre-instruction and CGRAM destination.</summary>
    public ushort ProgramStart => IsSurface ? (ushort)0xeb3b : (ushort)0xec6e;
    /// <summary>$8D:EB43/$EC76 first neutral frame follows the eight-byte setup.</summary>
    public ushort FirstFramePointer => (ushort)(ProgramStart + 8);
    /// <summary>Live BGR555 words per timed record: eight targeting CGRAM colors 84..91 for surface lightning, or seven targeting colors 65..71 for dark lightning.</summary>
    public int ColorsPerFrame => IsSurface ? 8 : 7;
    /// <summary>Thirteen surface or fourteen dark timed records in program address order, not expanded loop-execution order; dark lightning retains an additional 240-update neutral record.</summary>
    public IReadOnlyList<CrateriaLightningPaletteFrame> Frames { get; }

    /// <summary>The dark program inserts a second 240-update neutral record before its last flash group.</summary>
    internal int NeutralCount => IsSurface ? 1 : 2;
    /// <summary>Each timed record holds a duration, live colors, and a terminal wait word.</summary>
    private int FrameByteCount => 2 * (ColorsPerFrame + 2);
    /// <summary>The first loop's byte-sized timer instruction follows the opening neutral record.</summary>
    internal ushort TimerTwoPointer => (ushort)(FirstFramePointer + FrameByteCount);
    /// <summary>The first of seven timed flashes repeated by the two-count loop.</summary>
    private ushort RepeatedFramesPointer => (ushort)(TimerTwoPointer + 3);
    /// <summary>The branch that returns to the seven-flash group while its timer remains active.</summary>
    private ushort DecrementTwoPointer => (ushort)(RepeatedFramesPointer + 7 * FrameByteCount);
    /// <summary>Address of the neutral interval after the repeated flashes.</summary>
    private ushort NeutralFramesPointer => (ushort)(DecrementTwoPointer + 4);
    /// <summary>The final flash group's one-count timer follows the neutral interval.</summary>
    internal ushort TimerOnePointer => (ushort)(NeutralFramesPointer + NeutralCount * FrameByteCount);
    /// <summary>The first of four timed flashes in the final group.</summary>
    private ushort FinalFramesPointer => (ushort)(TimerOnePointer + 3);

    /// <summary>Calculates a neutral or flash record's native address, duration, and live color count.</summary>
    private CrateriaLightningPaletteFrame Frame(int index)
    {
        if (index == 0)
            return new(FirstFramePointer, ColorsPerFrame);
        if (index <= 7)
            return new((ushort)(RepeatedFramesPointer + (index - 1) * FrameByteCount), ColorsPerFrame);
        if (index < 8 + NeutralCount)
            return new((ushort)(NeutralFramesPointer + (index - 8) * FrameByteCount), ColorsPerFrame);
        int final = index - 8 - NeutralCount;
        return new((ushort)(FinalFramesPointer + final * FrameByteCount), ColorsPerFrame);
    }

    /// <summary>Presentation operand after the duration; independent color payloads remain required under #1165.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)frame >= Frames.Count)
            throw new ArgumentOutOfRangeException(nameof(frame));
        var definition = Frames[frame];
        if ((uint)color >= definition.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return (ushort)(definition.FirstColorPointer + 2 * color);
    }

    /// <summary>Projects calculated native records by ordinal without storing a copied table.</summary>
    private sealed class CalculatedList<T>(int count, Func<int, T> at) : IReadOnlyList<T>
    {
        /// <summary>The fixed number of calculated records.</summary>
        public int Count => count;
        /// <summary>Calculates a record and rejects ordinals outside this program's bounds.</summary>
        public T this[int index] => (uint)index < Count ? at(index) : throw new ArgumentOutOfRangeException(nameof(index));
        /// <summary>Calculates each record once in native address order.</summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return at(index);
        }
        /// <summary>Adapts calculated records for non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
