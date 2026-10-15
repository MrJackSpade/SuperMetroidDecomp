using SuperMetroid.Core.Enemies.Yard;

namespace SuperMetroid.Verification.Enemies.Yard;

/// <summary>
/// The native bank-$A3 lists the Yard ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class YardNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>The operand a native list carries where it names no program (the $CF5F sentinel).</summary>
    internal const ushort NullTarget = 0xcf5f;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (YardProgram Program, ushort Entry)[] Entries =
    [
        (YardProgram.OutsideTurnUpsideRightMovingUp, 0xc8c6),
        (YardProgram.CrawlingUpsideUpMovingLeft, 0xc8e0),
        (YardProgram.OutsideTurnUpsideUpMovingLeft, 0xc8fc),
        (YardProgram.CrawlingUpsideLeftMovingDown, 0xc916),
        (YardProgram.OutsideTurnUpsideLeftMovingDown, 0xc932),
        (YardProgram.CrawlingUpsideDownMovingRight, 0xc94c),
        (YardProgram.OutsideTurnUpsideDownMovingRight, 0xc968),
        (YardProgram.CrawlingUpsideRightMovingUp, 0xc982),
        (YardProgram.OutsideTurnUpsideLeftMovingUp, 0xc99e),
        (YardProgram.CrawlingUpsideUpMovingRight, 0xc9b8),
        (YardProgram.OutsideTurnUpsideUpMovingRight, 0xc9d4),
        (YardProgram.CrawlingUpsideRightMovingDown, 0xc9ee),
        (YardProgram.OutsideTurnUpsideRightMovingDown, 0xca0a),
        (YardProgram.CrawlingUpsideDownMovingLeft, 0xca24),
        (YardProgram.OutsideTurnUpsideDownMovingLeft, 0xca40),
        (YardProgram.CrawlingUpsideLeftMovingUp, 0xca5a),
        (YardProgram.InsideTurnUpsideUpMovingLeft, 0xca76),
        (YardProgram.InsideTurnUpsideRightMovingUp, 0xca8e),
        (YardProgram.InsideTurnUpsideDownMovingRight, 0xcaa6),
        (YardProgram.InsideTurnUpsideLeftMovingDown, 0xcabe),
        (YardProgram.InsideTurnUpsideUpMovingRight, 0xcad6),
        (YardProgram.InsideTurnUpsideLeftMovingUp, 0xcaee),
        (YardProgram.InsideTurnUpsideDownMovingLeft, 0xcb06),
        (YardProgram.InsideTurnUpsideRightMovingDown, 0xcb1e),
        (YardProgram.HidingUpsideUpMovingLeft, 0xcb36),
        (YardProgram.HiddenUpsideUpMovingLeft, 0xcb44),
        (YardProgram.HidingUpsideDownMovingLeft, 0xcb50),
        (YardProgram.HidingUpsideDownMovingRight, 0xcb6a),
        (YardProgram.HidingUpsideUpMovingRight, 0xcb84),
        (YardProgram.HiddenUpsideUpMovingRight, 0xcb92),
        (YardProgram.HidingUpsideRightMovingUp, 0xcb9e),
        (YardProgram.HidingUpsideLeftMovingUp, 0xcbb8),
        (YardProgram.HidingUpsideLeftMovingDown, 0xcbd2),
        (YardProgram.HidingUpsideRightMovingDown, 0xcbec),
        (YardProgram.AirborneFacingLeft, 0xcc06),
        (YardProgram.AirborneFacingLeftLoop, 0xcc0e),
        (YardProgram.AirborneFacingRight, 0xcc1e),
        (YardProgram.AirborneFacingRightLoop, 0xcc26),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (YardCueKind Kind, string Operands)> Instructions = new()
    {
        [0xcc36] = (YardCueKind.SetMovementFunction, "E"),
        [0xcc3f] = (YardCueKind.SetHidingProgram, "N"),
        [0xcc48] = (YardCueKind.SetDirection, "V"),
        [0xcc5f] = (YardCueKind.MoveBy, "SY"),
        [0xcc78] = (YardCueKind.RepeatPreviousPoseUnlessContinuing, ""),
    };
}
