namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// A statue projectile pose: the splash's four, the eye glow's ten, the particle's four, the tail's four, the soul's two,
/// then the base decoration's, its resting pose, Ridley's and Phantoon's.
/// </summary>
internal readonly record struct TourianStatueProjectileFrame(int Pose);
