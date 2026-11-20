using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The enemy placement lists of the Ceres rooms.</summary>
internal static class CeresRoomEnemyPopulationDefinitions
{
    /// <summary>The Ceres doors of the rooms with a door at each end: the left one facing right and the right one facing left.</summary>
    private static readonly RoomEnemyPlacement[] HallwayDoors =
    [
        new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
    ];

    /// <summary>The enemy population of the Ceres Elevator room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition CeresElevatorDefault { get; } = new("CeresElevatorDefault",
    [
        new(EnemyDefinitionId.CeresElevatorDoor, new(232, 631), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.CeresDoor, new(224, 639), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
    ], 0);

    /// <summary>The enemy population of the Falling Tile room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition FallingTileDefault { get; } = new("FallingTileDefault", HallwayDoors, 0);

    /// <summary>The enemy population of the Magnet Stairs room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition MagnetStairsDefault { get; } = new("MagnetStairsDefault",
    [
        new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.CeresDoor, new(224, 383), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
    ], 0);

    /// <summary>The enemy population of the Dead Scientist room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition DeadScientistDefault { get; } = new("DeadScientistDefault", HallwayDoors, 0);

    /// <summary>The enemy population of the Ceres Ridley hallway in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition CeresRidleyHallwayDefault { get; } = new("CeresRidleyHallwayDefault", HallwayDoors, 0);

    /// <summary>The enemy population of the Ceres Ridley room in its default state.</summary>
    internal static PlacedRoomEnemyPopulationDefinition CeresRidleyDefault { get; } = new("CeresRidleyDefault",
    [
        new(EnemyDefinitionId.RidleyCeres, new(186, 171), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.CeresRidleyRoomDoor, new(16, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The enemy population of the Ceres Elevator room once the area's boss is dead.</summary>
    internal static PlacedRoomEnemyPopulationDefinition CeresElevatorWhenAreaBossDead { get; } = new("CeresElevatorWhenAreaBossDead",
    [
        new(EnemyDefinitionId.CeresElevatorDoor, new(232, 631), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.CeresElevatorRoomWall, new(224, 639), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.CeresSteam, new(94, 108), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
        new(EnemyDefinitionId.CeresSteam, new(162, 108), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
        new(EnemyDefinitionId.CeresSteam, new(158, 200), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
        new(EnemyDefinitionId.CeresSteam, new(98, 200), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
        new(EnemyDefinitionId.CeresSteam, new(94, 296), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
        new(EnemyDefinitionId.CeresSteam, new(162, 296), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
        new(EnemyDefinitionId.CeresSteam, new(158, 392), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
        new(EnemyDefinitionId.CeresSteam, new(98, 408), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
        new(EnemyDefinitionId.CeresSteam, new(94, 504), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
        new(EnemyDefinitionId.CeresSteam, new(162, 504), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
        new(EnemyDefinitionId.CeresSteam, new(114, 600), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
    ], 0);

    /// <summary>The enemy population of the Falling Tile room once the area's boss is dead.</summary>
    internal static PlacedRoomEnemyPopulationDefinition FallingTileWhenAreaBossDead { get; } = new("FallingTileWhenAreaBossDead",
    [
        new(EnemyDefinitionId.CeresSteam, new(160, 204), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, new(224, 188), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, new(288, 188), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, new(352, 204), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, new(440, 168), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Left)),
        new(EnemyDefinitionId.CeresSteam, new(128, 66), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
        new(EnemyDefinitionId.CeresSteam, new(192, 66), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
        new(EnemyDefinitionId.CeresSteam, new(256, 66), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
        new(EnemyDefinitionId.CeresSteam, new(320, 66), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
        new(EnemyDefinitionId.CeresSteam, new(384, 66), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
        new(EnemyDefinitionId.CeresSteam, new(72, 168), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Right)),
        new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
    ], 0);

    /// <summary>The enemy population of the Magnet Stairs room once the area's boss is dead.</summary>
    internal static PlacedRoomEnemyPopulationDefinition MagnetStairsWhenAreaBossDead { get; } = new("MagnetStairsWhenAreaBossDead",
    [
        new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.CeresDoor, new(224, 383), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
        new(EnemyDefinitionId.CeresSteam, new(112, 156), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, new(194, 176), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Right)),
        new(EnemyDefinitionId.CeresSteam, new(120, 278), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        new(EnemyDefinitionId.CeresSteam, new(62, 304), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Left)),
        new(EnemyDefinitionId.CeresSteam, new(136, 404), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
    ], 0);

    /// <summary>The enemy population of the Dead Scientist room once the area's boss is dead.</summary>
    internal static PlacedRoomEnemyPopulationDefinition DeadScientistWhenAreaBossDead { get; } = new("DeadScientistWhenAreaBossDead", HallwayDoors, 0);

    /// <summary>The enemy population of the Ceres Ridley hallway once the area's boss is dead.</summary>
    internal static PlacedRoomEnemyPopulationDefinition CeresRidleyHallwayWhenAreaBossDead { get; } = new("CeresRidleyHallwayWhenAreaBossDead", HallwayDoors, 0);

    /// <summary>The enemy population of the Ceres Ridley room once the area's boss is dead.</summary>
    internal static PlacedRoomEnemyPopulationDefinition CeresRidleyWhenAreaBossDead { get; } = new("CeresRidleyWhenAreaBossDead",
    [
        new(EnemyDefinitionId.CeresRidleyRoomDoor, new(16, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The Ceres rooms' lists, in their authored order.</summary>
    internal static RoomEnemyPopulationDefinition[] Lists { get; } =
    [
        CeresElevatorDefault,
        FallingTileDefault,
        MagnetStairsDefault,
        DeadScientistDefault,
        CeresRidleyHallwayDefault,
        CeresRidleyDefault,
        CeresElevatorWhenAreaBossDead,
        FallingTileWhenAreaBossDead,
        MagnetStairsWhenAreaBossDead,
        DeadScientistWhenAreaBossDead,
        CeresRidleyHallwayWhenAreaBossDead,
        CeresRidleyWhenAreaBossDead,
    ];
}
