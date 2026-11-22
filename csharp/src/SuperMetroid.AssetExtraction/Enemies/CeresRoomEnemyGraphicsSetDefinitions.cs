using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Ceres room-header region, in table order.</summary>
internal static partial class CeresRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.CeresElevatorDefault, [
            new(UnplacedEnemyArtwork.CeresDoors, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FallingTileDefault, [
            new(UnplacedEnemyArtwork.CeresDoors, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MagnetStairsDefault, [
            new(UnplacedEnemyArtwork.CeresDoors, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.DeadScientistDefault, [
            new(UnplacedEnemyArtwork.CeresDoors, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CeresRidleyHallwayDefault, [
            new(UnplacedEnemyArtwork.CeresDoors, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CeresRidleyDefault, [
            new(EnemyDefinitionId.RidleyCeres, row: 1),
            new(UnplacedEnemyArtwork.CeresDoors, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CeresElevatorWhenAreaBossDead, [
            new(UnplacedEnemyArtwork.CeresDoors, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FallingTileWhenAreaBossDead, [
            new(UnplacedEnemyArtwork.CeresDoors, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MagnetStairsWhenAreaBossDead, [
            new(UnplacedEnemyArtwork.CeresDoors, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.DeadScientistWhenAreaBossDead, [
            new(UnplacedEnemyArtwork.CeresDoors, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CeresRidleyHallwayWhenAreaBossDead, [
            new(UnplacedEnemyArtwork.CeresDoors, row: 2),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CeresRidleyWhenAreaBossDead, [
            new(UnplacedEnemyArtwork.CeresDoors, row: 2),
        ]),
    ];
}
