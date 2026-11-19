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
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnOnLeftWallMovingUp, 0xc8c6),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingOnFloorMovingLeft, 0xc8e0),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnOnFloorMovingLeft, 0xc8fc),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingOnRightWallMovingDown, 0xc916),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnOnRightWallMovingDown, 0xc932),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingOnCeilingMovingRight, 0xc94c),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnOnCeilingMovingRight, 0xc968),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingOnLeftWallMovingUp, 0xc982),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnOnRightWallMovingUp, 0xc99e),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingOnFloorMovingRight, 0xc9b8),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnOnFloorMovingRight, 0xc9d4),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingOnLeftWallMovingDown, 0xc9ee),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnOnLeftWallMovingDown, 0xca0a),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingOnCeilingMovingLeft, 0xca24),
        new NativeProgramEntry<YardSegment>(YardSegment.OutsideTurnOnCeilingMovingLeft, 0xca40),
        new NativeProgramEntry<YardSegment>(YardSegment.CrawlingOnRightWallMovingUp, 0xca5a),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnOnFloorMovingLeft, 0xca76),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnOnLeftWallMovingUp, 0xca8e),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnOnCeilingMovingRight, 0xcaa6),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnOnRightWallMovingDown, 0xcabe),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnOnFloorMovingRight, 0xcad6),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnOnRightWallMovingUp, 0xcaee),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnOnCeilingMovingLeft, 0xcb06),
        new NativeProgramEntry<YardSegment>(YardSegment.InsideTurnOnLeftWallMovingDown, 0xcb1e),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingOnFloorMovingLeft, 0xcb36),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedOnFloorMovingLeft, 0xcb3e),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenOnFloorMovingLeft, 0xcb44),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingOnCeilingMovingLeft, 0xcb50),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedOnCeilingMovingLeft, 0xcb58),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenOnCeilingMovingLeft, 0xcb5e),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingOnCeilingMovingRight, 0xcb6a),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedOnCeilingMovingRight, 0xcb72),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenOnCeilingMovingRight, 0xcb78),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingOnFloorMovingRight, 0xcb84),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedOnFloorMovingRight, 0xcb8c),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenOnFloorMovingRight, 0xcb92),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingOnLeftWallMovingUp, 0xcb9e),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedOnLeftWallMovingUp, 0xcba6),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenOnLeftWallMovingUp, 0xcbac),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingOnRightWallMovingUp, 0xcbb8),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedOnRightWallMovingUp, 0xcbc0),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenOnRightWallMovingUp, 0xcbc6),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingOnRightWallMovingDown, 0xcbd2),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedOnRightWallMovingDown, 0xcbda),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenOnRightWallMovingDown, 0xcbe0),
        new NativeProgramEntry<YardSegment>(YardSegment.HidingOnLeftWallMovingDown, 0xcbec),
        new NativeProgramEntry<YardSegment>(YardSegment.ShellClosedOnLeftWallMovingDown, 0xcbf4),
        new NativeProgramEntry<YardSegment>(YardSegment.HiddenOnLeftWallMovingDown, 0xcbfa),
        new NativeProgramEntry<YardSegment>(YardSegment.AirborneFacingLeft, 0xcc06),
        new NativeProgramEntry<YardSegment>(YardSegment.AirborneFacingLeftTumbling, 0xcc0e),
        new NativeProgramEntry<YardSegment>(YardSegment.AirborneFacingRight, 0xcc1e),
        new NativeProgramEntry<YardSegment>(YardSegment.AirborneFacingRightTumbling, 0xcc26),
    ];

    /// <summary>The closed and hidden shells and the tumbles the animations only run on into (#2276).</summary>
    internal static readonly IReadOnlySet<YardSegment> OnlyRunOnInto = new HashSet<YardSegment>
    {
        YardSegment.ShellClosedOnFloorMovingLeft,
        YardSegment.ShellClosedOnCeilingMovingLeft,
        YardSegment.HiddenOnCeilingMovingLeft,
        YardSegment.ShellClosedOnCeilingMovingRight,
        YardSegment.HiddenOnCeilingMovingRight,
        YardSegment.ShellClosedOnFloorMovingRight,
        YardSegment.ShellClosedOnLeftWallMovingUp,
        YardSegment.HiddenOnLeftWallMovingUp,
        YardSegment.ShellClosedOnRightWallMovingUp,
        YardSegment.HiddenOnRightWallMovingUp,
        YardSegment.ShellClosedOnRightWallMovingDown,
        YardSegment.HiddenOnRightWallMovingDown,
        YardSegment.ShellClosedOnLeftWallMovingDown,
        YardSegment.HiddenOnLeftWallMovingDown,
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
