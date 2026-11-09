using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>What one spawned-object sheet of a graphics set composes.</summary>
/// <param name="Plan">The tile base, palette and spawners it holds.</param>
/// <param name="Sources">Its sheet's sources.</param>
/// <param name="LookChanges">How encounters' looks change each of its frames they change, by frame name (#1538).</param>
/// <param name="CorpsePictures">Where a rotting corpse lies in each of its frames that shows one, by frame name (#1539).</param>
/// <param name="Empty">The names of its frames' finished pictures that show nothing, which no sheet holds (#1538).</param>
internal sealed record SpawnedObjectSheetSource(SpawnedObjectSheetPlan Plan, SpriteSheetSources Sources,
    IReadOnlyDictionary<string, IReadOnlyList<FrameLookChange>> LookChanges, IReadOnlyDictionary<string, CorpsePicturePlacement> CorpsePictures,
    IReadOnlySet<string> Empty);
