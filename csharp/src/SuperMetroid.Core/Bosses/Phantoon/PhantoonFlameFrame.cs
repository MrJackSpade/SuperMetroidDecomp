namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>One Phantoon flame frame: its program and its position there.</summary>
/// <param name="Kind">The program the frame belongs to.</param>
/// <param name="Phase">The frame's position in that program.</param>
internal readonly record struct PhantoonFlameFrame(PhantoonFlameFrameKind Kind, int Phase);