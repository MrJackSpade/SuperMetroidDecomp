namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One physical bomb's fuse and initial velocity: its horizontal velocity, negative leftward, and its upward speed.
/// </summary>
internal readonly record struct BombSpreadLaunchDefinition(
    int FuseTimer, SignedSixteenSixteen XVelocity, UnsignedSixteenSixteen YSpeed);
