namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One Crocomire projectile frame.</summary>
/// <param name="Kind">The animation the frame belongs to.</param>
/// <param name="Phase">The frame's position in that animation.</param>
public readonly record struct CrocomireProjectileFrame(CrocomireProjectileFrameKind Kind, int Phase);
