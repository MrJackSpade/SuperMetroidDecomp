namespace SuperMetroid.Core.Game;

/// <summary>One complete early Tourian escape red-flash control program.</summary>
public sealed class TourianEscapeRedFlashPaletteFxProgramDefinition
{
    /// <summary>Binds one escape palette owner to its native setup pointer and uniform timed-record geometry.</summary>
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
