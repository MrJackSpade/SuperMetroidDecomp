namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Crateria lightning palette program.</summary>
public enum CrateriaLightningPaletteOwner
{
    /// <summary>$8D:F765, Crateria 1 lightning: program $8D:EB3B flashes BG palette 5 colors 4..11 at the Landing Site before power bombs are acquired.</summary>
    SurfaceLightning,
    /// <summary>$8D:F769, unused dark-lightning counterpart: program $8D:EC6E darkens seven colors starting at CGRAM color 65.</summary>
    UnusedDarkLightning,
}

/// <summary>One immutable word-sized palette-program mechanic.</summary>
/// <param name="Pointer">Bank-$8D byte address of the control word or duration, including unaligned addresses after byte-sized timer operands.</param>
/// <param name="Value">Compiled sixteen-bit instruction, operand, or duration; never an editable BGR555 color payload.</param>
public readonly record struct PaletteFxMechanicsWord(ushort Pointer, ushort Value);

/// <summary>One immutable byte-sized palette-program mechanic.</summary>
public readonly record struct PaletteFxMechanicsByte();

/// <summary>One timed lightning color record whose BGR555 payload remains live.</summary>
/// <param name="Pointer">Bank-$8D byte address of this record's duration word.</param>
/// <param name="Duration">Authored palette-FX instruction timer: neutral records use 240 updates, flash records one or two.</param>
/// <param name="ColorCount">Consecutive live CGRAM color words following the duration: eight for surface lightning or seven for dark lightning.</param>
public readonly record struct CrateriaLightningPaletteFrame(
    ushort Pointer,
    ushort Duration,
    int ColorCount)
{
    /// <summary>The first live BGR555 presentation word after the duration.</summary>
    public ushort FirstColorPointer => unchecked((ushort)(Pointer + sizeof(ushort)));

    /// <summary>The terminal wait command after the live colors.</summary>
    public ushort WaitInstructionPointer => unchecked((ushort)(
        FirstColorPointer + ColorCount * sizeof(ushort)));
}

/// <summary>
/// Immutable control words and byte operands for Crateria's two lightning programs.
/// </summary>
/// <remarks>
/// Definition <c>$F765</c> is the live Landing Site lightning effect. Definition
/// <c>$F769</c> is the cartridge's unused dark-lightning counterpart. Their 202 BGR555
/// words remain presentation data; timer setup, timing, branches, and targets are compiled.
/// </remarks>
public static class CrateriaLightningPaletteFxProgramMechanicsDefinitions
{
    private static readonly CrateriaLightningPaletteFxProgramDefinition Surface = new(CrateriaLightningPaletteOwner.SurfaceLightning);
    private static readonly CrateriaLightningPaletteFxProgramDefinition Dark = new(CrateriaLightningPaletteOwner.UnusedDarkLightning);
    private static readonly ProgramList Programs = new();

    /// <summary>The live and unused-dark programs in cartridge definition order.</summary>
    public static IReadOnlyList<CrateriaLightningPaletteFxProgramDefinition> All => Programs;
    /// <summary>$8D:EC59/$ED84 restart the neutral record when Samus is above Y=$0380.</summary>
    public const ushort VerticalSwitchSamusY = 0x0380;

    /// <summary>Resolves a compiled control or duration word from either lightning program, excluding their editable color payloads.</summary>
    /// <param name="pointer">Bank-$8D byte address; lookup requires an exact word-start identity, not general word alignment.</param>
    /// <param name="value">Compiled word on success, or zero when the address is not owned by either program's word mechanics.</param>
    /// <returns>Whether the address identifies a known instruction, operand, or duration word.</returns>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value) =>
        Surface.TryReadWord(pointer, out value) || Dark.TryReadWord(pointer, out value);
    /// <summary>Resolves the byte-sized loop-timer operands of either lightning program: two repetitions for the first flash group and one for the final group.</summary>
    /// <param name="pointer">Exact bank-$8D byte address immediately after a set-timer instruction.</param>
    /// <param name="value">Timer value, two or one, on success; otherwise zero.</param>
    /// <returns>Whether the address identifies one of the four compiled timer operands across both programs.</returns>
    public static bool TryReadMechanicsByte(ushort pointer, out byte value) =>
        Surface.TryReadByte(pointer, out value) || Dark.TryReadByte(pointer, out value);

