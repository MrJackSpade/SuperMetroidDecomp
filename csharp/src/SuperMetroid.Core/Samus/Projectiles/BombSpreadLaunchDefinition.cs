namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>One physical bomb's fuse and initial velocity: its horizontal direction and speed, and its upward speed.</summary>
internal readonly record struct BombSpreadLaunchDefinition(
    int FuseTimer, BombSpreadHorizontalVelocity XVelocity, UnsignedSixteenSixteen YSpeed);
