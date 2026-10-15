namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Maridia environmental palette control program.</summary>
public sealed class MaridiaEnvironmentalPaletteFxColorDefinition
{
    /// <summary>Selects one of the three mutually exclusive Maridia palette programs.</summary>
    internal MaridiaEnvironmentalPaletteFxColorDefinition(MaridiaEnvironmentalPaletteOwner owner) => Owner = owner;

    /// <summary>The environmental animation represented by this program.</summary>
    public MaridiaEnvironmentalPaletteOwner Owner { get; }

    /// <summary>The native setup entry; each following owner begins after its predecessor's terminal goto operand.</summary>
    public ushort ProgramStart => Owner switch
    {
        MaridiaEnvironmentalPaletteOwner.SandPits => MaridiaEnvironmentalPaletteFxColorDefinitions.SandPitProgram,
        MaridiaEnvironmentalPaletteOwner.SandFalls => (ushort)(new MaridiaEnvironmentalPaletteFxColorDefinition(MaridiaEnvironmentalPaletteOwner.SandPits).LoopInstructionPointer + 4),
        MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls => (ushort)(new MaridiaEnvironmentalPaletteFxColorDefinition(MaridiaEnvironmentalPaletteOwner.SandFalls).LoopInstructionPointer + 4),
        _ => throw new InvalidOperationException("Unsupported Maridia palette owner."),
    };

    /// <summary>The first timed record follows the setup opcode and its operand.</summary>
    public ushort FirstFramePointer => (ushort)(ProgramStart + 2 * sizeof(ushort));

    /// <summary>The terminal goto follows one complete rotation of the selected color group.</summary>
    public ushort LoopInstructionPointer => (ushort)(FirstFramePointer + FrameCount * FrameByteCount);

    /// <summary>A one-color cyclic rotation returns after the named group size.</summary>
    public int FrameCount => Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls
        ? MaridiaEnvironmentalPaletteFxColorDefinitions.WaterfallRotationColors
        : MaridiaEnvironmentalPaletteFxColorDefinitions.SandRotationColors;

    /// <summary>Sand pits rotate two groups; sand falls and waterfalls each rotate one group.</summary>
    public int ColorsPerFrame => FrameCount * (Owner == MaridiaEnvironmentalPaletteOwner.SandPits ? MaridiaEnvironmentalPaletteFxColorDefinitions.SandPitBandCount : 1);
    /// <summary>Bytes from a duration word through its terminal wait command.</summary>
    public int FrameByteCount =>
        sizeof(ushort) + ColorsPerFrame * sizeof(ushort) + sizeof(ushort);

    /// <summary>Returns the timed-record pointer for one zero-based cycle frame.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one contiguous presentation-color address within a frame.</summary>
    /// <remarks>
    /// For sand pits, frame f=0..3 and color c=0..7 address
    /// $F4EF + 20*f + 2*c. The base row has two four-color groups:
    /// ($3ED9,$2E57,$2A35,$25F3) and
    /// ($25D2,$1DB0,$196E,$112E). Within each group, select
    /// (c mod 4 + f) mod 4; the group is floor(c/4).
    /// Sand falls address $F547 + 12*f + 2*c for c=0..3 and use
    /// the same second group, rotated left by f. All 48 words match
    /// the pinned NTSC J/U v1.0 ROM. Both callers read these colors
    /// as live presentation data; f=4 reaches each loop command.
    /// Background waterfalls use base BGR555 row
    /// ($0400,$0C22,$1864,$2086,$2CC9,$1C65,$1043,$0821).
    /// For frame f=0..7 and color c=0..7, the word at
    /// $F57F + 20*f + 2*c is base[(c+f) mod 8]. All 64 words match
    /// the pinned ROM; f=8 reaches loop control. The
    /// installed presentation view calculates these rotations and the shared sand band
    /// from 16 authored first-row colors, preserving every supplied edit.
    /// </remarks>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }
}