    private sealed class ProgramList : IReadOnlyList<CrateriaLightningPaletteFxProgramDefinition>
    {
        public int Count => 2;
        public CrateriaLightningPaletteFxProgramDefinition this[int index] => index switch
        {
            0 => Surface,
            1 => Dark,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        public IEnumerator<CrateriaLightningPaletteFxProgramDefinition> GetEnumerator()
        { yield return Surface; yield return Dark; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

/// <summary>Calculated setup, two repeated flash groups and neutral intervals for one lightning owner.</summary>
public sealed class CrateriaLightningPaletteFxProgramDefinition
{
    internal CrateriaLightningPaletteFxProgramDefinition(CrateriaLightningPaletteOwner owner)
    {
        Owner = owner;
        Frames = new CalculatedList<CrateriaLightningPaletteFrame>(12 + NeutralCount, Frame);
        MechanicsWords = new CalculatedList<PaletteFxMechanicsWord>(12 + Frames.Count * 2, Word);
        MechanicsBytes = new CalculatedList<PaletteFxMechanicsByte>(2, TimerByte);
    }

    /// <summary>Program identity selecting either the live $F765 surface lightning or retained unused $F769 dark-lightning mechanics and color resource.</summary>
    public CrateriaLightningPaletteOwner Owner { get; }
    private bool IsSurface => Owner == CrateriaLightningPaletteOwner.SurfaceLightning;
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

    private int NeutralCount => IsSurface ? 1 : 2;
    private int FrameByteCount => 2 * (ColorsPerFrame + 2);
    private ushort TimerTwoPointer => (ushort)(FirstFramePointer + FrameByteCount);
    private ushort RepeatedFramesPointer => (ushort)(TimerTwoPointer + 3);
    private ushort DecrementTwoPointer => (ushort)(RepeatedFramesPointer + 7 * FrameByteCount);
    private ushort NeutralFramesPointer => (ushort)(DecrementTwoPointer + 4);
    private ushort TimerOnePointer => (ushort)(NeutralFramesPointer + NeutralCount * FrameByteCount);
    private ushort FinalFramesPointer => (ushort)(TimerOnePointer + 3);
    private ushort DecrementOnePointer => (ushort)(FinalFramesPointer + 4 * FrameByteCount);
    private ushort GotoPointer => (ushort)(DecrementOnePointer + 4);

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

    private PaletteFxMechanicsWord Word(int index)
    {
        if (index >= 12)
        {
            var frame = Frame((index - 12) / 2);
            return (index & 1) == 0 ? new(frame.Pointer, frame.Duration)
                : new(frame.WaitInstructionPointer, PaletteFxInstructionCodes.Wait);
        }
        return index switch
        {
            0 => new(ProgramStart, PaletteFxInstructionCodes.SetPreInstruction),
            1 => new((ushort)(ProgramStart + 2), IsSurface ? PaletteFxPreInstructionCodes.SwitchAboveY380 : PaletteFxPreInstructionCodes.SwitchAboveY380Second),
            2 => new((ushort)(ProgramStart + 4), PaletteFxInstructionCodes.SetColorIndex),
            3 => new((ushort)(ProgramStart + 6), ColorByteIndex),
            4 => new(TimerTwoPointer, PaletteFxInstructionCodes.SetTimer),
            5 => new(DecrementTwoPointer, PaletteFxInstructionCodes.DecrementTimerAndGoto),
            6 => new((ushort)(DecrementTwoPointer + 2), RepeatedFramesPointer),
            7 => new(TimerOnePointer, PaletteFxInstructionCodes.SetTimer),
            8 => new(DecrementOnePointer, PaletteFxInstructionCodes.DecrementTimerAndGoto),
            9 => new((ushort)(DecrementOnePointer + 2), FinalFramesPointer),
            10 => new(GotoPointer, PaletteFxInstructionCodes.Goto),
            _ => new((ushort)(GotoPointer + 2), FirstFramePointer),
        };
    }

    private PaletteFxMechanicsByte TimerByte(int index) => index == 0
        ? new() : new();

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

    private sealed class CalculatedList<T>(int count, Func<int, T> at) : IReadOnlyList<T>
    {
        public int Count => count;
        public T this[int index] => (uint)index < Count ? at(index) : throw new ArgumentOutOfRangeException(nameof(index));
        public IEnumerator<T> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return at(index);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
