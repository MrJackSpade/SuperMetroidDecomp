using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>What one spawned-object sheet of a graphics set composes.</summary>
/// <param name="Plan">The tile base, palette and spawners it holds.</param>
/// <param name="Sources">Its sheet's sources.</param>
/// <param name="Pictures">The layers each of its frames drawn over uploaded characters draws as, by frame name.</param>
/// <param name="RotPictures">The kind of corpse whose rot each of its frames drawn over a corpse's rows shows, by frame name (#1406).</param>
internal sealed record SpawnedObjectSheetSource(SpawnedObjectSheetPlan Plan, SpriteSheetSources Sources,
    IReadOnlyDictionary<string, CharacterRange[][]> Pictures, IReadOnlyDictionary<string, string> RotPictures);
