namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>One Phantoon flame frame: its animation and its position there.</summary>
/// <param name="Kind">The animation the frame belongs to.</param>
/// <param name="Phase">The frame's position in that animation.</param>
internal readonly record struct PhantoonFlameFrame(PhantoonFlameFrameKind Kind, int Phase);