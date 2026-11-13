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
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenUpsideUpMovingLeft, 0xcb44),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideDownMovingLeft, 0xcb50),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideDownMovingRight, 0xcb6a),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideUpMovingRight, 0xcb84),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenUpsideUpMovingRight, 0xcb92),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideRightMovingUp, 0xcb9e),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideLeftMovingUp, 0xcbb8),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideLeftMovingDown, 0xcbd2),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingUpsideRightMovingDown, 0xcbec),
        new NativeProgramEntry<YardSegment>(YardSegment.AirborneFacingLeft, 0xcc06),
        new NativeProgramEntry<YardSegment>(YardSegment.AirborneFacingLeftLoop, 0xcc0e),
        new NativeProgramEntry<YardSegment>(YardSegment.AirborneFacingRight, 0xcc1e),
        new NativeProgramEntry<YardSegment>(YardSegment.AirborneFacingRightLoop, 0xcc26),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<YardCue>> Instructions = new()
    {
        [0xcc36] = NativeCueShape<YardCue>.Of<YardCue.SetMovement>("E"),
        [0xcc3f] = NativeCueShape<YardCue>.Of<YardCue.SetHidingAnimation>("N"),
        [0xcc48] = NativeCueShape<YardCue>.Of<YardCue.SetDirection>("D"),
        [0xcc5f] = NativeCueShape<YardCue>.Of<YardCue.MoveBy>("SY"),
        [0xcc78] = NativeCueShape<YardCue>.Exactly(new YardCue.Signal(YardCueKind.RepeatPreviousPoseUnlessContinuing)),
    };
}
