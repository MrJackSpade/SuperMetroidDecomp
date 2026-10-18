namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>One complete Norfair environmental palette control program.</summary>
public sealed class NorfairEnvironmentalPaletteFxColorDefinition
{
    /// <summary>Each heat phase begins with a word instruction and one byte phase operand.</summary>
    private const int HeatPhasePublicationByteCount = 3;
    /// <summary>Three colors precede the skip command in each palette record.</summary>
    private const int LeadingColorCount = 3;
    /// <summary>The first color follows the frame's two-byte duration.</summary>
    private const int LeadingColorsOffset = 2;
    /// <summary>Two trailing colors follow duration, leading colors, and skip command.</summary>
    private const int TrailingColorsOffset = 10;

    /// <summary>Captures the compiled pointer geometry and destination for one environmental loop.</summary>
    internal NorfairEnvironmentalPaletteFxColorDefinition(
        NorfairEnvironmentalPaletteOwner owner,
        PaletteFxObject paletteFxObject,
        ushort programStart,
        ushort firstFramePointer,
        ushort loopInstructionPointer,
        ushort colorByteIndex,
        bool publishesHeatPhase)
    {
        Owner = owner;
        Object = paletteFxObject;
        ProgramStart = programStart;
        FirstFramePointer = firstFramePointer;
        LoopInstructionPointer = loopInstructionPointer;
        ColorByteIndex = colorByteIndex;
        PublishesHeatPhase = publishesHeatPhase;
    }

    /// <summary>The mutually exclusive environmental palette owner.</summary>
    public NorfairEnvironmentalPaletteOwner Owner { get; }
    /// <summary>The palette-FX object that installs this program.</summary>
    public PaletteFxObject Object { get; }
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
        if ((uint)frame >= NorfairEnvironmentalPaletteFxColorDefinitions.FrameCount)
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
        if ((uint)color >= NorfairEnvironmentalPaletteFxColorDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        int mechanicsPrefix = PublishesHeatPhase ? HeatPhasePublicationByteCount : 0;
        int colorOffset = color < LeadingColorCount
            ? mechanicsPrefix + LeadingColorsOffset + color * sizeof(ushort)
            : mechanicsPrefix + TrailingColorsOffset +
                (color - LeadingColorCount) * sizeof(ushort);
        return unchecked((ushort)(FramePointer(frame) + colorOffset));
    }
}
