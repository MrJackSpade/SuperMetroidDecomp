namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One Draygon projectile frame: its animation and its position there.</summary>
/// <param name="Kind">The animation the frame belongs to.</param>
/// <param name="Phase">The frame's position in that animation.</param>
public readonly record struct DraygonProjectileFrame(DraygonProjectileFrameKind Kind, int Phase);
