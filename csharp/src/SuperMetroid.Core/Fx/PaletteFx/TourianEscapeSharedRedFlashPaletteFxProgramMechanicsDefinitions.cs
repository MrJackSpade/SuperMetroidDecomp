namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for Tourian's shared general-level red-flash loop.</summary>
/// <remarks>
/// Definitions <c>$FFD1</c> and <c>$FFD5</c> select different CGRAM destinations before
/// converging at <c>$F94D</c>. The 98 BGR555 words remain live presentation data; this
/// catalog owns entry routing, timing, the inline CGRAM skip, waits, and loop control.
/// </remarks>
public static class TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions
{
    /// <summary><c>PalFxDef_Tourian20</c> at <c>$8D:FFD1</c>.</summary>
    public const ushort GeneralLevelDefinitionPointer = 0xffd1;

    /// <summary><c>PalFxInstList_Tourian20</c> at <c>$8D:F941</c>.</summary>
    public const ushort GeneralLevelProgramStart = 0xf941;

    /// <summary><c>PalFxDef_Tourian40</c> at <c>$8D:FFD5</c>.</summary>
    public const ushort ArkanoidDefinitionPointer = 0xffd5;

    /// <summary><c>PalFxInstList_Tourian40</c> at <c>$8D:F949</c>.</summary>
    public const ushort ArkanoidProgramStart = 0xf949;

    /// <summary>The shared timed-record loop at <c>$8D:F94D</c>.</summary>
    public const ushort FirstFramePointer = 0xf94d;

    /// <summary>The terminal <c>goto</c> at <c>$8D:FA65</c>.</summary>
    public const ushort LoopInstructionPointer = 0xfa65;

    /// <summary>The shared loop contains fourteen records.</summary>
    public const int FrameCount = 14;

    /// <summary>Each record writes seven live BGR555 colors.</summary>
    public const int ColorsPerFrame = 7;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 20;

    /// <summary>CGRAM byte $A8: general-level red-flash destination.</summary>
    private const ushort GeneralColorByte = 0x00a8;
    /// <summary>CGRAM byte $E8: Arkanoid-block and red-orb flash destination.</summary>
    private const ushort ArkanoidColorByte = 0x00e8;
    /// <summary>Both palette-FX entries, ordered by their owner value and materialized without a backing array.</summary>
    internal static readonly IReadOnlyList<TourianEscapeSharedRedFlashPaletteFxProgramDefinition> Definitions = new ProgramEntries();
    /// <summary>Projects each owner onto its definition pointer, entry list, CGRAM destination, and branch policy.</summary>
    private sealed class ProgramEntries : IReadOnlyList<TourianEscapeSharedRedFlashPaletteFxProgramDefinition>
    {
        /// <summary>Tourian's two definitions converge on the same timed-record loop.</summary>
        public int Count => 2;
        /// <summary>Indexes the general-level entry first and the Arkanoid/red-orb entry second.</summary>
        public TourianEscapeSharedRedFlashPaletteFxProgramDefinition this[int index] =>
            (TourianEscapeSharedRedFlashPaletteOwner)index switch
            {
                TourianEscapeSharedRedFlashPaletteOwner.GeneralLevel => new(
                    TourianEscapeSharedRedFlashPaletteOwner.GeneralLevel,
                    GeneralLevelDefinitionPointer, GeneralLevelProgramStart, GeneralColorByte, usesGoto: true),
                TourianEscapeSharedRedFlashPaletteOwner.ArkanoidBlocksAndRedOrbs => new(
                    TourianEscapeSharedRedFlashPaletteOwner.ArkanoidBlocksAndRedOrbs,
                    ArkanoidDefinitionPointer, ArkanoidProgramStart, ArkanoidColorByte, usesGoto: false),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        /// <summary>Visits both entries in the same order as the owner values.</summary>
        public IEnumerator<TourianEscapeSharedRedFlashPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Uses the typed entry iteration for callers of the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Calculates a timed-record address from the first frame and its 20-byte stride, rejecting out-of-range frames.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Locates a live color word, accounting for the inline CGRAM-index instruction before the seventh color.</summary>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        int offset = color < 6 ? sizeof(ushort) + color * sizeof(ushort) : 16;
        return unchecked((ushort)(FramePointer(frame) + offset));
    }

    /// <summary>Recognizes entry setup, loop control, duration, index increment, and wait words while leaving colors live.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (TourianEscapeSharedRedFlashPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadEntryWord(pointer, out value))
                return true;
        }

        value = pointer switch
        {
            LoopInstructionPointer => (ushort)PaletteFxInstruction.Goto,
            LoopInstructionPointer + 2 => FirstFramePointer,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0; frame < FrameCount; frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => 2,
                14 => (ushort)PaletteFxInstruction.ColorPlus4,
                FrameByteCount - sizeof(ushort) => (ushort)PaletteFxInstruction.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
