namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete late-Crateria escape palette control program.</summary>
public sealed class CrateriaEscapeLightningPaletteFxProgramDefinition
{
    /// <summary>Stores a program's native setup, destination, and color-row dimensions.</summary>
    internal CrateriaEscapeLightningPaletteFxProgramDefinition(
        CrateriaEscapeLightningPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex,
        int colorsPerFrame)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        ColorsPerFrame = colorsPerFrame;
    }

    /// <summary>The mutually exclusive palette owner.</summary>
    public CrateriaEscapeLightningPaletteOwner Owner { get; }
    /// <summary>The palette-FX definition identity that installs this program.</summary>
    public ushort DefinitionPointer { get; }
    /// <summary>The color-index setup entry.</summary>
    public ushort ProgramStart { get; }
    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }
    /// <summary>The number of live BGR555 colors in every record.</summary>
    public int ColorsPerFrame { get; }
    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));
    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public int FrameByteCount => sizeof(ushort) * (ColorsPerFrame + 2);
    /// <summary>The terminal <c>goto</c> command after all timed records.</summary>
    public ushort LoopInstructionPointer => unchecked((ushort)(FirstFramePointer +
        CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.FrameCount * FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
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
        for (int frame = 0; frame < CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.FrameCount; frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.Duration(frame),
                var item when item == FrameByteCount - sizeof(ushort) => (ushort)PaletteFxInstruction.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }
        return false;
    }
}
