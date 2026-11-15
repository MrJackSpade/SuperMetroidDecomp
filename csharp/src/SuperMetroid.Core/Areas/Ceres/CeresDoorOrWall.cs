namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>Which of the Ceres station's doors and walls a placement places (#1953), each its own actor.</summary>
internal enum CeresDoorOrWall
{
    /// <summary>An ordinary door facing right, which opens as Samus comes near and shakes the station during its escape.</summary>
    DoorFacingRight,
    /// <summary>An ordinary door facing left, which opens as Samus comes near and shakes the station during its escape.</summary>
    DoorFacingLeft,
    /// <summary>The elevator room's blocked door, which blows apart once the station starts to self-destruct.</summary>
    ElevatorDoor,
    /// <summary>The door of Ceres Ridley's room, which his scene draws while he holds the room.</summary>
    RidleyRoomDoor,
    /// <summary>The elevator room's left door, an ordinary door until Ridley escapes and an unseen wall after.</summary>
    ElevatorRoomWall,
    /// <summary>The left wall of Ceres Ridley's getaway chamber, beside its turning floor.</summary>
    GetawayChamberLeftWall,
    /// <summary>The right wall of Ceres Ridley's getaway chamber, beside its turning floor.</summary>
    GetawayChamberRightWall,
}
