namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>A run of consecutive frames of the extended frames document that belong to one enemy family (#1623).</summary>
/// <param name="Family">The family the run's frames belong to.</param>
/// <param name="Count">How many frames the run holds.</param>
internal readonly record struct EnemyExtendedFrameRun(EnemyExtendedFrameFamily Family, int Count);
