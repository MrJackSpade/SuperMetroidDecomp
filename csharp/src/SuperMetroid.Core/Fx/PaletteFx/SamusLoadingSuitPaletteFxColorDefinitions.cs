namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics shared by the three Samus-loading palette programs.</summary>
/// <remarks>
/// Issue #855 / #625: pinned NTSC J/U v1.0 ROM definitions <c>$8D:E1F4</c>,
/// <c>$E1F8</c>, and <c>$E1FC</c> enter at <c>$8D:DB62</c>, <c>$DCC8</c>,
/// and <c>$DE2E</c>. Each starts with <c>SetColorIndex($0180)</c>, then
/// four <c>SetTimer(byte)</c> groups at bank-local entry offsets
/// <c>$0007,$0056,$00A5,$00F4</c>. Timers <c>$24,3,3,2</c> replay each
/// group's pair of <c>3, colors[16], Wait</c> records via
/// <c>DecrementTimerAndGoto(group start)</c>. One final
/// <c>1, colors[16], Wait</c> record starts at offset <c>$0140</c>;
/// <c>Delete</c> follows at <c>$0164</c>. Thus
/// <c>(36 + 3 + 3 + 2)*2*3 + 1 = 265</c> frames elapse. All 99
/// control words and 12 timer bytes match the ROM. The 432 BGR555 words
/// remain live presentation data supplied by the presentation compiler;
/// this catalog owns only placement, timing, replay, and deletion. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class SamusLoadingSuitPaletteFxColorDefinitions
{
    /// <summary>All programs write CGRAM from byte index <c>$0180</c>.</summary>
    public const ushort ColorByteIndex = 0x0180;

    /// <summary>Each program has four independently counted two-record groups.</summary>
    public const int GroupCount = 4;

    /// <summary>Each counted group contains two palette records.</summary>
    public const int FramesPerGroup = 2;

    /// <summary>Each complete program contains eight counted records and one final record.</summary>
    public const int FrameCount = GroupCount * FramesPerGroup + 1;

    /// <summary>Each record writes sixteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 36;

    /// <summary>Each record in a counted group lasts three frames.</summary>
    public const ushort GroupFrameDuration = 3;

    /// <summary>The terminal record lasts one frame.</summary>
    public const ushort FinalFrameDuration = 1;

    /// <summary>Chosen replay chronology of the loading flash, retained under #1165's nonsense exception.</summary>
    /// <remarks>Native $8D:DB66 and the following timer operands repeat each
    /// six-frame palette pair 36, 3, 3, then 2 times. The first two pairs even
    /// contain identical colors: their split is script chronology, not a color
    /// interpolation parameter. $8D:C648 assigns each literal counter and
    /// $C639 decrements it without a duration, position, or brightness input.
    /// The separate fanfare at $92:ED24 counts to 360, not this program's 265.
    /// These choices specify how long the flashes look; fitting the group index
    /// or spelling the same numbers as cases would only disguise that timing.
    /// Layout and repeated palette data are reviewed separately; this exception
    /// covers only the four replay operands, not their surrounding tables.</remarks>
    private static readonly byte[] GroupTimerValues = [36, 3, 3, 2];
    /// <summary>Native Power-suit definition $8D:E1F4 selects program $DB62.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Power =
        new(SamusLoadingSuitPaletteFxProgramOwner.PowerSuit, 0xe1f4, 0xdb62);
    /// <summary>Native Varia-suit definition $8D:E1F8 selects program $DCC8.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Varia =
        new(SamusLoadingSuitPaletteFxProgramOwner.VariaSuit, 0xe1f8, 0xdcc8);
    /// <summary>Native Gravity-suit definition $8D:E1FC selects program $DE2E.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Gravity =
        new(SamusLoadingSuitPaletteFxProgramOwner.GravitySuit, 0xe1fc, 0xde2e);

    /// <summary>The power, Varia, and gravity programs in definition order.</summary>
    /// <remarks>Named program identities replace the former indexed descriptor array.
    /// The view preserves original order/count/index rejection; runtime mechanics
    /// dispatches directly to the same three named owners without enumeration.
    /// Native header words and all program ownership are independently checked for1165.</remarks>
    public static IReadOnlyList<SamusLoadingSuitPaletteFxColorDefinition> All { get; } = new ProgramList();

    /// <summary>Exposes the three named program owners in native definition order without an indexed descriptor array.</summary>
    private sealed class ProgramList : IReadOnlyList<SamusLoadingSuitPaletteFxColorDefinition>
    {
        /// <summary>One program each for Power, Varia, and Gravity suit loading.</summary>
        public int Count => 3;
        /// <summary>Maps native definition ordinal zero through two to its named suit program.</summary>
        public SamusLoadingSuitPaletteFxColorDefinition this[int index] => index switch
        {
            0 => Power,
            1 => Varia,
            2 => Gravity,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Yields Power, Varia, then Gravity to match the compiled definition order.</summary>
        public IEnumerator<SamusLoadingSuitPaletteFxColorDefinition> GetEnumerator()
        {
            yield return Power;
            yield return Varia;
            yield return Gravity;
        }
        /// <summary>Uses the same suit order for non-generic enumeration.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Returns the bank-local offset of one counted group.</summary>
    /// <remarks>Native $8D:DB62/DCC8/DE2E programs have a four-byte color-index
    /// setup and three-byte initial timer command, so the first record starts at7.
    /// Each next group is two36-byte color records, four-byte loop command/target,
    /// and three-byte timer command later: offset=7+79*group for group0..3.
    /// Independently decoded from supported NTSC J/U v1.0 and pinned bank_8D.asm
    /// for #1165. No stored group-offset array remains.</remarks>
    public static ushort GroupStartOffset(int group)
    {
        if ((uint)group >= GroupCount)
            throw new ArgumentOutOfRangeException(nameof(group));
        return (ushort)(7 + group * (FramesPerGroup * FrameByteCount + 4 + 3));
    }

    /// <summary>Returns the cartridge-authored replay count of one group.</summary>
    public static byte GroupTimerValue(int group)
    {
        if ((uint)group >= GroupCount)
            throw new ArgumentOutOfRangeException(nameof(group));
        return GroupTimerValues[group];
    }
}
