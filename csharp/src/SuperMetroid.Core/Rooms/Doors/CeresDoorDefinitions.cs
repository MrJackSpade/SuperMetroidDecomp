namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Ceres rooms' doors, each named for the room it leads from and the room it leads to.</summary>
public static class CeresDoorDefinitions
{
    /// <summary>The door from the Ceres Elev room to the Falling Tile room.</summary>
    public static DoorDefinition CeresElevToFallingTile { get; } = DoorDefinitions.Find(nameof(CeresElevToFallingTile));

    /// <summary>The door from the Falling Tile room to the Ceres Elev room.</summary>
    public static DoorDefinition FallingTileToCeresElev { get; } = DoorDefinitions.Find(nameof(FallingTileToCeresElev));

    /// <summary>The door from the Falling Tile room to the Magnet Stairs room.</summary>
    public static DoorDefinition FallingTileToMagnetStairs { get; } = DoorDefinitions.Find(nameof(FallingTileToMagnetStairs));

    /// <summary>The door from the Magnet Stairs room to the Falling Tile room.</summary>
    public static DoorDefinition MagnetStairsToFallingTile { get; } = DoorDefinitions.Find(nameof(MagnetStairsToFallingTile));

    /// <summary>The door from the Magnet Stairs room to the Dead Scientist room.</summary>
    public static DoorDefinition MagnetStairsToDeadScientist { get; } = DoorDefinitions.Find(nameof(MagnetStairsToDeadScientist));

    /// <summary>The door from the Dead Scientist room to the Magnet Stairs room.</summary>
    public static DoorDefinition DeadScientistToMagnetStairs { get; } = DoorDefinitions.Find(nameof(DeadScientistToMagnetStairs));

    /// <summary>The door from the Dead Scientist room to the Ceres Ridley Hallway room.</summary>
    public static DoorDefinition DeadScientistToCeresRidleyHallway { get; } = DoorDefinitions.Find(nameof(DeadScientistToCeresRidleyHallway));

    /// <summary>The door from the Ceres Ridley Hallway room to the Dead Scientist room.</summary>
    public static DoorDefinition CeresRidleyHallwayToDeadScientist { get; } = DoorDefinitions.Find(nameof(CeresRidleyHallwayToDeadScientist));

    /// <summary>The door from the Ceres Ridley Hallway room to the Ceres Ridley room.</summary>
    public static DoorDefinition CeresRidleyHallwayToCeresRidley { get; } = DoorDefinitions.Find(nameof(CeresRidleyHallwayToCeresRidley));

    /// <summary>The Ceres Ridley room's door.</summary>
    public static DoorDefinition CeresRidley { get; } = DoorDefinitions.Find(nameof(CeresRidley));
}
