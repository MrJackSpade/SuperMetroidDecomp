using System.Globalization;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A Samus pose's animation (#1473, #2048): named segments, each frames held for their delays and the cue that ends the
/// segment. Samus's animation stands on a frame of one segment (<see cref="SamusAnimationFrame"/>) and plays on from
/// segment to segment by name and frame; a frame on a value a cue carries, or past the last segment, names no step of the
/// animation and reads the outcome the import decoded for it (<see cref="SamusAnimationDelays.TryStepFor"/>).
/// </summary>
internal sealed class SamusPoseAnimation
{
    /// <summary>The segments, in order.</summary>
    private readonly SamusAnimationSegment[] segments;

    /// <summary>Keeps the animation's <paramref name="name"/> and <paramref name="segments"/>, in the order the animation plays them.</summary>
    /// <exception cref="ArgumentException">There is no segment, or two segments share a name.</exception>
    internal SamusPoseAnimation(SamusAnimationName name, IReadOnlyList<SamusAnimationSegment> segments)
    {
        Ensure.NotNull(segments);
        Name = name;
        if (segments.Count == 0)
            throw new ArgumentException("An animation holds at least one segment.", nameof(segments));
        if (segments.Select(segment => segment.Name).Distinct().Count() != segments.Count)
            throw new ArgumentException("Two segments of an animation share a name.", nameof(segments));
        this.segments = [.. segments];
    }

    /// <summary>The animation's name, which the installation keys its outcomes by.</summary>
    internal SamusAnimationName Name { get; }

    /// <summary>The segments, in order.</summary>
    internal IReadOnlyList<SamusAnimationSegment> Segments => segments;

    /// <summary>The segment named <paramref name="name"/>.</summary>
    /// <exception cref="ArgumentException">The animation has no such segment.</exception>
    internal SamusAnimationSegment Segment(SamusAnimationSegmentName name) =>
        Array.Find(segments, candidate => candidate.Name == name) ??
        throw new ArgumentException($"Animation {Name} has no segment {name}.", nameof(name));

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

    /// <summary>The place named <paramref name="name"/>: frame <paramref name="frame"/> of the segment named <paramref name="segment"/>.</summary>
    internal SamusAnimationPlace Place(SamusAnimationPlaceName name, SamusAnimationSegmentName segment, int frame) =>
        new(name, this, Frame(segment, frame));

    /// <summary>The frame <paramref name="target"/> names in this animation.</summary>
    internal SamusAnimationFrame Resolve(SamusAnimationTarget target) => Frame(target.Segment, target.Frame);

    /// <summary>The name of the segment <paramref name="frame"/> is a frame of.</summary>
    internal SamusAnimationSegmentName SegmentName(SamusAnimationFrame frame) => SegmentOf(frame).Name;

    /// <summary>
    /// <paramref name="frame"/>'s name, as the installation keys a frame of this animation: its segment's name and the frame
    /// within the segment.
    /// </summary>
    internal string NameOf(SamusAnimationFrame frame) =>
        $"{SegmentName(frame)}.{frame.Frame.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>The frame of this animation the installation names <paramref name="name"/> (<see cref="NameOf"/>).</summary>
    /// <exception cref="InvalidDataException">The name names no frame of this animation.</exception>
    internal SamusAnimationFrame FrameNamed(string? name)
    {
        int separator = name?.LastIndexOf('.') ?? -1;
        if (name is null || separator <= 0 || char.IsDigit(name[0]) ||
            !Enum.TryParse(name.AsSpan(0, separator), ignoreCase: false, out SamusAnimationSegmentName segment) || !Enum.IsDefined(segment) ||
            !int.TryParse(name.AsSpan(separator + 1), NumberStyles.None, CultureInfo.InvariantCulture, out int frame))
            throw new InvalidDataException($"'{name}' names no animation frame.");
        int index = Array.FindIndex(segments, candidate => candidate.Name == segment);
        return index >= 0 && (index == segments.Length - 1 || frame < segments[index].Length)
            ? new SamusAnimationFrame(index, frame)
            : throw new InvalidDataException($"'{name}' names no frame of the animation.");
    }

    /// <summary>The step of the animation <paramref name="frame"/> is on, or null when it is on a cue's value or past the animation.</summary>
    internal SamusPoseAnimationStep? TryStepAt(SamusAnimationFrame frame)
    {
        SamusAnimationSegment segment = SegmentOf(frame);
        return frame.Frame < segment.Delays.Count ? new SamusAnimationDelay(segment.Delays[frame.Frame])
            : frame.Frame == segment.CueFrame ? segment.End
            : null;
    }

    /// <summary>
    /// The frame after <paramref name="frame"/>: the next of its segment, or the next segment's first once the segment's
    /// frames, its cue and the values the cue carries are behind it. Past the last segment the frames run on.
    /// </summary>
    internal SamusAnimationFrame Next(SamusAnimationFrame frame)
    {
        SamusAnimationSegment segment = SegmentOf(frame);
        return frame.Segment < segments.Length - 1 && frame.Frame + 1 == segment.Length
            ? new SamusAnimationFrame(frame.Segment + 1, 0)
            : frame with { Frame = frame.Frame + 1 };
    }

    /// <summary>The segment <paramref name="frame"/> is a frame of.</summary>
    /// <exception cref="ArgumentException">The animation has no such segment, or the frame lies past a segment that is not its last.</exception>
    private SamusAnimationSegment SegmentOf(SamusAnimationFrame frame) =>
        frame.Segment >= 0 && frame.Segment < segments.Length && frame.Frame >= 0 &&
        (frame.Segment == segments.Length - 1 || frame.Frame < segments[frame.Segment].Length)
            ? segments[frame.Segment]
            : throw new ArgumentException($"The animation has no frame {frame}.", nameof(frame));
}
