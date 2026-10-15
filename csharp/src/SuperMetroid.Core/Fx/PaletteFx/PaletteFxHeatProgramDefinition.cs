namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One suit-specific Samus-in-heat palette control program.</summary>
/// <remarks>The $8D:E45E/E68A/E8B6 setup occupies eight bytes before phase zero.
/// Sixteen 34-byte records end at the terminal goto. These native layout relationships
/// calculate all cursor fields; the descriptor stores only suit identity and a calculated
/// list view, never a generated frame cache. Independently reviewed for #1165.</remarks>
public sealed class PaletteFxHeatProgramDefinition
{
    /// <summary>Fifteen BGR555 colors follow every timed duration word.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>Binds a suit identity to its calculated native frame address sequence.</summary>
    internal PaletteFxHeatProgramDefinition(PaletteFxHeatSuit suit)
    {
        Suit = suit;
        Frames = new FrameList(suit);
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
    public IReadOnlyList<PaletteFxHeatProgramFrameDefinition> Frames { get; }

    /// <summary>Calculates sixteen fixed-stride frame descriptors for a selected suit.</summary>
    private sealed class FrameList(PaletteFxHeatSuit suit) : IReadOnlyList<PaletteFxHeatProgramFrameDefinition>
    {
        /// <summary>Gets the number of timed heat palette records per suit.</summary>
        public int Count => 16;
        /// <summary>Gets a phase's native duration-word address as a frame descriptor.</summary>
        public PaletteFxHeatProgramFrameDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                return new(PaletteFxHeatInstructionListDefinitions.Resolve(suit, (ushort)index));
            }
        }
        /// <summary>Yields all sixteen calculated phase descriptors in program order.</summary>
        public IEnumerator<PaletteFxHeatProgramFrameDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same phase order to non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Reads aligned control fields; color payloads and other pointers are unowned.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        int setupOffset = pointer - ProgramStart;
        ushort? setupWord = setupOffset switch
        {
            0 => (ushort)PaletteFxInstruction.SetPreInstruction,
            2 => (ushort)PaletteFxPreInstruction.Heat,
            4 => (ushort)PaletteFxInstruction.SetColorIndex,
            6 => 0x0182,
            _ => null,
        };
        if (setupWord.HasValue) { value = setupWord.Value; return true; }
        if (pointer == LoopInstructionPointer) { value = (ushort)PaletteFxInstruction.Goto; return true; }
        if (pointer == LoopInstructionPointer + 2)
        {
            value = PaletteFxHeatInstructionListDefinitions.Resolve(Suit, 0);
            return true;
        }
        int offset = pointer - PaletteFxHeatInstructionListDefinitions.Resolve(Suit, 0);
        if ((uint)offset < 16 * 34)
        {
            if (offset % 34 == 0) { value = Duration(Suit, offset / 34); return true; }
            if (offset % 34 == 32) { value = (ushort)PaletteFxInstruction.Wait; return true; }
        }
        value = 0;
        return false;
    }
}
