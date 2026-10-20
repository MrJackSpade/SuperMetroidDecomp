namespace SuperMetroid.Core.Enemies;

/// <summary>The bank-$B4 graphics sets of the Ceres room-header region, in native table order.</summary>
internal static class CeresRoomEnemyGraphicsSetDefinitions
{
    /// <summary>The region's lists, in native table order.</summary>
    internal static RoomEnemyGraphicsSetDefinition[] Build() =>
    [
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresElev0), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FallingTile0), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MagnetStairs0), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DeadScientist0), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Room58Escape0), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresRidley0), [
            new(EnemyDefinitionId.RidleyCeres, PaletteRow: 1),
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresElev1), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.FallingTile1), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.MagnetStairs1), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.DeadScientist1), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.Room58Escape1), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
        new(nameof(RoomEnemyGraphicsSetDefinitions.CeresRidley1), [
            new(EnemyDefinitionId.CeresDoor, PaletteRow: 2, StagingBlock: 1),
        ]),
    ];
}
