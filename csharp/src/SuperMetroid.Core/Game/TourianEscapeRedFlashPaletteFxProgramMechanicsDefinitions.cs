namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive early Tourian escape red-flash program.</summary>
public enum TourianEscapeRedFlashPaletteOwner
{
    /// <summary>PalFxDef_Tourian8 ($8D:FFC9), flashing six shutter colors from CGRAM byte $0132 through the fourteen two-tick records at $F7A9.</summary>
    Shutter,
    /// <summary>PalFxDef_Tourian10 ($8D:FFCD), flashing four background colors from CGRAM byte $0070 through the fourteen four-tick records at $F891.</summary>
    Background,
}

/// <summary>Immutable mechanics for the first two Tourian escape palette loops.</summary>
/// <remarks>
/// The shutter and background programs contain fourteen uniform records apiece. Their
/// 140 BGR555 words remain live presentation data; this catalog owns palette placement,
/// record timing, waits, and loop control.
/// </remarks>
public static class TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions
{
    /// <summary><c>PalFxDef_Tourian8</c> at <c>$8D:FFC9</c>.</summary>
    public const ushort ShutterDefinitionPointer = 0xffc9;

    /// <summary><c>PalFxInstList_Tourian8</c> at <c>$8D:F7A9</c>.</summary>
    public const ushort ShutterProgramStart = 0xf7a9;

    /// <summary><c>PalFxDef_Tourian10</c> at <c>$8D:FFCD</c>.</summary>
    public const ushort BackgroundDefinitionPointer = 0xffcd;

    /// <summary><c>PalFxInstList_Tourian10</c> at <c>$8D:F891</c>.</summary>
    public const ushort BackgroundProgramStart = 0xf891;

    /// <summary>Both loops contain fourteen timed records.</summary>
    public const int FrameCount = 14;

    private static readonly TourianEscapeRedFlashPaletteFxProgramDefinition[] Definitions =
    [
        new(TourianEscapeRedFlashPaletteOwner.Shutter,
            ShutterDefinitionPointer, ShutterProgramStart, 0x0132, 2, 6),
        new(TourianEscapeRedFlashPaletteOwner.Background,
            BackgroundDefinitionPointer, BackgroundProgramStart, 0x0070, 4, 4),
    ];

    /// <summary>The shutter and background red-flash programs in definition order.</summary>
    public static IReadOnlyList<TourianEscapeRedFlashPaletteFxProgramDefinition> All { get; } = Array.AsReadOnly(Definitions);

    /// <summary>Resolves one compiled word-sized mechanic across both programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (TourianEscapeRedFlashPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }
}

/// <summary>One complete early Tourian escape red-flash control program.</summary>
public sealed class TourianEscapeRedFlashPaletteFxProgramDefinition
{
    internal TourianEscapeRedFlashPaletteFxProgramDefinition(
        TourianEscapeRedFlashPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex,
        ushort duration,
        int colorsPerFrame)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        Duration = duration;
        ColorsPerFrame = colorsPerFrame;
    }

    /// <summary>The mutually exclusive escape-palette owner.</summary>
    public TourianEscapeRedFlashPaletteOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The color-index setup entry.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The uniform display duration for every record.</summary>
    public ushort Duration { get; }

    /// <summary>The number of live BGR555 colors in every record.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public int FrameByteCount => sizeof(ushort) * (ColorsPerFrame + 2);

    /// <summary>The terminal <c>goto</c> command after all timed records.</summary>
    public ushort LoopInstructionPointer => unchecked((ushort)(
        FirstFramePointer +
        TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount *
        FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >=
            TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(frame));
        }
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one live BGR555 color word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => (ushort)PaletteFxInstruction.SetColorIndex,
            var item when item == ProgramStart + 2 => ColorByteIndex,
            var item when item == LoopInstructionPointer => (ushort)PaletteFxInstruction.Goto,
            var item when item == LoopInstructionPointer + 2 => FirstFramePointer,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0;
             frame < TourianEscapeRedFlashPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => Duration,
                var item when item == FrameByteCount - sizeof(ushort) =>
                    (ushort)PaletteFxInstruction.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
