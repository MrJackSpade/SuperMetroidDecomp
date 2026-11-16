using SuperMetroid.TestSupport.MovieReplay;
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
    internal static readonly NativeProgramEntry<CeresDoorSegment>[] Entries =
    [
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.RidleyRoomDoor, 0xf53a),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.RidleyRoomDoorWait, 0xf55e),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.DoorFacingRight, 0xf56c),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.OpenFacingRight, 0xf578),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.CloseFacingRight, 0xf584),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.ClosedFacingRight, 0xf598),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.ClosedFacingRightWait, 0xf59c),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.DoorFacingLeft, 0xf5be),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.OpenFacingLeft, 0xf5ca),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.CloseFacingLeft, 0xf5d6),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.ClosedFacingLeft, 0xf5ea),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.ClosedFacingLeftWait, 0xf5ee),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.ElevatorDoor, 0xf610),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.ElevatorRoomWall, 0xf61a),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.GetawayChamberLeftWall, 0xf62a),
        new NativeProgramEntry<CeresDoorSegment>(CeresDoorSegment.GetawayChamberRightWall, 0xf634),
    ];

    /// <summary>The segments no owner starts and no cue branches to, which animations only run on into (#2096).</summary>
    internal static readonly IReadOnlySet<CeresDoorSegment> OnlyRunOnInto = new HashSet<CeresDoorSegment>
    {
        CeresDoorSegment.OpenFacingRight,
        CeresDoorSegment.OpenFacingLeft,
    };

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, CeresDoorNativeListAddressesInstruction> Instructions = new()
    {
        [0xf63e] = new CeresDoorNativeListAddressesInstruction(
        [
            CeresDoorCueKind.RightDoorAppears, CeresDoorCueKind.LeftDoorAppears, CeresDoorCueKind.RightDoorStandsOpen,
            CeresDoorCueKind.LeftDoorStandsOpen, CeresDoorCueKind.RightDoorStandsClosed, CeresDoorCueKind.LeftDoorStandsClosed,
        ], "T"),
        [0xf66a] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.RidleyRoomDoorWaits], "T"),
        [0xf678] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.ElevatorRoomWallAppears], "T"),
        [0xf68b] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.MakeIntangible], ""),
        [0xf695] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.MakeTangible], ""),
        [0xf69f] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.HandDrawingToRidley], ""),
        [0xf6a6] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.MakeInvisible], ""),
        [0xf6b0] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.MakeVisibleAndTakeDrawingBack], ""),
        [0xf6b3] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.MakeVisible], ""),
        [0xf6bd] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.PlayOpeningSound], ""),
    };
}
