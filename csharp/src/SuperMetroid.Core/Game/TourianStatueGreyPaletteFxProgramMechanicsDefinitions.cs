namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Tourian entrance statue selected for greying.</summary>
public enum TourianStatueBoss
{
    Draygon,
    Kraid,
    Ridley,
    Phantoon,
}

/// <summary>
/// Immutable control words for the four Tourian entrance-statue grey-out palette programs.
/// </summary>
/// <remarks>
/// The four entries at $8D:E222-$E23A select a CGRAM destination and converge on the
/// shared eight-frame program at $8D:E23E. Its 64 BGR555 colors remain live presentation
/// data; only color-index setup, branches, durations, waits, and deletion are compiled.
/// </remarks>
public static class TourianStatueGreyPaletteFxProgramMechanicsDefinitions
{
    private static readonly TourianStatueGreyPaletteFxProgramDefinition[] Definitions =
    [
        new(TourianStatueBoss.Draygon, 0xe222, 0x00c0, usesGoto: true),
        new(TourianStatueBoss.Kraid, 0xe22a, 0x00e0, usesGoto: true),
        new(TourianStatueBoss.Ridley, 0xe232, 0x0120, usesGoto: true),
        new(TourianStatueBoss.Phantoon, 0xe23a, 0x0140, usesGoto: false),
    ];
    private static readonly IReadOnlyList<TourianStatueGreyPaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary><c>InstList_PaletteFXObject_Common_GreyOutTourianStatue</c> at $8D:E23E.</summary>
    public const ushort FirstFramePointer = 0xe23e;

    /// <summary>Eight eight-frame records form the statue grey-out sequence.</summary>
    public const int FrameCount = 8;

    /// <summary>Eight BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>Bytes from one duration word through its terminal wait command.</summary>
    public const int FrameByteCount = 20;

    /// <summary>Terminal <c>delete</c> instruction at $8D:E2DE.</summary>
    public const ushort DeleteInstructionPointer = 0xe2de;

    /// <summary>The Draygon, Kraid, Ridley, and Phantoon entries in cartridge order.</summary>
    public static IReadOnlyList<TourianStatueGreyPaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves one compiled mechanics word across all four statue entries.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (TourianStatueGreyPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        int frameOffset = pointer - FirstFramePointer;
        if (frameOffset >= 0 && frameOffset < FrameCount * FrameByteCount)
        {
            int inFrame = frameOffset % FrameByteCount;
            if (inFrame == 0)
            {
                value = 8;
                return true;
            }
            if (inFrame == FrameByteCount - sizeof(ushort))
            {
                value = PaletteFxInstructionCodes.Wait;
                return true;
            }
        }

        if (pointer == DeleteInstructionPointer)
        {
            value = PaletteFxInstructionCodes.Delete;
            return true;
        }

        value = 0;
        return false;
    }

    /// <summary>Returns the timed-record pointer for one zero-based fade frame.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }
}

/// <summary>One boss-specific entry into the shared Tourian statue grey-out program.</summary>
public sealed class TourianStatueGreyPaletteFxProgramDefinition
{
    internal TourianStatueGreyPaletteFxProgramDefinition(
        TourianStatueBoss boss,
        ushort programStart,
        ushort colorByteIndex,
        bool usesGoto)
    {
        Boss = boss;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        UsesGoto = usesGoto;
    }

    /// <summary>The statue boss represented by this entry.</summary>
    public TourianStatueBoss Boss { get; }

    /// <summary>The boss-specific entry instruction-list pointer.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The byte index of the statue's first CGRAM color.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>Whether this entry uses an explicit branch rather than falling through.</summary>
    public bool UsesGoto { get; }

    /// <summary>Reads one boss-specific setup word.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        int offset = pointer - ProgramStart;
        ushort? setupWord = offset switch
        {
            0 => PaletteFxInstructionCodes.SetColorIndex,
            2 => ColorByteIndex,
            4 when UsesGoto => PaletteFxInstructionCodes.Goto,
            6 when UsesGoto =>
                TourianStatueGreyPaletteFxProgramMechanicsDefinitions.FirstFramePointer,
            _ => null,
        };
        if (setupWord.HasValue)
        {
            value = setupWord.Value;
            return true;
        }

        value = 0;
        return false;
    }
}
