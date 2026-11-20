namespace SuperMetroid.Core.World.RoomStates;

/// <summary>What each turning floor does to its room's screen (<see cref="RoomTurningFloor"/>).</summary>
public static class RoomTurningFloorDefinitions
{
    /// <summary>
    /// The turning floors that are their screen alone, drawing none of their level's blocks (#1647): the elevator shaft's.
    /// Ceres Ridley's getaway floor and no turning floor leave the level's blocks drawn.
    /// </summary>
    public static readonly IReadOnlySet<RoomTurningFloor> HidingLevelBlocks = new HashSet<RoomTurningFloor> { RoomTurningFloor.ElevatorShaft };
}
