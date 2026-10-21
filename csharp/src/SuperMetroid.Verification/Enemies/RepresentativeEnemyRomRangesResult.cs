namespace SuperMetroid.Verification.Enemies;

/// <summary>The start and byte length of one representative enemy rom ranges result.</summary>
/// <param name="Start">The start.</param>
/// <param name="ByteLength">The byte length.</param>
internal readonly record struct RepresentativeEnemyRomRangesResult(int Start, int ByteLength);
