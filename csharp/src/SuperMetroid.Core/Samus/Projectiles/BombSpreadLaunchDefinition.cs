namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// One spread bomb's fuse and launch velocity, in pixels per update: its horizontal velocity, negative leftward, and its
/// vertical velocity, negative upward, before any held-Down bonus (<see cref="SamusBombSpreadLaunches.Launch"/>).
/// </summary>
internal readonly record struct BombSpreadLaunchDefinition(
    int FuseTimer, SignedSixteenSixteen HorizontalVelocity, SignedSixteenSixteen VerticalVelocity);
