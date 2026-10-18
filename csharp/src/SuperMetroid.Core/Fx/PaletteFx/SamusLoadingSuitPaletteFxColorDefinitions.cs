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

    /// <summary>Each program has four independently counted two-record groups.</summary>
    public const int GroupCount = 4;

    /// <summary>Each counted group contains two palette records.</summary>
    public const int FramesPerGroup = 2;

    /// <summary>Each complete program contains eight counted records and one final record.</summary>
    public const int FrameCount = GroupCount * FramesPerGroup + 1;

    /// <summary>Each record writes sixteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 16;
    /// <summary>Native Power-suit definition $8D:E1F4 selects program $DB62.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Power =
        new(SamusLoadingSuitPaletteFxProgramOwner.PowerSuit, PaletteFxObject.SamusLoadingPowerSuit, 238);
    /// <summary>Native Varia-suit definition $8D:E1F8 selects program $DCC8.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Varia =
        new(SamusLoadingSuitPaletteFxProgramOwner.VariaSuit, PaletteFxObject.SamusLoadingVariaSuit, 247);
    /// <summary>Native Gravity-suit definition $8D:E1FC selects program $DE2E.</summary>
    private static readonly SamusLoadingSuitPaletteFxColorDefinition Gravity =
        new(SamusLoadingSuitPaletteFxProgramOwner.GravitySuit, PaletteFxObject.SamusLoadingGravitySuit, 256);

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
}
