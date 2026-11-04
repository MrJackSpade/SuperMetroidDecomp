using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>One encounter's artwork sequence and what each of its steps copies over the sprite characters (#1513).</summary>
/// <param name="Sequence">The sequence.</param>
/// <param name="Steps">What each step copies, from the first, in the order the copies land.</param>
internal sealed record EncounterArtworkSteps(EncounterArtworkSequence Sequence, IReadOnlyList<IReadOnlyList<EncounterArtworkUpload>> Steps);
