using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>What one spawned-object sheet of a graphics set composes.</summary>
/// <param name="Plan">The tile base, palette and spawners it holds.</param>
/// <param name="Sources">Its sheet's sources.</param>
/// <param name="Variations">How each of its frames an encounter's artwork sequence changes follows it, by frame name (#1513).</param>
/// <param name="RotPictures">The kind of corpse whose rot each of its frames drawn over a corpse's rows shows, by frame name (#1406).</param>
/// <param name="BlankRotPictures">The names of its frames' pictures of the rots at which they show nothing, which it does not hold (#1513).</param>
internal sealed record SpawnedObjectSheetSource(SpawnedObjectSheetPlan Plan, SpriteSheetSources Sources,
    IReadOnlyDictionary<string, FrameArtworkVariation[]> Variations, IReadOnlyDictionary<string, string> RotPictures,
    IReadOnlySet<string> BlankRotPictures);
