namespace SuperMetroid.Core.Game;

/// <summary>One complete cinematic PLANET ZEBES text-fade control program.</summary>
public sealed class PlanetZebesTextPaletteFxProgramDefinition
{
    /// <summary>Selects one of the mutually exclusive title-text fade directions.</summary>
    internal PlanetZebesTextPaletteFxProgramDefinition(PlanetZebesTextPaletteFxProgramOwner owner) => Owner = owner;

    /// <summary>The mutually exclusive text-fade owner.</summary>
    public PlanetZebesTextPaletteFxProgramOwner Owner { get; }

    /// <summary>Fade-out begins after fade-in's setup, timed records and final delete word.</summary>
    public ushort ProgramStart => (ushort)(PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FadeInProgram
        + (Owner == PlanetZebesTextPaletteFxProgramOwner.FadeOut
            ? 3 * sizeof(ushort) + PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount
                * PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameByteCount : 0));

    /// <summary>Byte destination from the independently required text palette and first color.</summary>
    public ushort ColorByteIndex => Owner switch
    {
        PlanetZebesTextPaletteFxProgramOwner.FadeIn or PlanetZebesTextPaletteFxProgramOwner.FadeOut =>
            (ushort)(sizeof(ushort) * (16 * PlanetZebesTextPaletteFxProgramMechanicsDefinitions.TextPalette
                + PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FirstTextColor)),
        _ => throw new InvalidOperationException("Unsupported PLANET ZEBES fade owner."),
    };
    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>The terminal <c>delete</c> command after all timed records.</summary>
    public ushort DeleteInstructionPointer => unchecked((ushort)(FirstFramePointer +
        PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount *
        PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame *
            PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Returns one presentation-owned BGR555 word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= PlanetZebesTextPaletteFxProgramMechanicsDefinitions.ColorsPerFrame)
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
             frame < PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameDuration,
                PlanetZebesTextPaletteFxProgramMechanicsDefinitions.FrameByteCount -
                    sizeof(ushort) => (ushort)PaletteFxInstruction.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
