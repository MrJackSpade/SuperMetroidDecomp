using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The bank-$A1 placement lists of the Ceres room-header region, in native table order.</summary>
internal static class CeresRoomEnemyPopulationDefinitions
{
    /// <summary>The region's lists, in native table order.</summary>
    internal static RoomEnemyPopulationDefinition[] Build() =>
    [
        new(nameof(RoomEnemyPopulationDefinitions.CeresElev0),
        [
            new(EnemyDefinitionId.CeresDoor, new(232, 631), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.RotatingElevatorPreExplosionOverlay)),
            new(EnemyDefinitionId.CeresDoor, new(224, 639), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.FallingTile0),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.MagnetStairs0),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(224, 383), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.DeadScientist0),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Room58Escape0),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.CeresRidley0),
        [
            new(EnemyDefinitionId.RidleyCeres, new(186, 171), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, EnemySpawnSettings.None),
            new(EnemyDefinitionId.CeresDoor, new(16, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.RidleyRoomFacingRight)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.CeresElev1),
        [
            new(EnemyDefinitionId.CeresDoor, new(232, 631), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.RotatingElevatorPreExplosionOverlay)),
            new(EnemyDefinitionId.CeresDoor, new(224, 639), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.RotatingElevatorInvisibleWall)),
            new(EnemyDefinitionId.Steam, new(94, 108), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
            new(EnemyDefinitionId.Steam, new(162, 108), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
            new(EnemyDefinitionId.Steam, new(158, 200), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
            new(EnemyDefinitionId.Steam, new(98, 200), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
            new(EnemyDefinitionId.Steam, new(94, 296), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
            new(EnemyDefinitionId.Steam, new(162, 296), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
            new(EnemyDefinitionId.Steam, new(158, 392), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
            new(EnemyDefinitionId.Steam, new(98, 408), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
            new(EnemyDefinitionId.Steam, new(94, 504), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
            new(EnemyDefinitionId.Steam, new(162, 504), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
            new(EnemyDefinitionId.Steam, new(114, 600), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.FallingTile1),
        [
            new(EnemyDefinitionId.Steam, new(160, 204), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.Steam, new(224, 188), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.Steam, new(288, 188), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.Steam, new(352, 204), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.Steam, new(440, 168), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Left)),
            new(EnemyDefinitionId.Steam, new(128, 66), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
            new(EnemyDefinitionId.Steam, new(192, 66), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
            new(EnemyDefinitionId.Steam, new(256, 66), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
            new(EnemyDefinitionId.Steam, new(320, 66), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
            new(EnemyDefinitionId.Steam, new(384, 66), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
            new(EnemyDefinitionId.Steam, new(72, 168), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Right)),
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.MagnetStairs1),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(224, 383), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
            new(EnemyDefinitionId.Steam, new(112, 156), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.Steam, new(194, 176), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Right)),
            new(EnemyDefinitionId.Steam, new(120, 278), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.Steam, new(62, 304), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Left)),
            new(EnemyDefinitionId.Steam, new(136, 404), EnemyProperties.ProcessInstructions, EnemyExtraProperties.None, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.DeadScientist1),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Room58Escape1),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.CeresRidley1),
        [
            new(EnemyDefinitionId.CeresDoor, new(16, 127), EnemyProperties.ProcessOffScreen | EnemyProperties.ProcessInstructions | EnemyProperties.SolidToSamus, EnemyExtraProperties.None, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.RidleyRoomFacingRight)),
        ], 0),
    ];
}
