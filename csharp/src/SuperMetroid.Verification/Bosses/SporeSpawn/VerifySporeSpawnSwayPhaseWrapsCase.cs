namespace SuperMetroid.Verification.Bosses.SporeSpawn;

/// <summary>The start, delta and expected of one verify spore spawn sway phase wraps case.</summary>
/// <param name="Start">The start.</param>
/// <param name="Delta">The delta.</param>
/// <param name="Expected">The expected.</param>
internal readonly record struct VerifySporeSpawnSwayPhaseWrapsCase(byte Start, int Delta, byte Expected);
