namespace SuperMetroid.Core.Enemies;

/// <summary>The bank-$A1 placement lists of the Ceres room-header region, in native table order.</summary>
internal static class CeresRoomEnemyPopulationDefinitions
{
    /// <summary>The region's lists, in native table order.</summary>
    internal static RoomEnemyPopulationDefinition[] Build() =>
    [
        new(nameof(RoomEnemyPopulationDefinitions.CeresElev0),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x00e8, 0x0277), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x00e0, 0x027f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(RoomEnemyPopulationDefinitions.FallingTile0),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x01e0, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(RoomEnemyPopulationDefinitions.MagnetStairs0),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x00e0, 0x017f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(RoomEnemyPopulationDefinitions.DeadScientist0),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x01e0, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(RoomEnemyPopulationDefinitions.Room58Escape0),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x01e0, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(RoomEnemyPopulationDefinitions.CeresRidley0),
        [
            new(EnemyDefinitionId.RidleyCeres, new(0x00ba, 0x00ab), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x0010, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0003, 0x0000),
        ], 0x00),
        new(nameof(RoomEnemyPopulationDefinitions.CeresElev1),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x00e8, 0x0277), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x00e0, 0x027f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x005e, 0x006c), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00a2, 0x006c), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0005, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x009e, 0x00c8), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0062, 0x00c8), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0005, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x005e, 0x0128), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00a2, 0x0128), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0005, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x009e, 0x0188), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0062, 0x0198), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0005, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x005e, 0x01f8), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0004, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00a2, 0x01f8), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0005, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0072, 0x0258), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0005, 0x0000),
        ], 0x00),
        new(nameof(RoomEnemyPopulationDefinitions.FallingTile1),
        [
            new(EnemyDefinitionId.Steam, new(0x00a0, 0x00cc), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00e0, 0x00bc), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0120, 0x00bc), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0160, 0x00cc), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x01b8, 0x00a8), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0080, 0x0042), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00c0, 0x0042), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0100, 0x0042), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0140, 0x0042), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0180, 0x0042), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0002, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0048, 0x00a8), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0003, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x01e0, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(RoomEnemyPopulationDefinitions.MagnetStairs1),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x00e0, 0x017f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0070, 0x009c), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x00c2, 0x00b0), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0003, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0078, 0x0116), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x003e, 0x0130), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0001, 0x0000),
            new(EnemyDefinitionId.Steam, new(0x0088, 0x0194), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0x2000), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
        ], 0x00),
        new(nameof(RoomEnemyPopulationDefinitions.DeadScientist1),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x01e0, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(RoomEnemyPopulationDefinitions.Room58Escape1),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0020, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0000, 0x0000),
            new(EnemyDefinitionId.CeresDoor, new(0x01e0, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0001, 0x0000),
        ], 0x00),
        new(nameof(RoomEnemyPopulationDefinitions.CeresRidley1),
        [
            new(EnemyDefinitionId.CeresDoor, new(0x0010, 0x007f), EnemySpawnSettings.None, EnemyPropertyWord.FromWord(0xa800), EnemyExtraPropertyWord.FromWord(0x0000), 0x0003, 0x0000),
        ], 0x00),
    ];
}
