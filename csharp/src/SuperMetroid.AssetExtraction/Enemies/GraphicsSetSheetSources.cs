namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>What a room graphics set's pre-coloured sheets compose (#1392).</summary>
/// <param name="Enemies">Each enemy sheet, by the enemy drawn under the set.</param>
/// <param name="Spawned">Each spawned-object sheet, by its plan.</param>
internal sealed record GraphicsSetSheetSources(IReadOnlyList<EnemySheetSource> Enemies, IReadOnlyList<SpawnedObjectSheetSource> Spawned);
