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
    /// <summary>The open facing right animation.</summary>
    OpenFacingRight,
    /// <summary>The close facing right animation.</summary>
    CloseFacingRight,
    /// <summary>The closed facing right animation.</summary>
    ClosedFacingRight,
    /// <summary>The closed facing right wait animation.</summary>
    ClosedFacingRightWait,
    /// <summary>An ordinary door facing left at its start.</summary>
    DoorFacingLeft,
    /// <summary>The open facing left animation.</summary>
    OpenFacingLeft,
    /// <summary>The close facing left animation.</summary>
    CloseFacingLeft,
    /// <summary>The closed facing left animation.</summary>
    ClosedFacingLeft,
    /// <summary>The closed facing left wait animation.</summary>
    ClosedFacingLeftWait,
    /// <summary>The elevator room's blocked door at its start.</summary>
    ElevatorDoor,
    /// <summary>The elevator room's blocked door standing.</summary>
    ElevatorDoorLoop,
    /// <summary>The elevator room's wall at its start.</summary>
    ElevatorRoomWall,
    /// <summary>The elevator room's wall standing unseen.</summary>
    ElevatorRoomWallLoop,
    /// <summary>The getaway chamber's left wall at its start.</summary>
    GetawayChamberLeftWall,
    /// <summary>The getaway chamber's left wall standing.</summary>
    GetawayChamberLeftWallLoop,
    /// <summary>The getaway chamber's right wall at its start.</summary>
    GetawayChamberRightWall,
    /// <summary>The getaway chamber's right wall standing.</summary>
    GetawayChamberRightWallLoop,
}
