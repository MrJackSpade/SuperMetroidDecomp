namespace SuperMetroid.Verification.Enemies.Hoppers;

/// <summary>The falling, start, step, expected and expected falling of one verify hopper hop arc record case.</summary>
/// <param name="Falling">The falling.</param>
/// <param name="Start">The start.</param>
/// <param name="Step">The step.</param>
/// <param name="Expected">The expected.</param>
/// <param name="ExpectedFalling">The expected falling.</param>
internal readonly record struct VerifyHopperHopArcRecordCase(bool Falling, int Start, int Step, int Expected, bool ExpectedFalling);
