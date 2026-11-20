namespace SuperMetroid.Core.Samus.Poses;

/// <summary>A segment of a Samus animation that ends in a cue (#2554): frames held for their delays, then the cue.</summary>
/// <param name="Name">What the segment is for, unique within its animation.</param>
/// <param name="Delays">Each frame's delay, in ticks, in frame order.</param>
/// <param name="End">The cue the frame after the delays runs.</param>
internal sealed record SamusCuedAnimationSegment(SamusAnimationSegmentName Name, IReadOnlyList<int> Delays, SamusAnimationCue End)
    : SamusAnimationSegment(Name, Delays);
