namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive old-Tourian escape accent palette program.</summary>
public enum OldTourianEscapeAccentPaletteOwner
{
    OrangeRailings,
    YellowPanels,
}

/// <summary>Immutable mechanics for the old-Tourian railing and panel flash loops.</summary>
/// <remarks>
/// Definitions <c>$FFDD</c> and <c>$FFE1</c> use identical fifteen-record timing.
/// Their 90 BGR555 words remain live presentation data; this catalog owns palette
/// placement, timing, waits, and loop control.
/// </remarks>
public static class OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions
{
    /// <summary><c>PalFxDef_Crateria10</c> at <c>$8D:FFDD</c>.</summary>
    public const ushort OrangeRailingsDefinitionPointer = 0xffdd;

    /// <summary><c>PalFxInstList_Crateria10</c> at <c>$8D:FBC1</c>.</summary>
    public const ushort OrangeRailingsProgramStart = 0xfbc1;

    /// <summary><c>PalFxDef_Crateria20</c> at <c>$8D:FFE1</c>.</summary>
    public const ushort YellowPanelsDefinitionPointer = 0xffe1;

    /// <summary><c>PalFxInstList_Crateria20</c> at <c>$8D:FC5F</c>.</summary>
    public const ushort YellowPanelsProgramStart = 0xfc5f;

    /// <summary>Both loops contain fifteen timed records.</summary>
    public const int FrameCount = 15;

    /// <summary>Each record writes three live BGR555 colors.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 10;

    /// <summary>Both complete loops last 64 frames.</summary>
    public const int CycleFrames = 64;

    private static readonly ushort[] Durations =
        [16, 1, 1, 2, 1, 2, 1, 1, 1, 1, 32, 2, 1, 1, 1];

    private static readonly OldTourianEscapeAccentPaletteFxProgramDefinition[] Definitions =
    [
        new(OldTourianEscapeAccentPaletteOwner.OrangeRailings,
            OrangeRailingsDefinitionPointer, OrangeRailingsProgramStart, 0x00d2),
        new(OldTourianEscapeAccentPaletteOwner.YellowPanels,
            YellowPanelsDefinitionPointer, YellowPanelsProgramStart, 0x00aa),
    ];
    private static readonly IReadOnlyList<OldTourianEscapeAccentPaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The orange-railing and yellow-panel programs in definition order.</summary>
    public static IReadOnlyList<OldTourianEscapeAccentPaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves one compiled mechanics word across both programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (OldTourianEscapeAccentPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }

    internal static ushort Duration(int frame) => Durations[frame];
}

/// <summary>One complete old-Tourian escape accent control program.</summary>
public sealed class OldTourianEscapeAccentPaletteFxProgramDefinition
{
    internal OldTourianEscapeAccentPaletteFxProgramDefinition(
        OldTourianEscapeAccentPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
    }

    /// <summary>The mutually exclusive escape-accent owner.</summary>
    public OldTourianEscapeAccentPaletteOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The color-index setup entry.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>The terminal <c>goto</c> command after all timed records.</summary>
    public ushort LoopInstructionPointer => unchecked((ushort)(
        FirstFramePointer +
        OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount *
        OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >=
            OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(frame));
        }
        return unchecked((ushort)(FirstFramePointer + frame *
            OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            var item when item == ProgramStart + 2 => ColorByteIndex,
            var item when item == LoopInstructionPointer => PaletteFxInstructionCodes.Goto,
            var item when item == LoopInstructionPointer + 2 => FirstFramePointer,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0;
             frame < OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.Duration(frame),
                OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameByteCount -
                    sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
