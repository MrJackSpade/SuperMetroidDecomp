namespace SuperMetroid.Core.Fx.PaletteFx;

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
public static class NorfairEnvironmentalPaletteFxColorDefinitions
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
    /// <summary>The color-index setup command and operand occupy four bytes.</summary>
    private const int SetupBytes = 4;
    /// <summary>The loop's goto command and destination occupy four bytes.</summary>
    private const int GotoBytes = 4;
    /// <summary>Regular phases contain duration, five colors, one skip command and one wait command.</summary>
    private const int RegularFrameBytes = 2 + ColorsPerFrame * 2 + 2 + 2;
    /// <summary>Heat publication adds an instruction word and its byte-sized phase operand before each regular record.</summary>
    private const int HeatPublicationBytes = 3;
    /// <summary>$8D:F08E publishes heat phases while writing foreground palette three.</summary>
    private const int HeatPalette = 3;
    /// <summary>The heat program's $006A CGRAM index starts at color five.</summary>
    private const int HeatFirstColor = 5;
    /// <summary>The first regular program writes foreground palette four; later programs advance the palette.</summary>
    private const int RegularFirstPalette = 4;
    /// <summary>All three regular programs start their writes at color one.</summary>
    private const int RegularFirstColor = 1;

    /// <summary>Materializes the four native programs from their shared record geometry.</summary>
    private static readonly ProgramDefinitions Definitions = new();
    /// <summary>Provides indexed definitions in adjacent native pointer order without storing duplicate records.</summary>
    private sealed class ProgramDefinitions : IReadOnlyList<NorfairEnvironmentalPaletteFxColorDefinition>
    {
        /// <summary>The four adjacent definition identities at $8D:F785 through $F791.</summary>
        public int Count => 4;
        /// <summary>Calculates one program's entry, frame, loop, and CGRAM addresses from its owner index.</summary>
        public NorfairEnvironmentalPaletteFxColorDefinition this[int index]
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
        /// <summary>Visits the heat-phase program followed by palette-four, five, and six programs.</summary>
        public IEnumerator<NorfairEnvironmentalPaletteFxColorDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Exposes the same native-order sequence through the nongeneric collection contract.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>The four real environmental programs in definition order.</summary>
    public static IReadOnlyList<NorfairEnvironmentalPaletteFxColorDefinition> All =>
        Definitions;

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
