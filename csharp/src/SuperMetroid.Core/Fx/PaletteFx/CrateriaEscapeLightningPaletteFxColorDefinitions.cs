namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for Crateria's paired late-escape palette loops.</summary>
/// <remarks>
/// Definitions <c>$FFE9</c> and <c>$FFED</c> share an eleven-record duration schedule.
/// Their 176 BGR555 words remain live presentation data; this catalog owns palette
/// placement, timing, waits, and loop control.
/// </remarks>
public static class CrateriaEscapeLightningPaletteFxColorDefinitions
{
    /// <summary><c>PalFxDef_Crateria4</c> at <c>$8D:FFE9</c>.</summary>
    public const ushort YellowLightningDefinitionPointer = 0xffe9;
    /// <summary><c>PalFxInstList_Crateria4</c> at <c>$8D:FE01</c>.</summary>
    /// <remarks>
    /// Its 11 live BGR555 colors per frame occupy $8D:FE07 + 26 * frame
    /// + 2 * color, with frame and color each bounded to 0..10. The eleven
    /// frames reuse four distinct authored color rows in order
    /// A,B,C,D,C,A,C,A,C,D,C. All 121 words match the pinned NTSC J/U
    /// v1.0 ROM and the bank-$8D annotation. The independent color payload
    /// remains required under #1165; this catalog converts its control program only.
    /// </remarks>
    public const ushort YellowLightningProgramStart = 0xfe01;
    /// <summary><c>PalFxDef_Crateria40</c> at <c>$8D:FFED</c>.</summary>
    public const ushort CreBlockPixelDefinitionPointer = 0xffed;
    /// <summary><c>PalFxInstList_Crateria40</c> at <c>$8D:FF27</c>.</summary>
    /// <remarks>
    /// For frame 0..10 and color 0..4, the live BGR555 word at
    /// $8D:FF2D + 14 * frame + 2 * color exactly equals the yellow-lightning
    /// word at $8D:FE07 + 26 * frame + 2 * (color + 6). All 55 pairs match
    /// the pinned NTSC J/U v1.0 ROM and bank-$8D annotation: this program
    /// repeats the last five colors of each eleven-color lightning row at
    /// CGRAM byte $00AE. Frame/color bounds exclude neighboring control words;
    /// the shared color payload remains a separate conversion obligation.
    /// </remarks>
    public const ushort CreBlockPixelProgramStart = 0xff27;
    /// <summary>Both loops contain eleven timed records.</summary>
    public const int FrameCount = 11;

    /// <summary>$8D:FFE9 yellow-lightning program targets eleven colors from CGRAM byte $A2.</summary>
    private static readonly CrateriaEscapeLightningPaletteFxColorDefinition YellowLightning = new(
        CrateriaEscapeLightningPaletteOwner.YellowLightning, YellowLightningDefinitionPointer,
        YellowLightningProgramStart, 0x00a2, 11);
    /// <summary>$8D:FFED CRE pixel program targets the shared five-color tail from CGRAM byte $AE.</summary>
    private static readonly CrateriaEscapeLightningPaletteFxColorDefinition CreBlockPixel = new(
        CrateriaEscapeLightningPaletteOwner.CreBlockPixel, CreBlockPixelDefinitionPointer,
        CreBlockPixelProgramStart, 0x00ae, 5);
    /// <summary>Fixed, indexable yellow-lightning and CRE-pixel program view.</summary>
    private static readonly ProgramList Programs = new();

    /// <summary>The yellow-lightning and CRE-pixel programs in definition order.</summary>
    public static IReadOnlyList<CrateriaEscapeLightningPaletteFxColorDefinition> All => Programs;

    /// <summary>Exposes both native definition records without a mutable program array.</summary>
    private sealed class ProgramList : IReadOnlyList<CrateriaEscapeLightningPaletteFxColorDefinition>
    {
        /// <summary>Gets the number of paired late-escape palette programs.</summary>
        public int Count => 2;
        /// <summary>Gets yellow lightning before its dependent CRE pixel tail.</summary>
        public CrateriaEscapeLightningPaletteFxColorDefinition this[int index] => index switch
        {
            0 => YellowLightning,
            1 => CreBlockPixel,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Yields yellow lightning followed by the CRE block-pixel program.</summary>
        public IEnumerator<CrateriaEscapeLightningPaletteFxColorDefinition> GetEnumerator()
        { yield return YellowLightning; yield return CreBlockPixel; }
        /// <summary>Exposes the same native definition order to non-generic collection consumers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>$8D:FE05/$FF2B duration schedule: three quiet intervals between one-tick flashes.</summary>
    internal static ushort Duration(int frame) => frame switch
    {
        0 => 49,
        5 => 17,
        7 => 24,
        >= 0 and < FrameCount => 1,
        _ => throw new IndexOutOfRangeException(),
    };
}
