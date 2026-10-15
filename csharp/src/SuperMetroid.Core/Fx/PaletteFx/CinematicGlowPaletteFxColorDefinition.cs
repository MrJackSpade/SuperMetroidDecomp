namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete cinematic glow control program.</summary>
public sealed class CinematicGlowPaletteFxColorDefinition
{
    /// <summary>Binds a glow owner to its native definition, program, CGRAM destination, record width, and cadence.</summary>
    internal CinematicGlowPaletteFxColorDefinition(
        CinematicGlowPaletteFxProgramOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex,
        int colorsPerFrame,
        ushort frameDuration)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        ColorsPerFrame = colorsPerFrame;
        FrameDuration = frameDuration;
    }

    /// <summary>The mutually exclusive glow owner.</summary>
    public CinematicGlowPaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    /// <remarks>
    /// <c>$8D:E1BC</c> owns the old Mother Brain lights and <c>$8D:E1C0</c> the gunship.
    /// </remarks>
    public ushort DefinitionPointer { get; }

    /// <summary>The native instruction-list entry.</summary>
    /// <remarks>
    /// <c>$8D:C9BA</c> is the old Mother Brain loop and <c>$8D:CA4E</c> the gunship loop.
    /// </remarks>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The number of live BGR555 colors written by each timed record.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The cartridge-authored duration of each timed record.</summary>
    public ushort FrameDuration { get; }

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public int FrameByteCount => (ColorsPerFrame + 2) * sizeof(ushort);

    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= CinematicGlowPaletteFxColorDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one presentation-owned BGR555 word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }
}
