namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive Norfair environmental palette program.</summary>
public enum NorfairEnvironmentalPaletteOwner
{
    /// <summary>$8D:F785, Norfair 2: InstList_PaletteFXObject_Norfair2_0 at $F08E updates BG palette 3 and publishes each phase through $F1C6 for Samus's separate heat palette.</summary>
    ForegroundAndHeatPhase,
    /// <summary>$8D:F789, Norfair 4: InstList_PaletteFXObject_Norfair4_0 at $F1D1 updates BG palette 4 from color 1 without publishing a heat phase.</summary>
    ForegroundPalette4,
    /// <summary>$8D:F78D, Norfair 8: InstList_PaletteFXObject_Norfair8_0 at $F2D9 updates BG palette 5 from color 1; the native 8 names the definition, not the palette number.</summary>
    ForegroundPalette5,
    /// <summary>$8D:F791, Norfair 10h: InstList_PaletteFXObject_Norfair10_0 at $F3E1 updates BG palette 6 from color 1 using its own five-color artwork rows.</summary>
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

    /// <summary>$8D:F785: first of four adjacent native environmental definitions, each init/list pair occupying four bytes.</summary>
    private const ushort FirstDefinition = 0xf785;
    /// <summary>$8D:F08E: foreground palette-three loop also publishes the current heat phase.</summary>
    private const ushort HeatPhaseProgram = 0xf08e;
    /// <summary>$8D:F1D1: foreground palette-four loop; palette-five/six programs follow with the same record geometry.</summary>
    private const ushort FirstRegularProgram = 0xf1d1;
    /// <summary>Each program begins with a color-index command/operand and ends with a goto command/operand.</summary>
    private const int SetupBytes = 4, GotoBytes = 4;
    /// <summary>Regular phases contain duration, five colors, one skip command and one wait command.</summary>
    private const int RegularFrameBytes = 2 + ColorsPerFrame * 2 + 2 + 2;
    /// <summary>Heat publication adds an instruction word and its byte-sized phase operand before each regular record.</summary>
    private const int HeatPublicationBytes = 3;
    /// <summary>$8D:F08E's $006A color index starts at palette three/color five.</summary>
    private const int HeatPalette = 3, HeatFirstColor = 5;
    /// <summary>$8D:F1D1/F2D9/F3E1 select color one in foreground palettes four/five/six.</summary>
    private const int RegularFirstPalette = 4, RegularFirstColor = 1;

    /// <summary>
    /// Lazily indexed view of the four Norfair palette programs, built from their shared phase layout.
    /// </summary>
    private static readonly ProgramDefinitions Definitions = new();

    /// <summary>
    /// Provides the four program definitions without allocating a separate array of records.
    /// </summary>
    private sealed class ProgramDefinitions : IReadOnlyList<NorfairEnvironmentalPaletteFxProgramDefinition>
    {
        /// <summary>
        /// Gets the fixed number of Norfair environmental palette programs represented by this view.
        /// </summary>
        public int Count => 4;

        /// <summary>
        /// Gets the definition at its owner-order index and rejects indices outside the four-program catalog.
        /// </summary>
        /// <param name="index">The zero-based owner-order index of the requested program.</param>
        /// <returns>The definition whose owner corresponds to <paramref name="index"/>.</returns>
        public NorfairEnvironmentalPaletteFxProgramDefinition this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new ArgumentOutOfRangeException(nameof(index));
                var owner = (NorfairEnvironmentalPaletteOwner)index;
                bool heat = owner == NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase;
                int regular = index - (int)NorfairEnvironmentalPaletteOwner.ForegroundPalette4;
                int start = heat ? HeatPhaseProgram : FirstRegularProgram + regular *
                    (SetupBytes + FrameCount * RegularFrameBytes + GotoBytes);
                int first = start + SetupBytes;
                int palette = heat ? HeatPalette : RegularFirstPalette + regular;
                int color = heat ? HeatFirstColor : RegularFirstColor;
                return new(owner, (ushort)(FirstDefinition + index * 4), (ushort)start, (ushort)first,
                    (ushort)(first + FrameCount * (RegularFrameBytes + (heat ? HeatPublicationBytes : 0))),
                    (ushort)((palette * 16 + color) * sizeof(ushort)), heat);
            }
        }
        /// <summary>
        /// Enumerates the four definitions in the same order as <see cref="NorfairEnvironmentalPaletteOwner"/>.
        /// </summary>
        /// <returns>An enumerator over the owner-ordered program definitions.</returns>
        public IEnumerator<NorfairEnvironmentalPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Returns a non-generic enumerator over this sequence.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>The four real environmental programs in definition order.</summary>
    public static IReadOnlyList<NorfairEnvironmentalPaletteFxProgramDefinition> All =>
        Definitions;

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
    internal static ushort Duration(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new IndexOutOfRangeException();
        int distance = Math.Min(frame, FrameCount - 1 - frame);
        return (ushort)(distance == 0 ? 16 : Math.Clamp(distance + 2, 4, 8));
    }
}

