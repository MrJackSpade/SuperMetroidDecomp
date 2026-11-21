namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A named segment of a Samus animation (#2048, #2395, #2530): frames each shown for their delay, then the cue that ends
/// the segment, or, for a lead, no cue: its last frame plays straight on into the next segment's first, as the frames of
/// one run do. A repeat names the segment it plays again (<see cref="SamusAnimationRepeat"/>), so the frames an animation
/// shows once before the frames it repeats are a lead of their own.
/// </summary>
/// <param name="Name">What the segment is for, unique within its animation.</param>
/// <param name="Delays">Each frame's delay, in ticks, in frame order.</param>
/// <param name="End">The cue the frame after the delays runs; null for a lead, which ends in no cue of its own.</param>
internal sealed partial record SamusAnimationSegment(SamusAnimationSegmentName Name, IReadOnlyList<int> Delays, SamusAnimationCue? End)
{
    /// <summary>A lead named <paramref name="name"/>: frames held for <paramref name="delays"/> that play on into the next segment.</summary>
    internal SamusAnimationSegment(SamusAnimationSegmentName name, IReadOnlyList<int> delays) : this(name, delays, null)
    {
    }

    /// <summary>The frame of the segment its end cue is on, after its delays; a lead's next segment starts there instead.</summary>
    internal int CueFrame => Delays.Count;
}
