namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>The native trail interpreter's pair of sparkles (#2458), run beside the port's trail.</summary>
/// <param name="Left">The left sparkle.</param>
/// <param name="Right">The right sparkle.</param>
internal sealed record NativeTrail(NativeTrailSide Left, NativeTrailSide Right);
