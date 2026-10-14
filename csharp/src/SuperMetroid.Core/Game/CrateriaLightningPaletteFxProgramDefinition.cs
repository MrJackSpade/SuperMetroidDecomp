namespace SuperMetroid.Core.Game;

/// <summary>Calculated setup, two repeated flash groups and neutral intervals for one lightning owner.</summary>
public sealed class CrateriaLightningPaletteFxProgramDefinition
{
    /// <summary>Calculates fixed layout addresses and timed records for the selected lightning owner.</summary>
    internal CrateriaLightningPaletteFxProgramDefinition(CrateriaLightningPaletteOwner owner)
    {
        Owner = owner;
        Frames = new CalculatedList<CrateriaLightningPaletteFrame>(12 + NeutralCount, Frame);
        MechanicsWords = new CalculatedList<PaletteFxMechanicsWord>(12 + Frames.Count * 2, Word);
        MechanicsBytes = new CalculatedList<PaletteFxMechanicsByte>(2, TimerByte);
    }

    /// <summary>Program identity selecting either the live $F765 surface lightning or retained unused $F769 dark-lightning mechanics and color resource.</summary>
    public CrateriaLightningPaletteOwner Owner { get; }
    /// <summary>Determines which cartridge layout, CGRAM destination, and record width apply.</summary>
    internal bool IsSurface => Owner == CrateriaLightningPaletteOwner.SurfaceLightning;
    /// <summary>$8D:EB3B/$EC6E setup starts: pre-instruction and CGRAM destination.</summary>
    public ushort ProgramStart => IsSurface ? (ushort)0xeb3b : (ushort)0xec6e;
    /// <summary>Surface uses CGRAM byte $A8; unused dark lightning uses byte $82.</summary>
    public ushort ColorByteIndex => IsSurface ? (ushort)0x00a8 : (ushort)0x0082;
    /// <summary>$8D:EB43/$EC76 first neutral frame follows the eight-byte setup.</summary>
    public ushort FirstFramePointer => (ushort)(ProgramStart + 8);
    /// <summary>Live BGR555 words per timed record: eight targeting CGRAM colors 84..91 for surface lightning, or seven targeting colors 65..71 for dark lightning.</summary>
    public int ColorsPerFrame => IsSurface ? 8 : 7;
    /// <summary>Thirteen surface or fourteen dark timed records in program address order, not expanded loop-execution order; dark lightning retains an additional 240-update neutral record.</summary>
    public IReadOnlyList<CrateriaLightningPaletteFrame> Frames { get; }
    /// <summary>Calculated setup/loop/branch words followed by each record's duration and terminal wait command; addresses may be unaligned and color words are excluded.</summary>
    public IReadOnlyList<PaletteFxMechanicsWord> MechanicsWords { get; }
    /// <summary>Two opaque entries marking this program's byte-sized timer operands in first/final group order; their addresses and values are resolved through <see cref="CrateriaLightningPaletteFxProgramMechanicsDefinitions.TryReadMechanicsByte"/>.</summary>
    public IReadOnlyList<PaletteFxMechanicsByte> MechanicsBytes { get; }

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
    /// <summary>The branch that can repeat the final four flashes.</summary>
    private ushort DecrementOnePointer => (ushort)(FinalFramesPointer + 4 * FrameByteCount);
    /// <summary>The final branch returns to the program's opening neutral record.</summary>
    private ushort GotoPointer => (ushort)(DecrementOnePointer + 4);

    /// <summary>Calculates a neutral or flash record's native address, duration, and live color count.</summary>
    private CrateriaLightningPaletteFrame Frame(int index)
    {
        if (index == 0)
            return new(FirstFramePointer, 240, ColorsPerFrame);
        if (index <= 7)
            return new((ushort)(RepeatedFramesPointer + (index - 1) * FrameByteCount),
                index is 1 or 7 ? (ushort)2 : (ushort)1, ColorsPerFrame);
        if (index < 8 + NeutralCount)
            return new((ushort)(NeutralFramesPointer + (index - 8) * FrameByteCount), 240, ColorsPerFrame);
        int final = index - 8 - NeutralCount;
        return new((ushort)(FinalFramesPointer + final * FrameByteCount), final == 3 ? (ushort)2 : (ushort)1, ColorsPerFrame);
    }

    /// <summary>Calculates setup, timer, branch, duration, and wait words while skipping live BGR555 payloads.</summary>
    private PaletteFxMechanicsWord Word(int index)
    {
        if (index >= 12)
        {
            var frame = Frame((index - 12) / 2);
            return (index & 1) == 0 ? new(frame.Pointer, frame.Duration)
                : new(frame.WaitInstructionPointer, (ushort)PaletteFxInstruction.Wait);
        }
        return index switch
        {
            0 => new(ProgramStart, (ushort)PaletteFxInstruction.SetPreInstruction),
            1 => new((ushort)(ProgramStart + 2), IsSurface ? (ushort)PaletteFxPreInstruction.SwitchAboveY380 : (ushort)PaletteFxPreInstruction.SwitchAboveY380Second),
            2 => new((ushort)(ProgramStart + 4), (ushort)PaletteFxInstruction.SetColorIndex),
            3 => new((ushort)(ProgramStart + 6), ColorByteIndex),
            4 => new(TimerTwoPointer, (ushort)PaletteFxInstruction.SetTimer),
            5 => new(DecrementTwoPointer, (ushort)PaletteFxInstruction.DecrementTimerAndGoto),
            6 => new((ushort)(DecrementTwoPointer + 2), RepeatedFramesPointer),
            7 => new(TimerOnePointer, (ushort)PaletteFxInstruction.SetTimer),
            8 => new(DecrementOnePointer, (ushort)PaletteFxInstruction.DecrementTimerAndGoto),
            9 => new((ushort)(DecrementOnePointer + 2), FinalFramesPointer),
            10 => new(GotoPointer, (ushort)PaletteFxInstruction.Goto),
            _ => new((ushort)(GotoPointer + 2), FirstFramePointer),
        };
    }

    /// <summary>Retains ordinal identity for each byte-sized timer operand without duplicating its value.</summary>
    private PaletteFxMechanicsByte TimerByte(int index) => index == 0
        ? new() : new();

    /// <summary>Finds an exact compiled word address, including unaligned duration words after byte operands.</summary>
    internal bool TryReadWord(ushort pointer, out ushort value)
    {
        for (int index = 0; index < MechanicsWords.Count; index++)
        {
            var word = Word(index);
            if (word.Pointer == pointer) { value = word.Value; return true; }
        }
        value = 0;
        return false;
    }

    /// <summary>Finds the two timer bytes whose values are two and one for the respective flash groups.</summary>
    internal bool TryReadByte(ushort pointer, out byte value)
    {
        if (pointer == TimerTwoPointer + 2) { value = 2; return true; }
        if (pointer == TimerOnePointer + 2) { value = 1; return true; }
        value = 0;
        return false;
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
