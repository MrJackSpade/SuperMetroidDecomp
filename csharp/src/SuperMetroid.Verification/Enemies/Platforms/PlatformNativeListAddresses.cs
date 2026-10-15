using SuperMetroid.Core.Enemies.Platforms;

namespace SuperMetroid.Verification.Enemies.Platforms;

/// <summary>
/// The native bank-$A3 lists the Tripper or Kamer ROM oracle (#641) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class PlatformNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa3;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (PlatformProgram Program, ushort Entry)[] Entries =
    [
        (PlatformProgram.KamerMovingLeft, 0x9bbb),
        (PlatformProgram.KamerMovingLeftLoop, 0x9bbd),
        (PlatformProgram.KamerMovingRight, 0x9bd1),
        (PlatformProgram.KamerMovingRightLoop, 0x9bd3),
        (PlatformProgram.KamerStillLeft, 0x9be7),
        (PlatformProgram.KamerStillLeftLoop, 0x9be9),
        (PlatformProgram.KamerStillRight, 0x9bfd),
        (PlatformProgram.KamerStillRightLoop, 0x9bff),
        (PlatformProgram.TripperMovingLeft, 0x9c13),
        (PlatformProgram.TripperMovingLeftLoop, 0x9c15),
        (PlatformProgram.TripperMovingRight, 0x9c29),
        (PlatformProgram.TripperMovingRightLoop, 0x9c2b),
        (PlatformProgram.TripperStillMovingLeft, 0x9c3f),
        (PlatformProgram.TripperStillMovingLeftLoop, 0x9c41),
        (PlatformProgram.TripperStillMovingRight, 0x9c55),
        (PlatformProgram.TripperStillMovingRightLoop, 0x9c57),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (PlatformCueKind Kind, string Operands)> Instructions = new()
    {
        [0x9c6b] = (PlatformCueKind.MoveLeft, ""),
        [0x9c76] = (PlatformCueKind.MoveRight, ""),
        [0x9c81] = (PlatformCueKind.MoveLeft, ""),
        [0x9c8c] = (PlatformCueKind.MoveRight, ""),
    };
}
