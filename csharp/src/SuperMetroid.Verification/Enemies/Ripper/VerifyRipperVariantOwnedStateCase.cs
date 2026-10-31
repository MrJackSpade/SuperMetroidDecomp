using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Enemies.Ripper;

/// <summary>The velocity and speed row of one verify ripper variant owned state case.</summary>
/// <param name="First">The velocity.</param>
/// <param name="Second">The speed row.</param>
internal readonly record struct VerifyRipperVariantOwnedStateCase(SignedSixteenSixteen First, EnemyLinearSpeedRow Second);
