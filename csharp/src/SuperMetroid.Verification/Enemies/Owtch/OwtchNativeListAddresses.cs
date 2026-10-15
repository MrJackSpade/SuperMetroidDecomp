using SuperMetroid.Core.Enemies.Owtch;

namespace SuperMetroid.Verification.Enemies.Owtch;

/// <summary>
/// The native bank-$A2 lists the Owtch ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class OwtchNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa2;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (OwtchProgram Program, ushort Entry)[] Entries =
    [
        (OwtchProgram.MovingLeft, 0xa3ab),
        (OwtchProgram.MovingLeftLoop, 0xa3ad),
        (OwtchProgram.MovingRight, 0xa3bd),
        (OwtchProgram.MovingRightLoop, 0xa3bf),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (OwtchCueKind Kind, string Operands)> Instructions = new()
    {
        [0xa56d] = (OwtchCueKind.SetMovingLeft, ""),
        [0xa571] = (OwtchCueKind.SetMovingRight, ""),
    };
}
