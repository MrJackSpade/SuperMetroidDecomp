namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Ceres rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static class CeresDoorDefinitions
{
    /// <summary>The door from the Ceres Elevator room to the Falling Tile room.</summary>
    public static DoorDefinition CeresElevatorToFallingTile { get; } = new("CeresElevatorToFallingTile", RoomDefinitions.FallingTile, new(DoorDirection.Right, DoorClosingBehavior.None), new(0, 0), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.FallingTileEndsTurningFloor);

    /// <summary>The door from the Falling Tile room to the Ceres Elevator room.</summary>
    public static DoorDefinition FallingTileToCeresElevator { get; } = new("FallingTileToCeresElevator", RoomDefinitions.CeresElevator, new(DoorDirection.Left, DoorClosingBehavior.None), new(0, 0), new(0, 2), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.CeresElevatorShowsTurningFloor);

    /// <summary>The door from the Falling Tile room to the Magnet Stairs room.</summary>
    public static DoorDefinition FallingTileToMagnetStairs { get; } = new("FallingTileToMagnetStairs", RoomDefinitions.MagnetStairs, new(DoorDirection.Right, DoorClosingBehavior.None), new(0, 0), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Magnet Stairs room to the Falling Tile room.</summary>
    public static DoorDefinition MagnetStairsToFallingTile { get; } = new("MagnetStairsToFallingTile", RoomDefinitions.FallingTile, new(DoorDirection.Left, DoorClosingBehavior.None), new(0, 0), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Magnet Stairs room to the Dead Scientist room.</summary>
    public static DoorDefinition MagnetStairsToDeadScientist { get; } = new("MagnetStairsToDeadScientist", RoomDefinitions.DeadScientist, new(DoorDirection.Right, DoorClosingBehavior.None), new(0, 0), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Dead Scientist room to the Magnet Stairs room.</summary>
    public static DoorDefinition DeadScientistToMagnetStairs { get; } = new("DeadScientistToMagnetStairs", RoomDefinitions.MagnetStairs, new(DoorDirection.Left, DoorClosingBehavior.None), new(0, 0), new(0, 1), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Dead Scientist room to the Ceres Ridley Hallway room.</summary>
    public static DoorDefinition DeadScientistToCeresRidleyHallway { get; } = new("DeadScientistToCeresRidleyHallway", RoomDefinitions.CeresRidleyHallway, new(DoorDirection.Right, DoorClosingBehavior.None), new(0, 0), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ceres Ridley Hallway room to the Dead Scientist room.</summary>
    public static DoorDefinition CeresRidleyHallwayToDeadScientist { get; } = new("CeresRidleyHallwayToDeadScientist", RoomDefinitions.DeadScientist, new(DoorDirection.Left, DoorClosingBehavior.None), new(0, 0), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The door from the Ceres Ridley Hallway room to the Ceres Ridley room.</summary>
    public static DoorDefinition CeresRidleyHallwayToCeresRidley { get; } = new("CeresRidleyHallwayToCeresRidley", RoomDefinitions.CeresRidley, new(DoorDirection.Right, DoorClosingBehavior.None), new(0, 0), new(0, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);

    /// <summary>The Ceres Ridley room's door.</summary>
    public static DoorDefinition CeresRidley { get; } = new("CeresRidley", RoomDefinitions.CeresRidleyHallway, new(DoorDirection.Left, DoorClosingBehavior.None), new(0, 0), new(1, 0), DoorSamusSpeed.AxisDefault, DoorSetupDefinitions.None);
}
