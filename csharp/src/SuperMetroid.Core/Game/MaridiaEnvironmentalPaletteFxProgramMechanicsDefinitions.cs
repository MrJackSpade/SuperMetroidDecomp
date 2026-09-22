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
/// </remarks>
public static class MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions
{
    private static readonly MaridiaEnvironmentalPaletteFxProgramDefinition[] Definitions =
    [
        new(
            MaridiaEnvironmentalPaletteOwner.SandPits,
            definitionPointer: 0xf795,
            programStart: 0xf4e9,
            firstFramePointer: 0xf4ed,
            loopInstructionPointer: 0xf53d,
            colorByteIndex: 0x0048,
            frameCount: 4,
            colorsPerFrame: 8,
            duration: 10),
        new(
            MaridiaEnvironmentalPaletteOwner.SandFalls,
            definitionPointer: 0xf799,
            programStart: 0xf541,
            firstFramePointer: 0xf545,
            loopInstructionPointer: 0xf575,
            colorByteIndex: 0x0050,
            frameCount: 4,
            colorsPerFrame: 4,
            duration: 10),
        new(
            MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls,
            definitionPointer: 0xf79d,
            programStart: 0xf579,
            firstFramePointer: 0xf57d,
            loopInstructionPointer: 0xf61d,
            colorByteIndex: 0x0068,
            frameCount: 8,
            colorsPerFrame: 8,
            duration: 2),
    ];
    private static readonly IReadOnlyList<MaridiaEnvironmentalPaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The sand-pit, sand-fall, and waterfall programs in definition order.</summary>
    public static IReadOnlyList<MaridiaEnvironmentalPaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves one compiled mechanics word across all three programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (MaridiaEnvironmentalPaletteFxProgramDefinition definition in Definitions)
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
    internal MaridiaEnvironmentalPaletteFxProgramDefinition(
        MaridiaEnvironmentalPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort firstFramePointer,
        ushort loopInstructionPointer,
        ushort colorByteIndex,
        int frameCount,
        int colorsPerFrame,
        ushort duration)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        FirstFramePointer = firstFramePointer;
        LoopInstructionPointer = loopInstructionPointer;
        ColorByteIndex = colorByteIndex;
        FrameCount = frameCount;
        ColorsPerFrame = colorsPerFrame;
        Duration = duration;
    }

    /// <summary>The environmental animation represented by this program.</summary>
    public MaridiaEnvironmentalPaletteOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The color-index setup entry for this program.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The first timed BGR555 record.</summary>
    public ushort FirstFramePointer { get; }

    /// <summary>The terminal <c>goto</c> command after the last record.</summary>
    public ushort LoopInstructionPointer { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The number of distinct timed records in one loop.</summary>
    public int FrameCount { get; }

    /// <summary>The live BGR555 words in each timed record.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The display duration shared by every record.</summary>
    public ushort Duration { get; }

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
