namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres doors' and walls' typed animations (#641): one segment per animation a start, branch or wake enters, in authored order.
/// An ordinary door's segments are named once; the way the door faces chooses its pictures (#2557). Their steps are
/// <see cref="CeresDoorAnimations"/>.
/// </summary>
internal enum CeresDoorSegment
{
    /// <summary>The door of Ridley's room opening at its start.</summary>
    RidleyRoomDoor,

    /// <summary>The door of Ridley's room waiting, drawn by Ridley's scene, while the area's boss is alive.</summary>
    RidleyRoomDoorWait,

    /// <summary>An ordinary door at its start.</summary>
    Door,

    /// <summary>A door standing open, unseen and passable, telling the room it stands open; an opened door repeats it.</summary>
    Open,

    /// <summary>A door turning solid and seen, then sliding shut from open through three quarters, half and a quarter open.</summary>
    Close,

    /// <summary>A door turning solid and seen again, shut.</summary>
    Closed,

    /// <summary>A door standing shut and telling the room so, then sounding and sliding open, after which it is unseen and passable.</summary>
    ClosedWait,

    /// <summary>The elevator room's blocked door: intangible, standing for good.</summary>
    ElevatorDoor,

    /// <summary>
    /// The elevator room's wall: an ordinary left-facing door before Ridley escapes, afterwards a tangible wall standing
    /// unseen for good.
    /// </summary>
    ElevatorRoomWall,

    /// <summary>The getaway chamber's left wall: intangible, standing for good.</summary>
    GetawayChamberLeftWall,

    /// <summary>The getaway chamber's right wall: intangible, standing for good.</summary>
    GetawayChamberRightWall,
}
