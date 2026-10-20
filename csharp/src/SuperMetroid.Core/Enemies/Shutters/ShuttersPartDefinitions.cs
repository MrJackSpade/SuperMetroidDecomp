namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>The definitions the Shutters species installs and the part each one is.</summary>
internal static class ShuttersPartDefinitions
{
    /// <summary>Each Shutters definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, ShuttersPart> Parts { get; } = new Dictionary<EnemyDefinitionId, ShuttersPart>
    {
        [EnemyDefinitionId.Kamer] = ShuttersPart.Kamer,
        [EnemyDefinitionId.ShutterDestroyable] = ShuttersPart.ShutterDestroyable,
        [EnemyDefinitionId.ShutterGrowing] = ShuttersPart.ShutterGrowing,
        [EnemyDefinitionId.ShutterHorizShootable] = ShuttersPart.ShutterHorizShootable,
        [EnemyDefinitionId.ShutterShootable] = ShuttersPart.ShutterShootable,
    };
}
