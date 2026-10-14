namespace SuperMetroid.Core.Game;

/// <summary>One Draygon projectile frame: its program and its position there.</summary>
/// <param name="Kind">The program the frame belongs to.</param>
/// <param name="Phase">The frame's position in that program.</param>
public readonly record struct DraygonProjectileFrame(DraygonProjectileFrameKind Kind, int Phase);
