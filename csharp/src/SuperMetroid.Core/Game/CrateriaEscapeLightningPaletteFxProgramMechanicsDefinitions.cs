namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive late Crateria escape palette program.</summary>
public enum CrateriaEscapeLightningPaletteOwner
{
    /// <summary>PalFxDef_Crateria4 ($8D:FFE9), running the eleven-color yellow-lightning loop from $FE01 at CGRAM byte $A2.</summary>
    YellowLightning,
    /// <summary>PalFxDef_Crateria40 ($8D:FFED), running the five-color CRE-block pixel tail from $FF27 at CGRAM byte $AE with the lightning cadence.</summary>
    CreBlockPixel,
}

/// <summary>Immutable mechanics for Crateria's paired late-escape palette loops.</summary>
/// <remarks>
/// Definitions <c>$FFE9</c> and <c>$FFED</c> share an eleven-record duration schedule.
/// Their 176 BGR555 words remain live presentation data; this catalog owns palette
/// placement, timing, waits, and loop control.
/// </remarks>
public static class CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions
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
    private static readonly CrateriaEscapeLightningPaletteFxProgramDefinition YellowLightning = new(
        CrateriaEscapeLightningPaletteOwner.YellowLightning, YellowLightningDefinitionPointer,
        YellowLightningProgramStart, 0x00a2, 11);
    /// <summary>$8D:FFED CRE pixel program targets the shared five-color tail from CGRAM byte $AE.</summary>
    private static readonly CrateriaEscapeLightningPaletteFxProgramDefinition CreBlockPixel = new(
        CrateriaEscapeLightningPaletteOwner.CreBlockPixel, CreBlockPixelDefinitionPointer,
        CreBlockPixelProgramStart, 0x00ae, 5);
    /// <summary>Stable read-only list backing the public catalog of the two compiled program definitions.</summary>
    private static readonly ProgramList Programs = new();

    /// <summary>The yellow-lightning and CRE-pixel programs in definition order.</summary>
    public static IReadOnlyList<CrateriaEscapeLightningPaletteFxProgramDefinition> All => Programs;

    /// <summary>Provides indexed and sequential access to the yellow-lightning and CRE-pixel definitions.</summary>
    private sealed class ProgramList : IReadOnlyList<CrateriaEscapeLightningPaletteFxProgramDefinition>
    {
        /// <summary>Gets the two compiled room variants.</summary>
        public int Count => 2;

        /// <summary>Gets the yellow-lightning definition first and the CRE-pixel definition second.</summary>
        /// <param name="index">Zero-based definition position, either zero or one.</param>
        public CrateriaEscapeLightningPaletteFxProgramDefinition this[int index] => index switch
        {
            0 => YellowLightning,
            1 => CreBlockPixel,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Enumerates the two definitions in the same order as the indexer.</summary>
        public IEnumerator<CrateriaEscapeLightningPaletteFxProgramDefinition> GetEnumerator()
        { yield return YellowLightning; yield return CreBlockPixel; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Resolves one compiled mechanics word across both programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value) =>
        YellowLightning.TryReadMechanicsWord(pointer, out value) ||
        CreBlockPixel.TryReadMechanicsWord(pointer, out value);
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

/// <summary>One complete late-Crateria escape palette control program.</summary>
public sealed class CrateriaEscapeLightningPaletteFxProgramDefinition
{
    /// <summary>Creates the control-program metadata needed to locate its setup, timed records, and palette payload.</summary>
    /// <param name="owner">Palette program variant associated with the definition.</param>
    /// <param name="definitionPointer">Native bank-$8D palette-FX definition that selects this program.</param>
    /// <param name="programStart">Address of the program's color-index setup instruction.</param>
    /// <param name="colorByteIndex">First destination byte in CGRAM for the live colors.</param>
    /// <param name="colorsPerFrame">Number of BGR555 color words carried by each timed record.</param>
    internal CrateriaEscapeLightningPaletteFxProgramDefinition(
        CrateriaEscapeLightningPaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex,
        int colorsPerFrame)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        ColorsPerFrame = colorsPerFrame;
    }

    /// <summary>The mutually exclusive palette owner.</summary>
    public CrateriaEscapeLightningPaletteOwner Owner { get; }
    /// <summary>The palette-FX definition identity that installs this program.</summary>
    public ushort DefinitionPointer { get; }
    /// <summary>The color-index setup entry.</summary>
    public ushort ProgramStart { get; }
    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }
    /// <summary>The number of live BGR555 colors in every record.</summary>
    public int ColorsPerFrame { get; }
    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));
    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public int FrameByteCount => sizeof(ushort) * (ColorsPerFrame + 2);
    /// <summary>The terminal <c>goto</c> command after all timed records.</summary>
    public ushort LoopInstructionPointer => unchecked((ushort)(FirstFramePointer +
        CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.FrameCount * FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one live BGR555 color word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
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
        for (int frame = 0; frame < CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.FrameCount; frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => CrateriaEscapeLightningPaletteFxProgramMechanicsDefinitions.Duration(frame),
                var item when item == FrameByteCount - sizeof(ushort) => PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }
        return false;
    }
}
