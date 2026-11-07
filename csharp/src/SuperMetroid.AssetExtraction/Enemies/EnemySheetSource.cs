using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>What one enemy's sheet composes under a graphics set.</summary>
/// <param name="Enemy">The enemy.</param>
/// <param name="Sources">Its sheet's sources, the names of its frames' finished pictures that show nothing among them (#1538).</param>
/// <param name="LookChanges">How encounters' looks change each of its frames they change, by frame name (#1538).</param>
/// <param name="CorpsePictures">Where a rotting corpse lies in each of its frames that shows one, by frame name (#1531).</param>
internal sealed record EnemySheetSource(EnemyDefinitionId Enemy, SpriteSheetSources Sources,
    IReadOnlyDictionary<string, IReadOnlyList<FrameLookChange>> LookChanges, IReadOnlyDictionary<string, CorpsePictureLayout> CorpsePictures);
