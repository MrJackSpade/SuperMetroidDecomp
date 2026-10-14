namespace SuperMetroid.Core.Game;

/// <summary>One complete exploding-Zebes layer-fade control program.</summary>
public sealed class ZebesExplosionLayerFadePaletteFxProgramDefinition
{
    /// <summary>Binds one layer's native setup pointer, CGRAM destination, and uniform hold duration.</summary>
    internal ZebesExplosionLayerFadePaletteFxProgramDefinition(
        ZebesExplosionLayerFadePaletteFxProgramOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex,
        ushort frameDuration)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        FrameDuration = frameDuration;
    }

    /// <summary>The mutually exclusive faded layer.</summary>
    public ZebesExplosionLayerFadePaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    /// <remarks><c>$8D:E1DC</c> is crust and <c>$8D:E1E0</c> is grey clouds.</remarks>
    public ushort DefinitionPointer { get; }

    /// <summary>The native instruction-list entry.</summary>
    /// <remarks><c>$8D:D48E</c> is crust and <c>$8D:D5A4</c> is grey clouds.</remarks>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The cartridge-authored duration of each timed record.</summary>
    public ushort FrameDuration { get; }

    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>The terminal <c>delete</c> command after all timed records.</summary>
    public ushort DeleteInstructionPointer => unchecked((ushort)(FirstFramePointer +
        ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount *
        ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame *
            ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Returns one live BGR555 color word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions
                .ColorsPerFrame)
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
            var item when item == DeleteInstructionPointer => (ushort)PaletteFxInstruction.Delete,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0;
             frame < ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => FrameDuration,
                ZebesExplosionLayerFadePaletteFxProgramMechanicsDefinitions.FrameByteCount -
                    sizeof(ushort) => (ushort)PaletteFxInstruction.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
