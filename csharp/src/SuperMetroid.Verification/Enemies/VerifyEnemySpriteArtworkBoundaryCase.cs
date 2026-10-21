namespace SuperMetroid.Verification.Enemies;

/// <summary>The x, y and clip of one verify enemy sprite artwork boundary case.</summary>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="Clip">The clip.</param>
internal readonly record struct VerifyEnemySpriteArtworkBoundaryCase(ushort X, ushort Y, bool Clip);
