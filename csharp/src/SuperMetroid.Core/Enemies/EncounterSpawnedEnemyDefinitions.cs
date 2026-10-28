namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemies each enemy's encounter spawns into its room beyond the room's own population (#1435), which the room's
/// graphics set therefore draws: Mother Brain's fight drops the tubes of her tank as it breaks and brings in the Baby
/// Metroid.
/// </summary>
internal static class EncounterSpawnedEnemyDefinitions
{
    /// <summary>The enemies each spawner's encounter brings into its room.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, EnemyDefinitionId[]> BySpawner { get; } = new Dictionary<EnemyDefinitionId, EnemyDefinitionId[]>
    {
        [EnemyDefinitionId.MotherBrainBody] = [EnemyDefinitionId.MotherBrainTubes, EnemyDefinitionId.BabyMetroidCutscene],
    };
}
