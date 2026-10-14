namespace SuperMetroid.Core.Game;

/// <summary>One complete old-Tourian escape accent control program.</summary>
public sealed class OldTourianEscapeAccentPaletteFxProgramDefinition
{
    /// <summary>Records one accent destination and its independent bank 8D setup list.</summary>
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

    /// <summary>Locates one of fifteen timed records after this entry's color-index setup.</summary>
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

    /// <summary>Locates one of three live BGR555 words immediately after a record's duration.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions
                .ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Recognizes this list's color-index setup, authored durations, waits, and terminal loop.</summary>
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
             frame < OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.Duration(frame),
                OldTourianEscapeAccentPaletteFxProgramMechanicsDefinitions.FrameByteCount -
                    sizeof(ushort) => (ushort)PaletteFxInstruction.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
