namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A Samus pose's animation (#1473, #2048): named segments, each frames held for their delays and the cue that ends the
/// segment. Samus's animation stands on a frame of one segment (<see cref="SamusAnimationFrame"/>); a frame on a value a cue
/// carries, or past the last segment, names no step of the animation and is read as the game reads it
/// (<see cref="SamusAnimationDelays.TryStepFor"/>).
/// </summary>
/// <remarks>
/// The game lays an animation's segments out one after another, each as long as its frames, its cue and the values the cue
/// carries. That layout is what a read past the animation's end, a read of a cue's value and a frame written for one
/// animation onto another follow (<see cref="OffsetOf"/>, <see cref="FrameAt"/>); the animation's own playback moves
/// from segment to segment by name and frame.
/// </remarks>
internal sealed class SamusPoseAnimation
{
    /// <summary>The segments, in order.</summary>
    private readonly SamusAnimationSegment[] segments;

    /// <summary>How far into the game's layout of the animation each segment starts.</summary>
    private readonly int[] starts;

    /// <summary>The step at each place of the layout up to the last segment's end cue, or none for a cue's value.</summary>
    private readonly SamusPoseAnimationStep?[] steps;

    /// <summary>Lays out <paramref name="segments"/>, each after the one before it.</summary>
    /// <exception cref="ArgumentException">There is no segment, or two segments share a name.</exception>
    internal SamusPoseAnimation(IReadOnlyList<SamusAnimationSegment> segments)
    {
        Ensure.NotNull(segments);
        if (segments.Count == 0)
            throw new ArgumentException("An animation holds at least one segment.", nameof(segments));
        if (segments.Select(segment => segment.Name).Distinct().Count() != segments.Count)
            throw new ArgumentException("Two segments of an animation share a name.", nameof(segments));
        this.segments = [.. segments];
        starts = new int[segments.Count];
        for (int index = 1; index < segments.Count; index++)
            starts[index] = starts[index - 1] + segments[index - 1].Length;
        SamusAnimationSegment last = segments[^1];
        steps = new SamusPoseAnimationStep?[starts[^1] + last.Delays.Count + 1];
        for (int index = 0; index < segments.Count; index++)
        {
            SamusAnimationSegment segment = segments[index];
            for (int frame = 0; frame < segment.Delays.Count; frame++)
                steps[starts[index] + frame] = new SamusAnimationDelay(segment.Delays[frame]);
            steps[starts[index] + segment.CueFrame] = segment.End;
        }
    }

    /// <summary>The segments, in order.</summary>
    internal IReadOnlyList<SamusAnimationSegment> Segments => segments;

    /// <summary>How many frames the animation's own steps span in the game's layout, its last end cue included.</summary>
    internal int FrameCount => steps.Length;

    /// <summary>The animation's first frame.</summary>
    internal SamusAnimationPlace Start => new(this, SamusAnimationFrame.Start);

    /// <summary>Frame <paramref name="frame"/> of the segment named <paramref name="segment"/>.</summary>
    /// <exception cref="ArgumentException">The animation has no such segment, or the frame lies past a segment that is not its last.</exception>
    internal SamusAnimationFrame Frame(SamusAnimationSegmentName segment, int frame)
    {
        int index = Array.FindIndex(segments, candidate => candidate.Name == segment);
        if (index < 0)
            throw new ArgumentException($"The animation has no segment {segment}.", nameof(segment));
        if (frame < 0 || (index < segments.Length - 1 && frame >= segments[index].Length))
            throw new ArgumentException($"Segment {segment} has no frame {frame}.", nameof(frame));
        return new SamusAnimationFrame(index, frame);
    }

    /// <summary>Frame <paramref name="frame"/> of the segment named <paramref name="segment"/>, as a path names it.</summary>
    internal SamusAnimationPlace Place(SamusAnimationSegmentName segment, int frame) => new(this, Frame(segment, frame));

    /// <summary>The frame <paramref name="target"/> names in this animation.</summary>
    internal SamusAnimationFrame Resolve(SamusAnimationTarget target) => Frame(target.Segment, target.Frame);

    /// <summary>The name of the segment <paramref name="frame"/> is a frame of.</summary>
    internal SamusAnimationSegmentName SegmentName(SamusAnimationFrame frame) => SegmentOf(frame).Name;

    /// <summary>The step of the animation <paramref name="frame"/> is on, or null when it is on a cue's value or past the animation.</summary>
    internal SamusPoseAnimationStep? TryStepAt(SamusAnimationFrame frame)
    {
        SamusAnimationSegment segment = SegmentOf(frame);
        return frame.Frame < segment.Delays.Count ? new SamusAnimationDelay(segment.Delays[frame.Frame])
            : frame.Frame == segment.CueFrame ? segment.End
            : null;
    }

    /// <summary>The frame after <paramref name="frame"/>: the next of its segment, or the next segment's first.</summary>
    internal SamusAnimationFrame Next(SamusAnimationFrame frame) => FrameAt(OffsetOf(frame) + 1);

    /// <summary>
    /// How far into the game's layout of the animation <paramref name="frame"/> lies: what a read past the animation, a read
    /// of a cue's value, a frame written for another animation and the installed pictures' numbering follow.
    /// </summary>
    internal int OffsetOf(SamusAnimationFrame frame)
    {
        _ = SegmentOf(frame);
        return starts[frame.Segment] + frame.Frame;
    }

    /// <summary>The frame <paramref name="offset"/> places into the game's layout of the animation lies at.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The offset is negative.</exception>
    internal SamusAnimationFrame FrameAt(int offset)
    {
        Ensure.AtLeastZero(offset);
        int index = segments.Length - 1;
        while (starts[index] > offset)
            index--;
        return new SamusAnimationFrame(index, offset - starts[index]);
    }

    /// <summary>The target naming the frame <paramref name="offset"/> places into the game's layout of the animation.</summary>
    internal SamusAnimationTarget TargetAt(int offset)
    {
        SamusAnimationFrame frame = FrameAt(offset);
        return new SamusAnimationTarget(segments[frame.Segment].Name, frame.Frame);
    }

    /// <summary>The step at <paramref name="offset"/> places into the game's layout, or null for a cue's value or a place past it.</summary>
    internal SamusPoseAnimationStep? TryStepAtOffset(int offset) => (uint)offset < (uint)steps.Length ? steps[offset] : null;

    /// <summary>The segment <paramref name="frame"/> is a frame of.</summary>
    /// <exception cref="ArgumentException">The animation has no such segment, or the frame lies past a segment that is not its last.</exception>
    private SamusAnimationSegment SegmentOf(SamusAnimationFrame frame) =>
        (uint)frame.Segment < (uint)segments.Length && frame.Frame >= 0 &&
        (frame.Segment == segments.Length - 1 || frame.Frame < segments[frame.Segment].Length)
            ? segments[frame.Segment]
            : throw new ArgumentException($"The animation has no frame {frame}.", nameof(frame));
}
