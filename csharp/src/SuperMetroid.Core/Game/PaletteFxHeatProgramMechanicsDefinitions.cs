namespace SuperMetroid.Core.Game;

/// <summary>
/// Immutable control words for the three Samus-in-heat palette programs in bank $8D.
/// </summary>
/// <remarks>
/// Each timed record contains fifteen BGR555 colors between its duration and terminal
/// wait command. Those 720 color words remain live presentation data; this catalog owns
/// only program setup, timing, waits, and loop control.
/// Power, Varia, and Gravity enter at $8D:E45E, $E68A, and $E8B6;
/// each installs heat pre-instruction $E379 and CGRAM byte $0182.
/// Their sixteen records start at $E466, $E692, or $E8BE plus $22*p,
/// p=0..15, and each ends in $C595 wait. Gotos at $E686, $E8B2,
/// and $EADE return to phase zero; p=16 reaches control. All 114
/// mechanics words match the pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class PaletteFxHeatProgramMechanicsDefinitions
{
    private static readonly PaletteFxHeatProgramDefinition Power = new(PaletteFxHeatSuit.Power);
    private static readonly PaletteFxHeatProgramDefinition Varia = new(PaletteFxHeatSuit.Varia);
    private static readonly PaletteFxHeatProgramDefinition Gravity = new(PaletteFxHeatSuit.Gravity);
    private static readonly IReadOnlyList<PaletteFxHeatProgramDefinition> Programs = new ProgramList();

    /// <summary>The Power, Varia, and Gravity programs in native selection order.</summary>
    public static IReadOnlyList<PaletteFxHeatProgramDefinition> All => Programs;

    private sealed class ProgramList : IReadOnlyList<PaletteFxHeatProgramDefinition>
    {
        public int Count => 3;
        public PaletteFxHeatProgramDefinition this[int index] => index switch
        {
            0 => Power, 1 => Varia, 2 => Gravity,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        public IEnumerator<PaletteFxHeatProgramDefinition> GetEnumerator()
        {
            yield return Power;
            yield return Varia;
            yield return Gravity;
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Resolves one compiled mechanics word across the three named suit programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value) =>
        Power.TryReadMechanicsWord(pointer, out value) ||
        Varia.TryReadMechanicsWord(pointer, out value) ||
        Gravity.TryReadMechanicsWord(pointer, out value);
}

/// <summary>One suit-specific Samus-in-heat palette control program.</summary>
/// <remarks>The $8D:E45E/E68A/E8B6 setup occupies eight bytes before phase zero.
/// Sixteen 34-byte records end at the terminal goto. These native layout relationships
/// calculate all cursor fields; the descriptor stores only suit identity and a calculated
/// list view, never a generated frame cache. Independently reviewed for #1165.</remarks>
public sealed class PaletteFxHeatProgramDefinition
{
    /// <summary>Fifteen BGR555 colors follow every timed duration word.</summary>
    public const int ColorsPerFrame = 15;
    private readonly IReadOnlyList<PaletteFxHeatProgramFrameDefinition> frames;

    internal PaletteFxHeatProgramDefinition(PaletteFxHeatSuit suit)
    {
        Suit = suit;
        frames = new FrameList(suit);
    }

    /// <summary>The mutually exclusive suit palette represented by this program.</summary>
    public PaletteFxHeatSuit Suit { get; }
    /// <summary>Eight-byte setup preceding the first selected native frame.</summary>
    public ushort ProgramStart => (ushort)(PaletteFxHeatInstructionListDefinitions.Resolve(Suit, 0) - 8);
    /// <summary>Native goto after sixteen duration/color/wait records.</summary>
    public ushort LoopInstructionPointer => (ushort)(PaletteFxHeatInstructionListDefinitions.Resolve(Suit, 0) + 16 * 34);

    /// <summary>Calculated duration for one of the sixteen native phases.</summary>
    /// <remarks>Native $8D:E466/E692/E8BE records hold at phase zero for16 frames,
    /// then traverse a mirrored duration ramp with a floor4 and plateau8. Interior
    /// p=1..14 uses min(8,max(4,min(p,15-p)+2)). The terminal phase holds16 for
    /// protected suits; Power uses3 before its loop. These explicit endpoint roles
    /// and the symmetric interior reproduce all48 original durations. No schedule
    /// array remains. Source: supported NTSC J/U v1.0 and pinned bank_8D.asm.</remarks>
    internal static ushort Duration(PaletteFxHeatSuit suit, int phase)
    {
        if ((uint)phase >= 16) throw new ArgumentOutOfRangeException(nameof(phase));
        if (phase == 0) return 16;
        if (phase == 15) return (ushort)(suit == PaletteFxHeatSuit.Power ? 3 : 16);
        return (ushort)Math.Min(8, Math.Max(4, Math.Min(phase, 15 - phase) + 2));
    }

    /// <summary>The sixteen timed records, calculated on access without stored rows.</summary>
    public IReadOnlyList<PaletteFxHeatProgramFrameDefinition> Frames => frames;

    private sealed class FrameList(PaletteFxHeatSuit suit) : IReadOnlyList<PaletteFxHeatProgramFrameDefinition>
    {
        public int Count => 16;
        public PaletteFxHeatProgramFrameDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                return new(PaletteFxHeatInstructionListDefinitions.Resolve(suit, (ushort)index), Duration(suit, index));
            }
        }
        public IEnumerator<PaletteFxHeatProgramFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Reads aligned control fields; color payloads and other pointers are unowned.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        int setupOffset = pointer - ProgramStart;
        ushort? setupWord = setupOffset switch
        {
            0 => PaletteFxInstructionCodes.SetPreInstruction,
            2 => PaletteFxPreInstructionCodes.Heat,
            4 => PaletteFxInstructionCodes.SetColorIndex,
            6 => 0x0182,
            _ => null,
        };
        if (setupWord.HasValue) { value = setupWord.Value; return true; }
        if (pointer == LoopInstructionPointer) { value = PaletteFxInstructionCodes.Goto; return true; }
        if (pointer == LoopInstructionPointer + 2)
        {
            value = PaletteFxHeatInstructionListDefinitions.Resolve(Suit, 0);
            return true;
        }
        int offset = pointer - PaletteFxHeatInstructionListDefinitions.Resolve(Suit, 0);
        if ((uint)offset < 16 * 34)
        {
            if (offset % 34 == 0) { value = Duration(Suit, offset / 34); return true; }
            if (offset % 34 == 32) { value = PaletteFxInstructionCodes.Wait; return true; }
        }
        value = 0;
        return false;
    }
}
/// <summary>One timed fifteen-color record in a Samus-in-heat palette program.</summary>
public readonly record struct PaletteFxHeatProgramFrameDefinition(
    ushort InstructionPointer,
    ushort Duration)
{
    /// <summary>The first live BGR555 presentation word after the duration.</summary>
    /// <remarks>Derived from the native duration word followed immediately by color data.
    /// The color values and repeated color-row relationships require their own review;
    /// this address calculation is not a disposition for their contents.</remarks>
    public ushort FirstColorPointer => unchecked((ushort)(InstructionPointer + sizeof(ushort)));
}
