namespace SuperMetroid.Verification.Enemies.Metaree;

/// <summary>The samus offset and expected of one verify metaree dive speed case.</summary>
/// <param name="SamusOffset">The samus offset.</param>
/// <param name="Expected">The expected.</param>
internal readonly record struct VerifyMetareeDiveSpeedCase(int SamusOffset, int Expected);
