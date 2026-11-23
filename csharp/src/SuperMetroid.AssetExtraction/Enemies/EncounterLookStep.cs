namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>One look of an encounter and what it copies, in the order its copies land.</summary>
/// <param name="Look">The look.</param>
/// <param name="Copies">What the look copies: nothing for the room's load.</param>
internal sealed partial record EncounterLookStep(ComposedLook Look, IReadOnlyList<EncounterArtworkUpload> Copies);
