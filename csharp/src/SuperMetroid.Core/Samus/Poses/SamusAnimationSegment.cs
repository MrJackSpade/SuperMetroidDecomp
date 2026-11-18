namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A named segment of a Samus animation (#2048, #2395): frames each shown for their delay, then the cue that ends the segment.
/// </summary>
/// <param name="Name">What the segment is for, unique within its animation.</param>
/// <param name="Delays">Each frame's delay, in ticks, in frame order.</param>
/// <param name="End">The cue the frame after the delays runs.</param>
internal sealed record SamusAnimationSegment(SamusAnimationSegmentName Name, IReadOnlyList<int> Delays, SamusAnimationCue End)
{
    /// <summary>The frame of the segment its end cue is on, after its delays.</summary>
    internal int CueFrame => Delays.Count;
}
