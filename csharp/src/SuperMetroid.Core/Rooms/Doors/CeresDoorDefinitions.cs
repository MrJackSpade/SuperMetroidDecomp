namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>The Ceres rooms' doors, named by the room they lead from.</summary>
[NumberedIdentities("Each numbered door is one door of its room, named by its place in that room's door list; every door has its own destination and is used by name, never iterated or indexed.")]
public static class CeresDoorDefinitions
{
    /// <summary>The <c>Door_CeresElev_0</c> door.</summary>
    public static CartridgeDoorHeader CeresElev0 { get; } = DoorDefinitions.Find(nameof(CeresElev0));

    /// <summary>The <c>Door_FallingTile_0</c> door.</summary>
    public static CartridgeDoorHeader FallingTile0 { get; } = DoorDefinitions.Find(nameof(FallingTile0));

    /// <summary>The <c>Door_FallingTile_1</c> door.</summary>
    public static CartridgeDoorHeader FallingTile1 { get; } = DoorDefinitions.Find(nameof(FallingTile1));

    /// <summary>The <c>Door_MagnetStairs_0</c> door.</summary>
    public static CartridgeDoorHeader MagnetStairs0 { get; } = DoorDefinitions.Find(nameof(MagnetStairs0));

    /// <summary>The <c>Door_MagnetStairs_1</c> door.</summary>
    public static CartridgeDoorHeader MagnetStairs1 { get; } = DoorDefinitions.Find(nameof(MagnetStairs1));

    /// <summary>The <c>Door_DeadScientist_0</c> door.</summary>
    public static CartridgeDoorHeader DeadScientist0 { get; } = DoorDefinitions.Find(nameof(DeadScientist0));

    /// <summary>The <c>Door_DeadScientist_1</c> door.</summary>
    public static CartridgeDoorHeader DeadScientist1 { get; } = DoorDefinitions.Find(nameof(DeadScientist1));

    /// <summary>The <c>Door_58Escape_0</c> door.</summary>
    public static CartridgeDoorHeader Room58Escape0 { get; } = DoorDefinitions.Find(nameof(Room58Escape0));

    /// <summary>The <c>Door_58Escape_1</c> door.</summary>
    public static CartridgeDoorHeader Room58Escape1 { get; } = DoorDefinitions.Find(nameof(Room58Escape1));

    /// <summary>The <c>Door_CeresRidley</c> door.</summary>
    public static CartridgeDoorHeader CeresRidley { get; } = DoorDefinitions.Find(nameof(CeresRidley));
}
