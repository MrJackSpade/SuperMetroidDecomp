using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static readonly NativeProgramEntry<CeresDoorProgram>[] Entries =
    [
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.RidleyRoomFacingRight, 0xf53a),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.RidleyRoomFacingRightWait, 0xf55e),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.NormalFacingRight, 0xf56c),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.OpenFacingRight, 0xf578),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.CloseFacingRight, 0xf584),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.ClosedFacingRight, 0xf598),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.ClosedFacingRightWait, 0xf59c),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.NormalFacingLeft, 0xf5be),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.OpenFacingLeft, 0xf5ca),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.CloseFacingLeft, 0xf5d6),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.ClosedFacingLeft, 0xf5ea),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.ClosedFacingLeftWait, 0xf5ee),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.RotatingElevatorPreExplosionOverlay, 0xf610),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.RotatingElevatorPreExplosionOverlayLoop, 0xf612),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.RotatingElevatorInvisibleWall, 0xf61a),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.RotatingElevatorInvisibleWallLoop, 0xf622),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.RidleyEscapeFloorLeftWall, 0xf62a),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.RidleyEscapeFloorLeftWallLoop, 0xf62c),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.RidleyEscapeFloorRightWall, 0xf634),
        new NativeProgramEntry<CeresDoorProgram>(CeresDoorProgram.RidleyEscapeFloorRightWallLoop, 0xf636),
    ];

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, CeresDoorNativeListAddressesInstruction> Instructions = new()
    {
        [0xf63e] = new CeresDoorNativeListAddressesInstruction(CeresDoorCueKind.GoToIfSamusDistant, "T"),
        [0xf66a] = new CeresDoorNativeListAddressesInstruction(CeresDoorCueKind.GoToIfAreaBossAlive, "T"),
        [0xf678] = new CeresDoorNativeListAddressesInstruction(CeresDoorCueKind.GoToIfRidleyNotEscaped, "T"),
        [0xf68b] = new CeresDoorNativeListAddressesInstruction(CeresDoorCueKind.MakeIntangible, ""),
        [0xf695] = new CeresDoorNativeListAddressesInstruction(CeresDoorCueKind.MakeTangible, ""),
        [0xf69f] = new CeresDoorNativeListAddressesInstruction(CeresDoorCueKind.HandDrawingToRidley, ""),
        [0xf6a6] = new CeresDoorNativeListAddressesInstruction(CeresDoorCueKind.MakeInvisible, ""),
        [0xf6b0] = new CeresDoorNativeListAddressesInstruction(CeresDoorCueKind.MakeVisibleAndTakeDrawingBack, ""),
        [0xf6b3] = new CeresDoorNativeListAddressesInstruction(CeresDoorCueKind.MakeVisible, ""),
        [0xf6bd] = new CeresDoorNativeListAddressesInstruction(CeresDoorCueKind.PlayOpeningSound, ""),
    };
}
