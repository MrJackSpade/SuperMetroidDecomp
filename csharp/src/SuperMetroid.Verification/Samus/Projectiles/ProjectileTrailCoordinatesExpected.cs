namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>A trail's four coordinates expected, each as its offset from the shot that left the trail.</summary>
/// <param name="First">The left sparkle's horizontal offset.</param>
/// <param name="Second">The left sparkle's vertical offset.</param>
/// <param name="Third">The right sparkle's horizontal offset.</param>
/// <param name="Fourth">The right sparkle's vertical offset.</param>
internal readonly record struct ProjectileTrailCoordinatesExpected(int First, int Second, int Third, int Fourth);
