namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive late Crateria escape palette program.</summary>
public enum CrateriaEscapeLightningPaletteOwner
{
    YellowLightning,
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
    /// v1.0 ROM and the bank-$8D annotation. Values within each row vary
    /// by color slot without a simpler justified lossless numeric rule;
    /// retain the authored presentation content and its live reads.
    /// </remarks>
    public const ushort YellowLightningProgramStart = 0xfe01;
    /// <summary><c>PalFxDef_Crateria40</c> at <c>$8D:FFED</c>.</summary>
    public const ushort CreBlockPixelDefinitionPointer = 0xffed;
    /// <summary><c>PalFxInstList_Crateria40</c> at <c>$8D:FF27</c>.</summary>
    public const ushort CreBlockPixelProgramStart = 0xff27;
    /// <summary>Both loops contain eleven timed records.</summary>
    public const int FrameCount = 11;
    /// <summary>Both complete loops last 98 frames.</summary>
    public const int CycleFrames = 98;

    /// <summary>Shared eleven-record late-Crateria escape duration schedule.</summary>
    /// <remarks>
    /// For the only valid frame indices 0..10, duration is 49 at frame zero,
    /// 17 at frame five, 24 at frame seven, and one otherwise. This sparse
    /// rule totals 98 frames. Both native streams use it: duration words are
    /// at $8D:FE05 + 26 * frame for yellow lightning and $8D:FF2B +
    /// 14 * frame for the CRE pixel. All 22 words match the pinned NTSC
    /// J/U v1.0 ROM. Their terminal gotos target $FE05 and $FF2B respectively;
    /// frame eleven would be control, not a twelfth duration.
    /// </remarks>
    private static readonly ushort[] Durations = [49, 1, 1, 1, 1, 17, 1, 24, 1, 1, 1];
    private static readonly CrateriaEscapeLightningPaletteFxProgramDefinition[] Definitions =
    [
        new(CrateriaEscapeLightningPaletteOwner.YellowLightning,
            YellowLightningDefinitionPointer, YellowLightningProgramStart, 0x00a2, 11),
        new(CrateriaEscapeLightningPaletteOwner.CreBlockPixel,
            CreBlockPixelDefinitionPointer, CreBlockPixelProgramStart, 0x00ae, 5),
    ];
    private static readonly IReadOnlyList<CrateriaEscapeLightningPaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The yellow-lightning and CRE-pixel programs in definition order.</summary>
    public static IReadOnlyList<CrateriaEscapeLightningPaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves one compiled mechanics word across both programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (CrateriaEscapeLightningPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }
        value = 0;
        return false;
    }

    internal static ushort Duration(int frame) => Durations[frame];
}

/// <summary>One complete late-Crateria escape palette control program.</summary>
public sealed class CrateriaEscapeLightningPaletteFxProgramDefinition
{
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
