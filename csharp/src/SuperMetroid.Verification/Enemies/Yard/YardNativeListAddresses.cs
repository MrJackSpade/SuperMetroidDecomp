using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<YardProgram>[] Entries =
    [
        new NativeProgramEntry<YardProgram>(YardProgram.OutsideTurnUpsideRightMovingUp, 0xc8c6),
        new NativeProgramEntry<YardProgram>(YardProgram.CrawlingUpsideUpMovingLeft, 0xc8e0),
        new NativeProgramEntry<YardProgram>(YardProgram.OutsideTurnUpsideUpMovingLeft, 0xc8fc),
        new NativeProgramEntry<YardProgram>(YardProgram.CrawlingUpsideLeftMovingDown, 0xc916),
        new NativeProgramEntry<YardProgram>(YardProgram.OutsideTurnUpsideLeftMovingDown, 0xc932),
        new NativeProgramEntry<YardProgram>(YardProgram.CrawlingUpsideDownMovingRight, 0xc94c),
        new NativeProgramEntry<YardProgram>(YardProgram.OutsideTurnUpsideDownMovingRight, 0xc968),
        new NativeProgramEntry<YardProgram>(YardProgram.CrawlingUpsideRightMovingUp, 0xc982),
        new NativeProgramEntry<YardProgram>(YardProgram.OutsideTurnUpsideLeftMovingUp, 0xc99e),
        new NativeProgramEntry<YardProgram>(YardProgram.CrawlingUpsideUpMovingRight, 0xc9b8),
        new NativeProgramEntry<YardProgram>(YardProgram.OutsideTurnUpsideUpMovingRight, 0xc9d4),
        new NativeProgramEntry<YardProgram>(YardProgram.CrawlingUpsideRightMovingDown, 0xc9ee),
        new NativeProgramEntry<YardProgram>(YardProgram.OutsideTurnUpsideRightMovingDown, 0xca0a),
        new NativeProgramEntry<YardProgram>(YardProgram.CrawlingUpsideDownMovingLeft, 0xca24),
        new NativeProgramEntry<YardProgram>(YardProgram.OutsideTurnUpsideDownMovingLeft, 0xca40),
        new NativeProgramEntry<YardProgram>(YardProgram.CrawlingUpsideLeftMovingUp, 0xca5a),
        new NativeProgramEntry<YardProgram>(YardProgram.InsideTurnUpsideUpMovingLeft, 0xca76),
        new NativeProgramEntry<YardProgram>(YardProgram.InsideTurnUpsideRightMovingUp, 0xca8e),
        new NativeProgramEntry<YardProgram>(YardProgram.InsideTurnUpsideDownMovingRight, 0xcaa6),
        new NativeProgramEntry<YardProgram>(YardProgram.InsideTurnUpsideLeftMovingDown, 0xcabe),
        new NativeProgramEntry<YardProgram>(YardProgram.InsideTurnUpsideUpMovingRight, 0xcad6),
        new NativeProgramEntry<YardProgram>(YardProgram.InsideTurnUpsideLeftMovingUp, 0xcaee),
        new NativeProgramEntry<YardProgram>(YardProgram.InsideTurnUpsideDownMovingLeft, 0xcb06),
        new NativeProgramEntry<YardProgram>(YardProgram.InsideTurnUpsideRightMovingDown, 0xcb1e),
        new NativeProgramEntry<YardProgram>(YardProgram.HidingUpsideUpMovingLeft, 0xcb36),
        new NativeProgramEntry<YardProgram>(YardProgram.HiddenUpsideUpMovingLeft, 0xcb44),
        new NativeProgramEntry<YardProgram>(YardProgram.HidingUpsideDownMovingLeft, 0xcb50),
        new NativeProgramEntry<YardProgram>(YardProgram.HidingUpsideDownMovingRight, 0xcb6a),
        new NativeProgramEntry<YardProgram>(YardProgram.HidingUpsideUpMovingRight, 0xcb84),
        new NativeProgramEntry<YardProgram>(YardProgram.HiddenUpsideUpMovingRight, 0xcb92),
        new NativeProgramEntry<YardProgram>(YardProgram.HidingUpsideRightMovingUp, 0xcb9e),
        new NativeProgramEntry<YardProgram>(YardProgram.HidingUpsideLeftMovingUp, 0xcbb8),
        new NativeProgramEntry<YardProgram>(YardProgram.HidingUpsideLeftMovingDown, 0xcbd2),
        new NativeProgramEntry<YardProgram>(YardProgram.HidingUpsideRightMovingDown, 0xcbec),
        new NativeProgramEntry<YardProgram>(YardProgram.AirborneFacingLeft, 0xcc06),
        new NativeProgramEntry<YardProgram>(YardProgram.AirborneFacingLeftLoop, 0xcc0e),
        new NativeProgramEntry<YardProgram>(YardProgram.AirborneFacingRight, 0xcc1e),
        new NativeProgramEntry<YardProgram>(YardProgram.AirborneFacingRightLoop, 0xcc26),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, NativeCueShape<YardCue>> Instructions = new()
    {
        [0xcc36] = NativeCueShape<YardCue>.Of<YardCue.SetMovementFunction>("E"),
        [0xcc3f] = NativeCueShape<YardCue>.Of<YardCue.SetHidingProgram>("N"),
        [0xcc48] = NativeCueShape<YardCue>.Of<YardCue.SetDirection>("D"),
        [0xcc5f] = NativeCueShape<YardCue>.Of<YardCue.MoveBy>("SY"),
        [0xcc78] = NativeCueShape<YardCue>.Exactly(new YardCue.Command(YardCueKind.RepeatPreviousPoseUnlessContinuing)),
    };
}
