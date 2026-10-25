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

    /// <summary>Each phase writes five live BGR555 colors.</summary>
    public const int ColorsPerFrame = 5;

    /// <summary>Each program's owner, in definition order: the heat-phase program, then palettes four, five and six.</summary>
    private static readonly NorfairEnvironmentalPaletteOwner[] Owners =
    [
        NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase,
        NorfairEnvironmentalPaletteOwner.ForegroundPalette4,
        NorfairEnvironmentalPaletteOwner.ForegroundPalette5,
        NorfairEnvironmentalPaletteOwner.ForegroundPalette6,
    ];
    /// <summary>The definition-order position of the first regular (palette four) program.</summary>
    private const int FirstRegularProgram = 1;
    /// <summary>The palette-FX object of each program, by owner.</summary>
    private static readonly PaletteFxObject[] Objects =
    [
        PaletteFxObject.NorfairForegroundAndHeatPhase,
        PaletteFxObject.NorfairForegroundPalette4,
        PaletteFxObject.NorfairForegroundPalette5,
        PaletteFxObject.NorfairForegroundPalette6,
    ];
    /// <summary>The program each object runs, by owner: the heat-phase program, then palettes four, five and six.</summary>
    private static readonly PaletteFxProgram[] Programs =
    [
        PaletteFxProgram.Norfair21,
        PaletteFxProgram.Norfair41,
        PaletteFxProgram.Norfair81,
        PaletteFxProgram.Norfair101,
    ];
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
    /// <summary>Provides indexed definitions in native object order without storing duplicate records.</summary>
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
                NorfairEnvironmentalPaletteOwner owner = Owners[index];
                bool heat = owner == NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase;
                int regular = index - FirstRegularProgram;
                int palette = heat ? HeatPalette : RegularFirstPalette + regular;
                int color = heat ? HeatFirstColor : RegularFirstColor;
                return new(owner, Objects[index], Programs[index],
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
}
