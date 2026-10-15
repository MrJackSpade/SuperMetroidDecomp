using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.Verification.Areas.Ceres;

/// <summary>
/// The native bank-$A6 lists the Ceres door ROM oracle (#1290) walks: each program's entry point and the owner
/// instructions the lists use, with the cue each must become and its operand roles (T target, A alternate, V value).
/// Only verification reads them; the port runs typed programs.
/// </summary>
internal static class CeresDoorNativeListAddresses
{
    /// <summary>The lists' bank.</summary>
    internal const byte Bank = 0xa6;

    /// <summary>Each typed program's native entry point, in authored order.</summary>
    internal static readonly (CeresDoorProgram Program, ushort Entry)[] Entries =
    [
        (CeresDoorProgram.RidleyRoomFacingRight, 0xf53a),
        (CeresDoorProgram.RidleyRoomFacingRightWait, 0xf55e),
        (CeresDoorProgram.NormalFacingRight, 0xf56c),
        (CeresDoorProgram.OpenFacingRight, 0xf578),
        (CeresDoorProgram.CloseFacingRight, 0xf584),
        (CeresDoorProgram.ClosedFacingRight, 0xf598),
        (CeresDoorProgram.ClosedFacingRightWait, 0xf59c),
        (CeresDoorProgram.NormalFacingLeft, 0xf5be),
        (CeresDoorProgram.OpenFacingLeft, 0xf5ca),
        (CeresDoorProgram.CloseFacingLeft, 0xf5d6),
        (CeresDoorProgram.ClosedFacingLeft, 0xf5ea),
        (CeresDoorProgram.ClosedFacingLeftWait, 0xf5ee),
        (CeresDoorProgram.RotatingElevatorPreExplosionOverlay, 0xf610),
        (CeresDoorProgram.RotatingElevatorPreExplosionOverlayLoop, 0xf612),
        (CeresDoorProgram.RotatingElevatorInvisibleWall, 0xf61a),
        (CeresDoorProgram.RotatingElevatorInvisibleWallLoop, 0xf622),
        (CeresDoorProgram.RidleyEscapeMode7LeftWall, 0xf62a),
        (CeresDoorProgram.RidleyEscapeMode7LeftWallLoop, 0xf62c),
        (CeresDoorProgram.RidleyEscapeMode7RightWall, 0xf634),
        (CeresDoorProgram.RidleyEscapeMode7RightWallLoop, 0xf636),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, (CeresDoorCueKind Kind, string Operands)> Instructions = new()
    {
        [0xf63e] = (CeresDoorCueKind.GoToIfSamusDistant, "T"),
        [0xf66a] = (CeresDoorCueKind.GoToIfAreaBossAlive, "T"),
        [0xf678] = (CeresDoorCueKind.GoToIfRidleyNotEscaped, "T"),
        [0xf68b] = (CeresDoorCueKind.MakeIntangible, ""),
        [0xf695] = (CeresDoorCueKind.MakeTangible, ""),
        [0xf69f] = (CeresDoorCueKind.HandDrawingToRidley, ""),
        [0xf6a6] = (CeresDoorCueKind.MakeInvisible, ""),
        [0xf6b0] = (CeresDoorCueKind.MakeVisibleAndTakeDrawingBack, ""),
        [0xf6b3] = (CeresDoorCueKind.MakeVisible, ""),
        [0xf6bd] = (CeresDoorCueKind.PlayOpeningSound, ""),
    };
}
