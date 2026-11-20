namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// The Ceres doors' and walls' typed animations (#641): one segment per animation a start, branch or wake enters, in authored order.
/// Their steps are <see cref="CeresDoorAnimations"/>.
/// </summary>
internal enum CeresDoorSegment
{
    /// <summary>The door of Ridley's room opening at its start.</summary>
    RidleyRoomDoor,
    /// <summary>The door of Ridley's room waiting, drawn by Ridley's scene, while the area's boss is alive.</summary>
    RidleyRoomDoorWait,
    /// <summary>An ordinary door facing right at its start.</summary>
    DoorFacingRight,
    /// <summary>
    /// A right-facing door standing open, unseen and passable, telling the room it stands open; an opened door repeats
    /// it.
    /// </summary>
    OpenFacingRight,
    /// <summary>
    /// A right-facing door turning solid and seen, then sliding shut from open through three quarters, half and a
    /// quarter open.
    /// </summary>
    CloseFacingRight,
    /// <summary>A right-facing door turning solid and seen again, shut.</summary>
    ClosedFacingRight,
    /// <summary>
    /// A right-facing door standing shut and telling the room so, then sounding and sliding open, after which it is
    /// unseen and passable.
    /// </summary>
    ClosedFacingRightWait,
    /// <summary>An ordinary door facing left at its start.</summary>
    DoorFacingLeft,
    /// <summary>
    /// A left-facing door standing open, unseen and passable, telling the room it stands open; an opened door repeats
    /// it.
    /// </summary>
    OpenFacingLeft,
    /// <summary>
    /// A left-facing door turning solid and seen, then sliding shut from open through three quarters, half and a
    /// quarter open.
    /// </summary>
    CloseFacingLeft,
    /// <summary>A left-facing door turning solid and seen again, shut.</summary>
    ClosedFacingLeft,
    /// <summary>
    /// A left-facing door standing shut and telling the room so, then sounding and sliding open, after which it is
    /// unseen and passable.
    /// </summary>
    ClosedFacingLeftWait,
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
