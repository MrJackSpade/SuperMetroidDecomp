using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Ceres room-header region, in table order.</summary>
internal static class CeresRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresElevDefault), [
            new(EnemyDefinitionId.CeresDoor),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FallingTileDefault), [
            new(EnemyDefinitionId.CeresDoor),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MagnetStairsDefault), [
            new(EnemyDefinitionId.CeresDoor),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DeadScientistDefault), [
            new(EnemyDefinitionId.CeresDoor),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Room58EscapeDefault), [
            new(EnemyDefinitionId.CeresDoor),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresRidleyDefault), [
            new(EnemyDefinitionId.RidleyCeres),
            new(EnemyDefinitionId.CeresDoor),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresElevWhenAreaBossDead), [
            new(EnemyDefinitionId.CeresDoor),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FallingTileWhenAreaBossDead), [
            new(EnemyDefinitionId.CeresDoor),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MagnetStairsWhenAreaBossDead), [
            new(EnemyDefinitionId.CeresDoor),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DeadScientistWhenAreaBossDead), [
            new(EnemyDefinitionId.CeresDoor),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Room58EscapeWhenAreaBossDead), [
            new(EnemyDefinitionId.CeresDoor),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresRidleyWhenAreaBossDead), [
            new(EnemyDefinitionId.CeresDoor),
        ]),
    ];
}
