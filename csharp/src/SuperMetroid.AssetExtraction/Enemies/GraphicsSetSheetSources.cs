using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>What a room graphics set's pre-coloured sheets compose (#1392).</summary>
/// <param name="Enemies">Each enemy sheet, by the enemy drawn under the set.</param>
/// <param name="SpawnsFromSheets">Whether the set's spawned objects draw from sheets.</param>
/// <param name="Spawned">Each spawned-object sheet, by its plan; none when the set's spawned objects draw from characters.</param>
internal sealed record GraphicsSetSheetSources(IReadOnlyList<EnemySheetSource> Enemies, bool SpawnsFromSheets,
    IReadOnlyList<SpawnedObjectSheetSource> Spawned);

/// <summary>What one enemy's sheet composes under a graphics set.</summary>
/// <param name="Enemy">The enemy.</param>
/// <param name="Sources">Its sheet's sources.</param>
internal sealed record EnemySheetSource(EnemyDefinitionId Enemy, SpriteSheetSources Sources);

/// <summary>What one spawned-object sheet of a graphics set composes.</summary>
/// <param name="Plan">The tile base, palette and spawners it holds.</param>
/// <param name="Sources">Its sheet's sources.</param>
internal sealed record SpawnedObjectSheetSource(SpawnedObjectSheetPlan Plan, SpriteSheetSources Sources);
