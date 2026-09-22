namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Tourian escape entry into the shared red-flash loop.</summary>
public enum TourianEscapeSharedRedFlashPaletteOwner
{
    GeneralLevel,
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

    /// <summary>The complete shared loop lasts 28 frames.</summary>
    public const int CycleFrames = 28;

    private static readonly TourianEscapeSharedRedFlashPaletteFxProgramDefinition[] Definitions =
    [
        new(TourianEscapeSharedRedFlashPaletteOwner.GeneralLevel,
            GeneralLevelDefinitionPointer, GeneralLevelProgramStart, 0x00a8, usesGoto: true),
        new(TourianEscapeSharedRedFlashPaletteOwner.ArkanoidBlocksAndRedOrbs,
            ArkanoidDefinitionPointer, ArkanoidProgramStart, 0x00e8, usesGoto: false),
    ];
    private static readonly IReadOnlyList<TourianEscapeSharedRedFlashPaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The two entries into the shared red-flash loop.</summary>
    public static IReadOnlyList<TourianEscapeSharedRedFlashPaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Returns one shared timed-record pointer.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
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
