namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Tourian escape entry into the shared red-flash loop.</summary>
public enum TourianEscapeSharedRedFlashPaletteOwner
{
    /// <summary>PalFxDef_Tourian20 ($8D:FFD1), entering at $F941 to select CGRAM byte $A8 and branch over the adjacent setup into the shared $F94D loop.</summary>
    GeneralLevel,
    /// <summary>PalFxDef_Tourian40 ($8D:FFD5), entering at $F949 to select CGRAM byte $E8 for Arkanoid blocks/red orbs and fall through into the shared $F94D loop.</summary>
    ArkanoidBlocksAndRedOrbs,
}

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
    /// <summary>Provides the two entry definitions that converge on the shared timed red-flash loop.</summary>
    internal static readonly IReadOnlyList<TourianEscapeSharedRedFlashPaletteFxProgramDefinition> Definitions = new ProgramEntries();

    /// <summary>Indexable view of the general-level and Arkanoid entry definitions in owner order.</summary>
    private sealed class ProgramEntries : IReadOnlyList<TourianEscapeSharedRedFlashPaletteFxProgramDefinition>
    {
        /// <summary>Gets the number of distinct entry definitions.</summary>
        public int Count => 2;

        /// <summary>Gets the entry definition for the requested red-flash owner.</summary>
        /// <param name="index">The owner value identifying the general-level or Arkanoid entry.</param>
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

        /// <summary>Enumerates the entry definitions in owner order.</summary>
        public IEnumerator<TourianEscapeSharedRedFlashPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>Returns one shared timed-record pointer.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one color word, skipping the inline CGRAM-index instruction.</summary>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        int offset = color < 6 ? sizeof(ushort) + color * sizeof(ushort) : 16;
        return unchecked((ushort)(FramePointer(frame) + offset));
    }

    /// <summary>Resolves one compiled mechanics word across both entries and the loop.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (TourianEscapeSharedRedFlashPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadEntryWord(pointer, out value))
                return true;
        }

        value = pointer switch
        {
            LoopInstructionPointer => PaletteFxInstructionCodes.Goto,
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
                14 => PaletteFxInstructionCodes.ColorPlus4,
                FrameByteCount - sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}

/// <summary>One entry into Tourian's shared escape red-flash loop.</summary>
public sealed class TourianEscapeSharedRedFlashPaletteFxProgramDefinition
{
    /// <summary>Creates the metadata for one entry into the shared Tourian red-flash loop.</summary>
    /// <param name="owner">The enemy category that owns this entry.</param>
    /// <param name="definitionPointer">The bank-$8D palette-FX definition address selecting the entry.</param>
    /// <param name="programStart">The instruction-list address where this entry begins.</param>
    /// <param name="colorByteIndex">The destination CGRAM byte selected before entering the shared loop.</param>
    /// <param name="usesGoto">Whether the entry branches over adjacent setup words to the shared loop.</param>
    internal TourianEscapeSharedRedFlashPaletteFxProgramDefinition(
        TourianEscapeSharedRedFlashPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex,
        bool usesGoto)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        UsesGoto = usesGoto;
    }

    /// <summary>The mutually exclusive entry owner.</summary>
    public TourianEscapeSharedRedFlashPaletteOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this entry.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The color-index setup entry.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>Whether this entry branches over the adjacent entry before the shared loop.</summary>
    public bool UsesGoto { get; }

    /// <summary>Resolves one compiled mechanics word in this entry.</summary>
    internal bool TryReadEntryWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            var item when item == ProgramStart + 2 => ColorByteIndex,
            var item when UsesGoto && item == ProgramStart + 4 =>
                PaletteFxInstructionCodes.Goto,
            var item when UsesGoto && item == ProgramStart + 6 =>
                TourianEscapeSharedRedFlashPaletteFxProgramMechanicsDefinitions.FirstFramePointer,
            _ => 0,
        };
        return value != 0;
    }
}