/// <summary>One complete Norfair environmental palette control program.</summary>
public sealed class NorfairEnvironmentalPaletteFxProgramDefinition
{
    /// <summary>
    /// Number of leading bytes in each heat-publishing record before its duration and color payload.
    /// </summary>
    private const int HeatPhasePublicationByteCount = 3;

    /// <summary>
    /// Number of colors stored before the palette-FX skip command in each phase record.
    /// </summary>
    private const int LeadingColorCount = 3;

    /// <summary>
    /// Byte displacement from a regular record's start to its first color word.
    /// </summary>
    private const int LeadingColorsOffset = 2;

    /// <summary>
    /// Byte displacement from a regular record's start to its trailing color words.
    /// </summary>
    private const int TrailingColorsOffset = 10;

    /// <summary>
    /// Describes one environmental palette program's owner, instruction boundaries, color destination, and heat-phase behavior.
    /// </summary>
    /// <param name="owner">The environmental palette object that owns the program.</param>
    /// <param name="definitionPointer">The native definition identity used to install this program.</param>
    /// <param name="programStart">The address of the program's color-index setup command.</param>
    /// <param name="firstFramePointer">The address of the first timed phase record.</param>
    /// <param name="loopInstructionPointer">The address of the terminal goto command that restarts the phase loop.</param>
    /// <param name="colorByteIndex">The first CGRAM byte updated by the program's color commands.</param>
    /// <param name="publishesHeatPhase">Whether each phase begins by publishing its byte-sized index for Samus's separate heat palette.</param>
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
    /// All 144 words match the pinned NTSC J/U v1.0 ROM. These live colors
    /// still require independent derivation under issue #1165.
    /// For the heat-phase owner, trailing color 3 at $F092 + 19*f + 13
    /// equals leading color 0 at offset 5 for every f=0..15. Trailing
    /// color 4 at offset 15 uses authored values by d=min(f,15-f):
    /// $4A52,$4214,$39F5,$31D7,$29D9,$21BA,$199C,$0D7F.
    /// All 32 trailing words match the pinned ROM; the color-4
    /// gradient remains pending independent derivation under issue #1165.
    /// For palette-4, trailing colors 3..4 at $F1D5 + 16*f + 10 and +12
    /// use authored pairs by d=min(f,15-f): ($4309,$0C77),
    /// ($36AC,$0CB8), ($328F,$1119), ($2A52,$157A),
    /// ($2214,$15BB), ($1DF7,$1A1C), ($15BA,$1E7D),
    /// ($0D7F,$22FF). All 32 ROM words match; these pairs
    /// remain pending independent derivation under issue #1165.
    /// For palette-5, trailing colors 3..4 at $F2DD + 16*f + 10 and +12
    /// use authored pairs by d=min(f,15-f): ($2DB3,$38CF),
    /// ($2594,$30D1), ($2176,$28D3), ($1D57,$24D5),
    /// ($1959,$20F7), ($153B,$18F9), ($111C,$14FB),
    /// ($0D1F,$0D1F). All 32 ROM words match; these pairs
    /// remain pending independent derivation under issue #1165.
    /// Palette-6 has its own five-color rows at $F3E5 + 16*f, with
    /// color offsets 2,4,6,10,12. For d=min(f,15-f), rows d=0..7 are
    /// ($09DA,$091A,$087A,$08A8,$0C05),
    /// ($0DDA,$093A,$089A,$08AA,$0828),
    /// ($0DFA,$0D5A,$08BA,$08AC,$084A),
    /// ($11FA,$0D7A,$08FA,$08CF,$086D),
    /// ($161A,$119A,$0D1A,$08D1,$0890),
    /// ($1A1A,$11BA,$0D3A,$08F4,$08B3),
    /// ($1A3A,$15DA,$0D7A,$08F6,$08D5),
    /// ($225A,$1A1A,$11BA,$091A,$091A).
    /// All 80 words match the pinned ROM; these colors
    /// remain pending independent derivation under issue #1165.
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
