namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One Crocomire projectile frame.</summary>
/// <param name="Kind">The program the frame belongs to.</param>
/// <param name="Phase">The frame's position in that program.</param>
public readonly record struct CrocomireProjectileFrame(CrocomireProjectileFrameKind Kind, int Phase);
