using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>The first and second of one verify projectile trail artwork case.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
internal readonly record struct VerifyProjectileTrailArtworkCase(SamusProjectileTrailSide First, SamusProjectileTrailSide Second);
