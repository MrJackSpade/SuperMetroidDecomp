namespace SuperMetroid.Verification.Enemies.WorkRobots;

/// <summary>The whole, fraction, expected whole and expected fraction of one verify work robot fall case.</summary>
/// <param name="Whole">The whole.</param>
/// <param name="Fraction">The fraction.</param>
/// <param name="ExpectedWhole">The expected whole.</param>
/// <param name="ExpectedFraction">The expected fraction.</param>
internal readonly record struct VerifyWorkRobotFallCase(short Whole, ushort Fraction, ushort ExpectedWhole, ushort ExpectedFraction);
