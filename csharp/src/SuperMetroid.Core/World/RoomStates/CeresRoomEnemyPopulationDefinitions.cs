using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World.RoomStates;

/// <summary>The enemy placement lists of the Ceres rooms.</summary>
internal static class CeresRoomEnemyPopulationDefinitions
{
    /// <summary>The Ceres rooms' lists.</summary>
    internal static RoomEnemyPopulationDefinition[] Build() =>
    [
        new(nameof(RoomEnemyPopulationDefinitions.CeresElevDefault),
        [
            new(EnemyDefinitionId.CeresDoor, new(232, 631), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.RotatingElevatorPreExplosionOverlay)),
            new(EnemyDefinitionId.CeresDoor, new(224, 639), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.FallingTileDefault),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.MagnetStairsDefault),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(224, 383), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.DeadScientistDefault),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Room58EscapeDefault),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.CeresRidleyDefault),
        [
            new(EnemyDefinitionId.RidleyCeres, new(186, 171), EnemyContact.None, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, EnemySpawnSettings.None),
            new(EnemyDefinitionId.CeresDoor, new(16, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.RidleyRoomFacingRight)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.CeresElevWhenAreaBossDead),
        [
            new(EnemyDefinitionId.CeresDoor, new(232, 631), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.RotatingElevatorPreExplosionOverlay)),
            new(EnemyDefinitionId.CeresDoor, new(224, 639), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.RotatingElevatorInvisibleWall)),
            new(EnemyDefinitionId.Steam, new(94, 108), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
            new(EnemyDefinitionId.Steam, new(162, 108), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
            new(EnemyDefinitionId.Steam, new(158, 200), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
            new(EnemyDefinitionId.Steam, new(98, 200), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
            new(EnemyDefinitionId.Steam, new(94, 296), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
            new(EnemyDefinitionId.Steam, new(162, 296), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
            new(EnemyDefinitionId.Steam, new(158, 392), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
            new(EnemyDefinitionId.Steam, new(98, 408), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
            new(EnemyDefinitionId.Steam, new(94, 504), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorLeft)),
            new(EnemyDefinitionId.Steam, new(162, 504), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
            new(EnemyDefinitionId.Steam, new(114, 600), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.RotatingElevatorRight)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.FallingTileWhenAreaBossDead),
        [
            new(EnemyDefinitionId.Steam, new(160, 204), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.Steam, new(224, 188), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.Steam, new(288, 188), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.Steam, new(352, 204), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.Steam, new(440, 168), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Left)),
            new(EnemyDefinitionId.Steam, new(128, 66), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
            new(EnemyDefinitionId.Steam, new(192, 66), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
            new(EnemyDefinitionId.Steam, new(256, 66), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
            new(EnemyDefinitionId.Steam, new(320, 66), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
            new(EnemyDefinitionId.Steam, new(384, 66), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Down)),
            new(EnemyDefinitionId.Steam, new(72, 168), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Right)),
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.MagnetStairsWhenAreaBossDead),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(224, 383), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
            new(EnemyDefinitionId.Steam, new(112, 156), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.Steam, new(194, 176), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Right)),
            new(EnemyDefinitionId.Steam, new(120, 278), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
            new(EnemyDefinitionId.Steam, new(62, 304), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Left)),
            new(EnemyDefinitionId.Steam, new(136, 404), EnemyContact.None, EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresSteamSpawnSettings(Variant: CeresSteamVariant.Up)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.DeadScientistWhenAreaBossDead),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.Room58EscapeWhenAreaBossDead),
        [
            new(EnemyDefinitionId.CeresDoor, new(32, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingRight)),
            new(EnemyDefinitionId.CeresDoor, new(480, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.NormalFacingLeft)),
        ], 0),
        new(nameof(RoomEnemyPopulationDefinitions.CeresRidleyWhenAreaBossDead),
        [
            new(EnemyDefinitionId.CeresDoor, new(16, 127), EnemyContact.SolidToSamus, EnemyProcessing.OffScreen | EnemyProcessing.AnimationScript, EnemyPresence.None, false, new CeresDoorSpawnSettings(Variant: CeresDoorVariant.RidleyRoomFacingRight)),
        ], 0),
    ];
}
