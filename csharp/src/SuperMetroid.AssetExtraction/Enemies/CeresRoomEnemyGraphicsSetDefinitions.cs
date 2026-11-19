using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Ceres room-header region, in table order.</summary>
internal static class CeresRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(RoomEnemyGraphicsSetDefinitions.CeresElevatorDefault, [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FallingTileDefault, [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MagnetStairsDefault, [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.DeadScientistDefault, [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CeresRidleyHallwayDefault, [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CeresRidleyDefault, [
            new(EnemyDefinitionId.RidleyCeres),
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CeresElevatorWhenAreaBossDead, [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.FallingTileWhenAreaBossDead, [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.MagnetStairsWhenAreaBossDead, [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.DeadScientistWhenAreaBossDead, [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CeresRidleyHallwayWhenAreaBossDead, [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(RoomEnemyGraphicsSetDefinitions.CeresRidleyWhenAreaBossDead, [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
    ];
}
