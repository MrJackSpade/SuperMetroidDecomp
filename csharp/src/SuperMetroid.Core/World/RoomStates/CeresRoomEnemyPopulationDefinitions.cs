using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The enemy placement lists of the Ceres rooms.</summary>
internal static class CeresRoomEnemyPopulationDefinitions
{
    /// <summary>The enemy population of the Ceres Elevator room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition CeresElevatorDefault { get; } = new("CeresElevatorDefault",
    [
        new(EnemyDefinitionId.CeresElevatorDoor, new(232, 631), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.CeresDoor, new(224, 639), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
    ], 0);

    /// <summary>The enemy population of the Falling Tile room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition FallingTileDefault { get; } = new("FallingTileDefault",
    [
        new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
    ], 0);

    /// <summary>The enemy population of the Magnet Stairs room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition MagnetStairsDefault { get; } = new("MagnetStairsDefault",
    [
        new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.CeresDoor, new(224, 383), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
    ], 0);

    /// <summary>The enemy population of the Dead Scientist room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition DeadScientistDefault { get; } = new("DeadScientistDefault",
    [
        new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
    ], 0);

    /// <summary>The enemy population of the Ceres Ridley hallway in its default state.</summary>
    internal static RoomEnemyPopulationDefinition CeresRidleyHallwayDefault { get; } = new("CeresRidleyHallwayDefault",
    [
        new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
    ], 0);

    /// <summary>The enemy population of the Ceres Ridley room in its default state.</summary>
    internal static RoomEnemyPopulationDefinition CeresRidleyDefault { get; } = new("CeresRidleyDefault",
    [
        new(EnemyDefinitionId.RidleyCeres, new(186, 171), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        new(EnemyDefinitionId.CeresRidleyRoomDoor, new(16, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
    ], 0);

    /// <summary>The enemy population of the Ceres Elevator room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition CeresElevatorWhenAreaBossDead { get; } = new("CeresElevatorWhenAreaBossDead",
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
    internal static RoomEnemyPopulationDefinition FallingTileWhenAreaBossDead { get; } = new("FallingTileWhenAreaBossDead",
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
    internal static RoomEnemyPopulationDefinition MagnetStairsWhenAreaBossDead { get; } = new("MagnetStairsWhenAreaBossDead",
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
    internal static RoomEnemyPopulationDefinition DeadScientistWhenAreaBossDead { get; } = new("DeadScientistWhenAreaBossDead",
    [
        new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
    ], 0);

    /// <summary>The enemy population of the Ceres Ridley hallway once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition CeresRidleyHallwayWhenAreaBossDead { get; } = new("CeresRidleyHallwayWhenAreaBossDead",
    [
        new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
        new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
    ], 0);

    /// <summary>The enemy population of the Ceres Ridley room once the area's boss is dead.</summary>
    internal static RoomEnemyPopulationDefinition CeresRidleyWhenAreaBossDead { get; } = new("CeresRidleyWhenAreaBossDead",
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
