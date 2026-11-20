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

    /// <summary>
    /// Each typed program's native entry point, in authored order, for a door facing right and one facing left (#2557): the
    /// door of Ridley's room, the elevator room's blocked door and the getaway chamber's walls run with the right-facing
    /// lists, and the elevator room's wall, which the cartridge lists between them, goes on as the left-facing door it
    /// stands as.
    /// </summary>
    internal static readonly IReadOnlyDictionary<HorizontalDirection, NativeProgramEntry<CeresDoorSegment>[]> EntriesBySide =
        new Dictionary<HorizontalDirection, NativeProgramEntry<CeresDoorSegment>[]>
        {
            [HorizontalDirection.Right] =
            [
                new(CeresDoorSegment.RidleyRoomDoor, 0xf53a),
                new(CeresDoorSegment.RidleyRoomDoorWait, 0xf55e),
                new(CeresDoorSegment.Door, 0xf56c),
                new(CeresDoorSegment.Open, 0xf578),
                new(CeresDoorSegment.Close, 0xf584),
                new(CeresDoorSegment.Closed, 0xf598),
                new(CeresDoorSegment.ClosedWait, 0xf59c),
                new(CeresDoorSegment.ElevatorDoor, 0xf610),
                new(CeresDoorSegment.ElevatorRoomWall, 0xf61a),
                new(CeresDoorSegment.GetawayChamberLeftWall, 0xf62a),
                new(CeresDoorSegment.GetawayChamberRightWall, 0xf634),
            ],
            [HorizontalDirection.Left] =
            [
                new(CeresDoorSegment.Door, 0xf5be),
                new(CeresDoorSegment.Open, 0xf5ca),
                new(CeresDoorSegment.Close, 0xf5d6),
                new(CeresDoorSegment.Closed, 0xf5ea),
                new(CeresDoorSegment.ClosedWait, 0xf5ee),
                new(CeresDoorSegment.ElevatorRoomWall, 0xf61a),
            ],
        };

    /// <summary>The segments no owner starts and no cue branches to, which animations only run on into (#2096).</summary>
    internal static readonly IReadOnlySet<CeresDoorSegment> OnlyRunOnInto = new HashSet<CeresDoorSegment>
    {
        CeresDoorSegment.Open,
    };

    /// <summary>
    /// The frame records the lists show while the door or wall is unseen (#2424), each straight after the instruction that
    /// hides it or in a list only a hidden door plays: the opening frames of Ridley's room's door, its wait, both ordinary
    /// doors' starts and open waits, and the elevator room's wall once Ridley has escaped. The typed segments name no
    /// picture there.
    /// </summary>
    internal static readonly IReadOnlySet<ushort> UnseenFrames = new HashSet<ushort>
    {
        0xf53e, 0xf55e, 0xf570, 0xf578, 0xf5c2, 0xf5ca, 0xf622,
    };

    /// <summary>Each owner instruction the lists use, keyed by its native routine: its cue and operand roles.</summary>
    internal static readonly Dictionary<ushort, CeresDoorNativeListAddressesInstruction> Instructions = new()
    {
        [0xf63e] = new CeresDoorNativeListAddressesInstruction(
        [
            CeresDoorCueKind.DoorAppears, CeresDoorCueKind.DoorStandsOpen, CeresDoorCueKind.DoorStandsClosed,
        ], "T"),
        [0xf66a] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.RidleyRoomDoorWaits], "T"),
        [0xf678] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.ElevatorRoomWallAppears], "T"),
        [0xf68b] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.MakeIntangible], ""),
        [0xf695] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.MakeTangible], ""),
        [0xf69f] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.ShowOverlay], ""),
        [0xf6a6] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.MakeInvisible], ""),
        [0xf6b0] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.MakeVisibleAndShowActor], ""),
        [0xf6b3] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.MakeVisible], ""),
        [0xf6bd] = new CeresDoorNativeListAddressesInstruction([CeresDoorCueKind.PlayOpeningSound], ""),
    };
}
