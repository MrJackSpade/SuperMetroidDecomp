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
/// $F785 enters $F092 + 19*i, i=0..15: $F1C6 publishes phase byte i,
/// then a duration, three colors, $C5AB skip of four CGRAM colors, two
/// colors, and $C595 wait. The other definitions begin records at
/// $F1D5, $F2DD, or $F3E5 plus 16*i; they omit the phase publication
/// and use $C5B4 to skip eight colors. Their CGRAM byte indices are $006A, $0082, $00A2,
/// and $00C2 respectively. Each terminal $C61E goto returns to its
/// first record after a 116-frame cycle; i=16 reaches control. All 224
/// mechanics words and sixteen phase bytes match the pinned NTSC J/U
/// v1.0 ROM and the guarded production caller.
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

    /// <summary>Shared duration for one bounded Norfair palette phase.</summary>
    /// <remarks>
    /// For i=0..15, let d=min(i,15-i). The duration is 16 when d=0;
    /// otherwise min(8,max(4,d+2)). This gives the exact symmetric
    /// 16,4,4,5,6,7,8,8,8,8,7,6,5,4,4,16 ROM schedule, totaling 116 frames.
    /// </remarks>
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
    /// <remarks>
    /// For the first three owners and leading colors c=0..2, phase
    /// f=0..15 selects authored row d=min(f,15-f). Rows d=0..7 are
    /// ($09FD,$093B,$0459), ($0E3D,$0D7C,$089A),
    /// ($165E,$0DBC,$08FB), ($1A9E,$11FD,$0D3C),
    /// ($1EBE,$161D,$119C), ($22FE,$1A5E,$15DD),
    /// ($2B1F,$1A9E,$163E), ($2F5F,$1EDF,$1A7F).
    /// The heat-phase owner places them at $F092 + 19*f + 5 + 2*c;
    /// palette-4 and palette-5 owners use base $F1D5 or $F2DD,
    /// respectively, plus 16*f + 2 + 2*c.
    /// All 144 words match the pinned NTSC J/U v1.0 ROM. Their irregular
    /// channel steps remain authored, live presentation colors.
    /// For the heat-phase owner, trailing color 3 at $F092 + 19*f + 13
    /// equals leading color 0 at offset 5 for every f=0..15. Trailing
    /// color 4 at offset 15 uses authored values by d=min(f,15-f):
    /// $4A52,$4214,$39F5,$31D7,$29D9,$21BA,$199C,$0D7F.
    /// All 32 trailing words match the pinned ROM; the irregular color-4
    /// gradient remains live rather than generated at runtime.
    /// For palette-4, trailing colors 3..4 at $F1D5 + 16*f + 10 and +12
    /// use authored pairs by d=min(f,15-f): ($4309,$0C77),
    /// ($36AC,$0CB8), ($328F,$1119), ($2A52,$157A),
    /// ($2214,$15BB), ($1DF7,$1A1C), ($15BA,$1E7D),
    /// ($0D7F,$22FF). All 32 ROM words match; the irregular pairs
    /// remain live presentation data.
    /// </remarks>
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
