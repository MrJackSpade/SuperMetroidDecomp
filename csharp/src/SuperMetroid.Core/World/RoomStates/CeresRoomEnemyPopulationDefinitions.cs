using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The enemy placement lists of the Ceres rooms.</summary>
internal static class CeresRoomEnemyPopulationDefinitions
{
    /// <summary>The Ceres rooms' lists.</summary>
    internal static RoomEnemyPopulationDefinition[] Build() =>
    [
        new(nameof(RoomEnemyPopulationDefinitions.CeresElevatorDefault),
        [
            new(EnemyDefinitionId.CeresElevatorDoor, new(232, 631), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
            new(EnemyDefinitionId.CeresDoor, new(224, 639), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.FallingTileDefault),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.MagnetStairsDefault),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
            new(EnemyDefinitionId.CeresDoor, new(224, 383), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.DeadScientistDefault),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.CeresRidleyHallwayDefault),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.CeresRidleyDefault),
        [
            new(EnemyDefinitionId.RidleyCeres, new(186, 171), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
            new(EnemyDefinitionId.CeresRidleyRoomDoor, new(16, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.CeresElevatorWhenAreaBossDead),
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
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.FallingTileWhenAreaBossDead),
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
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.MagnetStairsWhenAreaBossDead),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
            new(EnemyDefinitionId.CeresDoor, new(224, 383), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
            new(EnemyDefinitionId.CeresSteam, new(112, 156), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.CeresSteam, new(194, 176), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Right)),
            new(EnemyDefinitionId.CeresSteam, new(120, 278), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.CeresSteam, new(62, 304), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Left)),
            new(EnemyDefinitionId.CeresSteam, new(136, 404), EnemyContact.None, EnemyProcessing.Animates, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.DeadScientistWhenAreaBossDead),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.CeresRidleyHallwayWhenAreaBossDead),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Right)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, new CeresDoorSpawnSettings(Facing: HorizontalDirection.Left)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.CeresRidleyWhenAreaBossDead),
        [
            new(EnemyDefinitionId.CeresRidleyRoomDoor, new(16, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.Animates, EnemyPresence.None, false, EnemySpawnSettings.None),
        ], 0),
    ];
}
