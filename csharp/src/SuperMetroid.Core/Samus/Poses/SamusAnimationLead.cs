namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A lead of a Samus animation (#2554): frames held for their delays whose last plays straight on into the next segment's
/// first, with no cue of its own.
/// </summary>
/// <param name="Name">What the segment is for, unique within its animation.</param>
/// <param name="Delays">Each frame's delay, in ticks, in frame order.</param>
internal sealed record SamusAnimationLead(SamusAnimationSegmentName Name, IReadOnlyList<int> Delays)
    : SamusAnimationSegment(Name, Delays);
