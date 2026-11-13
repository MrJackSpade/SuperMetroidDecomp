namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>One Kraid rock frame: its animation and its position there.</summary>
/// <param name="Animation">The animation the frame belongs to.</param>
/// <param name="Phase">The frame's position in that animation.</param>
public readonly record struct KraidRockFrame(KraidRockSegment Animation, int Phase);
