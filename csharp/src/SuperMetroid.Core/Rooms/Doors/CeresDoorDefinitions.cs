namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Ceres rooms' doors, named by the room they lead from.</summary>
[NumberedIdentities("Each numbered door is one door of its room, named by its place in that room's door list; every door has its own destination and is used by name, never iterated or indexed.")]
public static class CeresDoorDefinitions
{
    /// <summary>The Ceres Elevator room's door 0.</summary>
    public static DoorDefinition CeresElev0 { get; } = DoorDefinitions.Find(nameof(CeresElev0));

    /// <summary>The Falling Tile room's door 0.</summary>
    public static DoorDefinition FallingTile0 { get; } = DoorDefinitions.Find(nameof(FallingTile0));

    /// <summary>The Falling Tile room's door 1.</summary>
    public static DoorDefinition FallingTile1 { get; } = DoorDefinitions.Find(nameof(FallingTile1));

    /// <summary>The Magnet Stairs room's door 0.</summary>
    public static DoorDefinition MagnetStairs0 { get; } = DoorDefinitions.Find(nameof(MagnetStairs0));

    /// <summary>The Magnet Stairs room's door 1.</summary>
    public static DoorDefinition MagnetStairs1 { get; } = DoorDefinitions.Find(nameof(MagnetStairs1));

    /// <summary>The Dead Scientist room's door 0.</summary>
    public static DoorDefinition DeadScientist0 { get; } = DoorDefinitions.Find(nameof(DeadScientist0));

    /// <summary>The Dead Scientist room's door 1.</summary>
    public static DoorDefinition DeadScientist1 { get; } = DoorDefinitions.Find(nameof(DeadScientist1));

    /// <summary>The Room 58 Escape's door 0.</summary>
    public static DoorDefinition Room58Escape0 { get; } = DoorDefinitions.Find(nameof(Room58Escape0));

    /// <summary>The Room 58 Escape's door 1.</summary>
    public static DoorDefinition Room58Escape1 { get; } = DoorDefinitions.Find(nameof(Room58Escape1));

    /// <summary>The Ceres Ridley room's door.</summary>
    public static DoorDefinition CeresRidley { get; } = DoorDefinitions.Find(nameof(CeresRidley));
}
