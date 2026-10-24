using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>What a room graphics set's pre-coloured sheets compose (#1392).</summary>
/// <param name="Enemies">Each enemy sheet, by the enemy drawn under the set.</param>
/// <param name="Spawned">Each spawned-object sheet, by its plan.</param>
/// <param name="Regions">The set's regions, which name them by ordinal and know what covers each (#1406).</param>
internal sealed record GraphicsSetSheetSources(IReadOnlyList<EnemySheetSource> Enemies, IReadOnlyList<SpawnedObjectSheetSource> Spawned, SetRegions Regions);

/// <summary>What one enemy's sheet composes under a graphics set.</summary>
/// <param name="Enemy">The enemy.</param>
/// <param name="Sources">Its sheet's sources.</param>
/// <param name="Pictures">The layers each of its frames drawn over uploaded characters draws as, by frame name.</param>
/// <param name="RotPictures">The kind of corpse whose rot each of its frames drawn over a corpse's rows shows, by frame name (#1406).</param>
internal sealed record EnemySheetSource(EnemyDefinitionId Enemy, SpriteSheetSources Sources,
    IReadOnlyDictionary<string, CharacterRange[][]> Pictures, IReadOnlyDictionary<string, string> RotPictures);

/// <summary>What one spawned-object sheet of a graphics set composes.</summary>
/// <param name="Plan">The tile base, palette and spawners it holds.</param>
/// <param name="Sources">Its sheet's sources.</param>
/// <param name="Pictures">The layers each of its frames drawn over uploaded characters draws as, by frame name.</param>
/// <param name="RotPictures">The kind of corpse whose rot each of its frames drawn over a corpse's rows shows, by frame name (#1406).</param>
internal sealed record SpawnedObjectSheetSource(SpawnedObjectSheetPlan Plan, SpriteSheetSources Sources,
    IReadOnlyDictionary<string, CharacterRange[][]> Pictures, IReadOnlyDictionary<string, string> RotPictures);
