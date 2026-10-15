namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete cinematic PLANET ZEBES text-fade control program.</summary>
public sealed class PlanetZebesTextPaletteFxColorDefinition
{
    /// <summary>Selects one of the mutually exclusive title-text fade directions.</summary>
    internal PlanetZebesTextPaletteFxColorDefinition(PlanetZebesTextPaletteFxProgramOwner owner) => Owner = owner;

    /// <summary>The mutually exclusive text-fade owner.</summary>
    public PlanetZebesTextPaletteFxProgramOwner Owner { get; }

    /// <summary>Fade-out begins after fade-in's setup, timed records and final delete word.</summary>
    public ushort ProgramStart => (ushort)(PlanetZebesTextPaletteFxColorDefinitions.FadeInProgram
        + (Owner == PlanetZebesTextPaletteFxProgramOwner.FadeOut
            ? 3 * sizeof(ushort) + PlanetZebesTextPaletteFxColorDefinitions.FrameCount
                * PlanetZebesTextPaletteFxColorDefinitions.FrameByteCount : 0));
    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= PlanetZebesTextPaletteFxColorDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame *
            PlanetZebesTextPaletteFxColorDefinitions.FrameByteCount));
    }

    /// <summary>Returns one presentation-owned BGR555 word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= PlanetZebesTextPaletteFxColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }
}
