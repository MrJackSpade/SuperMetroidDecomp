using SuperMetroid.Core.Enemies;
namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The graphics sets of the Ceres room-header region, in table order.</summary>
internal static class CeresRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in table order.</summary>
    internal static RoomEnemyGraphicsSetMembers[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresElevDefault), [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FallingTileDefault), [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MagnetStairsDefault), [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DeadScientistDefault), [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresRidleyHallwayDefault), [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresRidleyDefault), [
            new(EnemyDefinitionId.RidleyCeres),
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresElevWhenAreaBossDead), [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FallingTileWhenAreaBossDead), [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MagnetStairsWhenAreaBossDead), [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DeadScientistWhenAreaBossDead), [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresRidleyHallwayWhenAreaBossDead), [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresRidleyWhenAreaBossDead), [
            new(UnplacedEnemyArtwork.CeresDoors),
        ]),
    ];
}
