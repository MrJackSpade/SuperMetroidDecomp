namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Maridia environmental palette program.</summary>
public enum MaridiaEnvironmentalPaletteOwner
{
    SandPits,
    SandFalls,
    BackgroundWaterfalls,
}

/// <summary>
/// Immutable control words for Maridia's sand and background-waterfall palette loops.
/// </summary>
/// <remarks>
/// Definitions <c>$F795</c>, <c>$F799</c>, and <c>$F79D</c> own 112 BGR555 color
/// words. Those colors remain presentation data; this catalog owns only color-index
/// setup, durations, waits, and loop control.
/// Sand pits set CGRAM byte $0048 and enter four ten-frame records at
/// $F4ED + 20*i, i=0..3, then $C61E goto at $F53D: a 40-frame loop.
/// Sand falls set $0050 and enter four ten-frame records at
/// $F545 + 12*i, then goto $F575: another 40-frame loop.
/// Background waterfalls set $0068 and enter eight two-frame records
/// at $F57D + 20*i, i=0..7, then goto $F61D: a 16-frame loop.
/// Each record ends in $C595 wait; each goto targets its first record.
/// The next index in each program reaches control, not presentation data.
/// All 44 mechanics words match the pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions
{
    /// <summary>$8D:F795: native Maridia1 sand-pit palette-FX definition; subsequent selected definitions occupy four bytes each.</summary>
    internal const ushort SandPitDefinition = 0xF795;
    /// <summary>$8D:F4E9: native Maridia1 color-index setup, followed contiguously by sand-fall and waterfall programs.</summary>
    internal const ushort SandPitProgram = 0xF4E9;
    /// <summary>$8D:F4EF/F547: REQUIRED four-color sand rotation group.</summary>
    internal const int SandRotationColors = 4;
    /// <summary>$8D:F4EF: REQUIRED pair of separately rotating sand-pit color bands.</summary>
    internal const int SandPitBandCount = 2;
    /// <summary>$8D:F57F: REQUIRED eight-color waterfall rotation group.</summary>
    internal const int WaterfallRotationColors = 8;
    /// <summary>$8D:F4EB/F543: REQUIRED selected sand background palette slot2.</summary>
    internal const int SandPalette = 2;
    /// <summary>$8D:F57B: REQUIRED selected waterfall background palette slot3.</summary>
    internal const int WaterfallPalette = 3;
    /// <summary>$8D:F4EB: REQUIRED first sand-pit color4 within its palette.</summary>
    internal const int SandPitFirstColor = 4;
    /// <summary>$8D:F543: REQUIRED first sand-fall color8 within its palette.</summary>
    internal const int FallingFirstColor = 8;
    /// <summary>$8D:F57B operand0068: REQUIRED waterfall first color4 in palette3; the native source comment incorrectly labels colors8..F.</summary>
    internal const int WaterfallFirstColor = 4;
    /// <summary>$8D:F4ED/F545: REQUIRED selected ten-frame sand cadence; no authored-timing exception is claimed.</summary>
    internal const ushort SandDuration = 10;
    /// <summary>$8D:F57D: REQUIRED selected two-frame waterfall cadence.</summary>
    internal const ushort WaterfallDuration = 2;

    /// <summary>The sand-pit, sand-fall, and waterfall programs in native definition order, calculated without cached records.</summary>
    public static IReadOnlyList<MaridiaEnvironmentalPaletteFxProgramDefinition> All { get; } = new DefinitionSequence();

    private sealed class DefinitionSequence : IReadOnlyList<MaridiaEnvironmentalPaletteFxProgramDefinition>
    {
        public int Count => 3;
        public MaridiaEnvironmentalPaletteFxProgramDefinition this[int index] => index switch
        {
            0 => new(MaridiaEnvironmentalPaletteOwner.SandPits),
            1 => new(MaridiaEnvironmentalPaletteOwner.SandFalls),
            2 => new(MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        public IEnumerator<MaridiaEnvironmentalPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Resolves one compiled mechanics word across all three programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (MaridiaEnvironmentalPaletteFxProgramDefinition definition in All)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }
}

/// <summary>One complete Maridia environmental palette control program.</summary>
public sealed class MaridiaEnvironmentalPaletteFxProgramDefinition
{
    internal MaridiaEnvironmentalPaletteFxProgramDefinition(MaridiaEnvironmentalPaletteOwner owner) => Owner = owner;

    /// <summary>The environmental animation represented by this program.</summary>
    public MaridiaEnvironmentalPaletteOwner Owner { get; }

    /// <summary>Native four-byte definition identity selected by environmental owner.</summary>
    public ushort DefinitionPointer => (ushort)(MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.SandPitDefinition + (int)Owner * 4);

    /// <summary>The native setup entry; each following owner begins after its predecessor's terminal goto operand.</summary>
    public ushort ProgramStart => Owner switch
    {
        MaridiaEnvironmentalPaletteOwner.SandPits => MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.SandPitProgram,
        MaridiaEnvironmentalPaletteOwner.SandFalls => (ushort)(new MaridiaEnvironmentalPaletteFxProgramDefinition(MaridiaEnvironmentalPaletteOwner.SandPits).LoopInstructionPointer + 4),
        MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls => (ushort)(new MaridiaEnvironmentalPaletteFxProgramDefinition(MaridiaEnvironmentalPaletteOwner.SandFalls).LoopInstructionPointer + 4),
        _ => throw new InvalidOperationException("Unsupported Maridia palette owner."),
    };

    /// <summary>The first timed record follows the setup opcode and its operand.</summary>
    public ushort FirstFramePointer => (ushort)(ProgramStart + 2 * sizeof(ushort));

    /// <summary>The terminal goto follows one complete rotation of the selected color group.</summary>
    public ushort LoopInstructionPointer => (ushort)(FirstFramePointer + FrameCount * FrameByteCount);

    /// <summary>Destination byte address from the independently required palette slot and first color.</summary>
    public ushort ColorByteIndex => (ushort)(sizeof(ushort) * (16 *
        (Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls
            ? MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.WaterfallPalette
            : MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.SandPalette)
        + (Owner == MaridiaEnvironmentalPaletteOwner.SandPits
            ? MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.SandPitFirstColor
            : Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls
                ? MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.WaterfallFirstColor
                : MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.FallingFirstColor)));

    /// <summary>A one-color cyclic rotation returns after the independently required group size.</summary>
    public int FrameCount => Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls
        ? MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.WaterfallRotationColors
        : MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.SandRotationColors;

    /// <summary>Sand pits rotate two groups; sand falls and waterfalls each rotate one group.</summary>
    public int ColorsPerFrame => FrameCount * (Owner == MaridiaEnvironmentalPaletteOwner.SandPits ? MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.SandPitBandCount : 1);

    /// <summary>The independently required selected cadence for sand or waterfall animation.</summary>
    public ushort Duration => Owner == MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls
        ? MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.WaterfallDuration
        : MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.SandDuration;
    /// <summary>Bytes from a duration word through its terminal wait command.</summary>
    public int FrameByteCount =>
        sizeof(ushort) + ColorsPerFrame * sizeof(ushort) + sizeof(ushort);

    /// <summary>Frames from the first record through the next first record.</summary>
    public int CycleFrames => FrameCount * Duration;

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
    /// the pinned ROM; f=8 reaches loop control. The caller retains
    /// live presentation reads throughout the sixteen-frame cycle.
    /// </remarks>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Reads one fixed mechanics word while excluding BGR555 presentation words.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        if (pointer == ProgramStart)
        {
            value = PaletteFxInstructionCodes.SetColorIndex;
            return true;
        }
        if (pointer == unchecked((ushort)(ProgramStart + sizeof(ushort))))
        {
            value = ColorByteIndex;
            return true;
        }
        if (pointer == LoopInstructionPointer)
        {
            value = PaletteFxInstructionCodes.Goto;
            return true;
        }
        if (pointer == unchecked((ushort)(LoopInstructionPointer + sizeof(ushort))))
        {
            value = FirstFramePointer;
            return true;
        }

        int frameOffset = pointer - FirstFramePointer;
        if (frameOffset >= 0 && frameOffset < FrameCount * FrameByteCount)
        {
            int inFrame = frameOffset % FrameByteCount;
            if (inFrame == 0)
            {
                value = Duration;
                return true;
            }
            if (inFrame == FrameByteCount - sizeof(ushort))
            {
                value = PaletteFxInstructionCodes.Wait;
                return true;
            }
        }

        value = 0;
        return false;
    }
}
