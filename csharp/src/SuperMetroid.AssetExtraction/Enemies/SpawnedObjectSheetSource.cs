using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>What one spawned-object sheet of a graphics set composes.</summary>
/// <param name="Plan">The tile base, palette and spawners it holds.</param>
/// <param name="Sources">Its sheet's sources.</param>
/// <param name="Variations">How each of its frames an encounter's artwork sequence changes follows it, by frame name (#1513).</param>
/// <param name="CorpsePictures">Where a rotting corpse lies in each of its frames that shows one, by frame name (#1531).</param>
/// <param name="BlankPictures">The names of its frames' pictures that show nothing, which it does not hold (#1513).</param>
internal sealed record SpawnedObjectSheetSource(SpawnedObjectSheetPlan Plan, SpriteSheetSources Sources,
    IReadOnlyDictionary<string, FrameArtworkVariation[]> Variations, IReadOnlyDictionary<string, CorpsePictureLayout> CorpsePictures,
    IReadOnlySet<string> BlankPictures);
