namespace SuperMetroid.Verification.Enemies.Corpses;

/// <summary>The source, destination and length of one dead torizo artwork copy.</summary>
/// <param name="Source">The source.</param>
/// <param name="Destination">The destination.</param>
/// <param name="Length">The length.</param>
internal readonly record struct DeadTorizoArtworkCopy(int Source, int Destination, int Length);
