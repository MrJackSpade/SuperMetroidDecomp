namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Norfair environmental palette program.</summary>
public enum NorfairEnvironmentalPaletteOwner
{
    ForegroundAndHeatPhase,
    ForegroundPalette4,
    ForegroundPalette5,
    ForegroundPalette6,
}

/// <summary>Immutable mechanics for Norfair's four synchronized environmental loops.</summary>
/// <remarks>
/// Definitions <c>$F785-$F791</c> share sixteen authored phases. Their 320 BGR555 words
/// remain presentation data. The first owner additionally publishes sixteen byte-sized
/// heat-phase operands consumed by the separate Samus-in-heat palette object.
/// </remarks>
public static class NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions
{
    /// <summary>All four programs contain sixteen phases.</summary>
    public const int FrameCount = 16;

    /// <summary>Each phase writes five live BGR555 colors.</summary>
    public const int ColorsPerFrame = 5;

    /// <summary>The shared complete-cycle duration.</summary>
    public const int CycleFrames = 116;

    private static readonly ushort[] Durations =
        [16, 4, 4, 5, 6, 7, 8, 8, 8, 8, 7, 6, 5, 4, 4, 16];

    private static readonly NorfairEnvironmentalPaletteFxProgramDefinition[] Definitions =
    [
        new(NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase,
            0xf785, 0xf08e, 0xf092, 0xf1c2, 0x006a, publishesHeatPhase: true),
        new(NorfairEnvironmentalPaletteOwner.ForegroundPalette4,
            0xf789, 0xf1d1, 0xf1d5, 0xf2d5, 0x0082, publishesHeatPhase: false),
        new(NorfairEnvironmentalPaletteOwner.ForegroundPalette5,
            0xf78d, 0xf2d9, 0xf2dd, 0xf3dd, 0x00a2, publishesHeatPhase: false),
        new(NorfairEnvironmentalPaletteOwner.ForegroundPalette6,
            0xf791, 0xf3e1, 0xf3e5, 0xf4e5, 0x00c2, publishesHeatPhase: false),
    ];
    private static readonly IReadOnlyList<NorfairEnvironmentalPaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The four real environmental programs in definition order.</summary>
    public static IReadOnlyList<NorfairEnvironmentalPaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves one compiled word-sized mechanic across all four programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (NorfairEnvironmentalPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }

    /// <summary>Resolves one compiled heat-phase byte from the first program.</summary>
    public static bool TryReadMechanicsByte(ushort pointer, out byte value)
    {
        NorfairEnvironmentalPaletteFxProgramDefinition definition = Definitions[0];
        for (int frame = 0; frame < FrameCount; frame++)
        {
            if (pointer != unchecked((ushort)(definition.FramePointer(frame) + 2)))
                continue;
            value = unchecked((byte)frame);
            return true;
        }

        value = 0;
        return false;
    }

    internal static ushort Duration(int frame) => Durations[frame];
}

/// <summary>One complete Norfair environmental palette control program.</summary>
public sealed class NorfairEnvironmentalPaletteFxProgramDefinition
{
    private const int HeatPhasePublicationByteCount = 3;
    private const int LeadingColorCount = 3;
    private const int LeadingColorsOffset = 2;
    private const int TrailingColorsOffset = 10;

    internal NorfairEnvironmentalPaletteFxProgramDefinition(
        NorfairEnvironmentalPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort firstFramePointer,
        ushort loopInstructionPointer,
        ushort colorByteIndex,
        bool publishesHeatPhase)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        FirstFramePointer = firstFramePointer;
        LoopInstructionPointer = loopInstructionPointer;
        ColorByteIndex = colorByteIndex;
        PublishesHeatPhase = publishesHeatPhase;
    }

    /// <summary>The mutually exclusive environmental palette owner.</summary>
    public NorfairEnvironmentalPaletteOwner Owner { get; }
    /// <summary>The palette-FX definition identity that installs this program.</summary>
    public ushort DefinitionPointer { get; }
    /// <summary>The color-index setup entry.</summary>
    public ushort ProgramStart { get; }
    /// <summary>The first mixed-width timed record.</summary>
    public ushort FirstFramePointer { get; }
    /// <summary>The terminal <c>goto</c> command.</summary>
    public ushort LoopInstructionPointer { get; }
    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }
    /// <summary>Whether each record begins with a byte-sized heat-phase publication.</summary>
    public bool PublishesHeatPhase { get; }
    /// <summary>Bytes from one record's first mechanic through its wait command.</summary>
    public int FrameByteCount => PublishesHeatPhase ? 19 : 16;
    /// <summary>Returns one mixed-width record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one interleaved presentation-color address within a frame.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        int mechanicsPrefix = PublishesHeatPhase ? HeatPhasePublicationByteCount : 0;
        int colorOffset = color < LeadingColorCount
            ? mechanicsPrefix + LeadingColorsOffset + color * sizeof(ushort)
            : mechanicsPrefix + TrailingColorsOffset +
                (color - LeadingColorCount) * sizeof(ushort);
        return unchecked((ushort)(FramePointer(frame) + colorOffset));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            var item when item == ProgramStart + 2 => ColorByteIndex,
            var item when item == LoopInstructionPointer => PaletteFxInstructionCodes.Goto,
            var item when item == LoopInstructionPointer + 2 => FirstFramePointer,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0;
             frame < NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            int durationOffset = PublishesHeatPhase ? 3 : 0;
            value = offset switch
            {
                0 when PublishesHeatPhase => PaletteFxInstructionCodes.SetPaletteFxIndex,
                var item when item == durationOffset =>
                    NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.Duration(frame),
                var item when item == durationOffset + 8 => PublishesHeatPhase
                    ? PaletteFxInstructionCodes.ColorPlus4
                    : PaletteFxInstructionCodes.ColorPlus8,
                var item when item == durationOffset + 14 => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
