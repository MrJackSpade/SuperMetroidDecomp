using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Enemies.Yard;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>
/// The native bank-$A3 lists the Yard ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (D direction, E movement function,
/// N hiding target, S and Y signed X and Y offsets).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class YardNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>The operand a native list carries where it names no program (the $CF5F sentinel).</summary>
    internal const ushort NullTarget = 0xcf5f;

    /// <summary>
    /// The routine that ends an update of the Yard's closed shell (#2180), going back to show the closed shell again while
    /// the Yard hides or on an odd random draw.
    /// </summary>
    internal const ushort ShellClosedUpdateEndsRoutine = 0xcc78;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly NativeProgramEntry<YardSegment>[] Entries =
    [
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnUpsideRightMovingUp, 0xc8c6),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingUpsideUpMovingLeft, 0xc8e0),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnUpsideUpMovingLeft, 0xc8fc),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingUpsideLeftMovingDown, 0xc916),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnUpsideLeftMovingDown, 0xc932),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingUpsideDownMovingRight, 0xc94c),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnUpsideDownMovingRight, 0xc968),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingUpsideRightMovingUp, 0xc982),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnUpsideLeftMovingUp, 0xc99e),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingUpsideUpMovingRight, 0xc9b8),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnUpsideUpMovingRight, 0xc9d4),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingUpsideRightMovingDown, 0xc9ee),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnUpsideRightMovingDown, 0xca0a),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingUpsideDownMovingLeft, 0xca24),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnUpsideDownMovingLeft, 0xca40),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingUpsideLeftMovingUp, 0xca5a),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnUpsideUpMovingLeft, 0xca76),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnUpsideRightMovingUp, 0xca8e),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnUpsideDownMovingRight, 0xcaa6),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnUpsideLeftMovingDown, 0xcabe),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnUpsideUpMovingRight, 0xcad6),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnUpsideLeftMovingUp, 0xcaee),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnUpsideDownMovingLeft, 0xcb06),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnUpsideRightMovingDown, 0xcb1e),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideUpMovingLeft, 0xcb36),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedUpsideUpMovingLeft, 0xcb3e),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenUpsideUpMovingLeft, 0xcb44),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideDownMovingLeft, 0xcb50),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedUpsideDownMovingLeft, 0xcb58),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenUpsideDownMovingLeft, 0xcb5e),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideDownMovingRight, 0xcb6a),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedUpsideDownMovingRight, 0xcb72),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenUpsideDownMovingRight, 0xcb78),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideUpMovingRight, 0xcb84),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedUpsideUpMovingRight, 0xcb8c),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenUpsideUpMovingRight, 0xcb92),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideRightMovingUp, 0xcb9e),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedUpsideRightMovingUp, 0xcba6),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenUpsideRightMovingUp, 0xcbac),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideLeftMovingUp, 0xcbb8),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedUpsideLeftMovingUp, 0xcbc0),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenUpsideLeftMovingUp, 0xcbc6),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideLeftMovingDown, 0xcbd2),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedUpsideLeftMovingDown, 0xcbda),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenUpsideLeftMovingDown, 0xcbe0),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideRightMovingDown, 0xcbec),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedUpsideRightMovingDown, 0xcbf4),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenUpsideRightMovingDown, 0xcbfa),
        new NativeProgramEntry<YardSegment>(YardSegment.AirborneFacingLeft, 0xcc06),
        new NativeProgramEntry<YardSegment>(YardSegment.AirborneFacingLeftTumbling, 0xcc0e),
        new NativeProgramEntry<YardSegment>(YardSegment.AirborneFacingRight, 0xcc1e),
        new NativeProgramEntry<YardSegment>(YardSegment.AirborneFacingRightTumbling, 0xcc26),
    ];

    /// <summary>The closed and hidden shells and the tumbles the animations only run on into (#2276).</summary>
    internal static readonly IReadOnlySet<YardSegment> OnlyRunOnInto = new HashSet<YardSegment>
    {
        YardSegment.ShellClosedUpsideUpMovingLeft,
        YardSegment.ShellClosedUpsideDownMovingLeft,
        YardSegment.HiddenUpsideDownMovingLeft,
        YardSegment.ShellClosedUpsideDownMovingRight,
        YardSegment.HiddenUpsideDownMovingRight,
        YardSegment.ShellClosedUpsideUpMovingRight,
        YardSegment.ShellClosedUpsideRightMovingUp,
        YardSegment.HiddenUpsideRightMovingUp,
        YardSegment.ShellClosedUpsideLeftMovingUp,
        YardSegment.HiddenUpsideLeftMovingUp,
        YardSegment.ShellClosedUpsideLeftMovingDown,
        YardSegment.HiddenUpsideLeftMovingDown,
        YardSegment.ShellClosedUpsideRightMovingDown,
        YardSegment.HiddenUpsideRightMovingDown,
        YardSegment.AirborneFacingLeftTumbling,
        YardSegment.AirborneFacingRightTumbling,
    };

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<YardCue>> Instructions = new()
    {
        [0xcc36] = new NativeCueShape<YardCue>("a cue that begins a movement", cue => YardCueMovements.Begun(cue) is not null, "E"),
        [0xcc3f] = NativeCueShape<YardCue>.Of<YardCue.SetHidingAnimation>("N"),
        [0xcc48] = NativeCueShape<YardCue>.Of<YardCue.SetDirection>("D"),
        [0xcc5f] = NativeCueShape<YardCue>.Of<YardCue.MoveBy>("SY"),
        [ShellClosedUpdateEndsRoutine] = NativeCueShape<YardCue>.Exactly(new YardCue.Signal(YardCueKind.ShellClosedUpdateEnds)),
    };
}
