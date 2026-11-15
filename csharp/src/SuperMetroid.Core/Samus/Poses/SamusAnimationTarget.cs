namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The frame a cue moves its animation to (#2048): a frame of one of the animation's named segments. A cue read on a value
/// another cue carries may name a frame past the animation's last segment, as <paramref name="Frame"/> of that segment.
/// </summary>
/// <param name="Segment">The named segment.</param>
/// <param name="Frame">The frame within the segment, from zero.</param>
internal readonly record struct SamusAnimationTarget(SamusAnimationSegmentName Segment, int Frame);
