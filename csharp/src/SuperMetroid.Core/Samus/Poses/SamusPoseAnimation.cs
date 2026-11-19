using System.Globalization;
using SamusFramesBeyondAnimation = SuperMetroid.Core.Compatibility.FramesBeyondAnimation<SuperMetroid.Core.Samus.Poses.SamusAnimationName, SuperMetroid.Core.Samus.Poses.SamusAnimationFrame, SuperMetroid.Core.Samus.Poses.SamusAnimationPlaceName, SuperMetroid.Core.Samus.Poses.SamusPoseAnimationStep>;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A Samus pose's animation (#1473, #2048, #2395): named segments, each frames held for their delays and the cue that ends
/// the segment. Samus's animation stands on a frame of one segment (<see cref="SamusAnimationFrame"/>) and plays on from
/// segment to segment by name and frame; a frame past a segment's end cue, or past the last segment, names no step of the
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
        FrameOrder = Comparer<SamusAnimationFrame>.Create(Compare);
    }

    /// <summary>The animation's name, which the installation keys its outcomes by.</summary>
    internal SamusAnimationName Name { get; }

    /// <summary>The segments, in order.</summary>
    internal IReadOnlyList<SamusAnimationSegment> Segments => segments;

    /// <summary>The animation's first frame, which starting or restarting it sets.</summary>
    internal SamusAnimationFrame Start => new(segments[0].Name, 0);

    /// <summary>
    /// The frame after the animation's first: a spin starts on it, skipping its still first frame, and a standing pose aiming
    /// up entered from one keeps the gun raised on it.
    /// </summary>
    internal SamusAnimationFrame AfterStart => new(segments[0].Name, 1);

    /// <summary>Orders frames of this animation as it plays them: an earlier segment first, then an earlier frame.</summary>
    internal IComparer<SamusAnimationFrame> FrameOrder { get; }

    /// <summary>The segment named <paramref name="name"/>.</summary>
    /// <exception cref="ArgumentException">The animation has no such segment.</exception>
    internal SamusAnimationSegment Segment(SamusAnimationSegmentName name) => segments[IndexOf(name)];

    /// <summary>Frame <paramref name="frame"/> of the segment named <paramref name="segment"/>.</summary>
    /// <exception cref="ArgumentException">The animation has no such segment, or the frame is negative.</exception>
    internal SamusAnimationFrame Frame(SamusAnimationSegmentName segment, int frame) => Require(new SamusAnimationFrame(segment, frame));

    /// <summary><paramref name="frame"/>, which must be a frame of one of this animation's segments.</summary>
    /// <exception cref="ArgumentException">The animation has no such segment, or the frame is negative.</exception>
    internal SamusAnimationFrame Require(SamusAnimationFrame frame)
    {
        _ = IndexOf(frame.Segment);
        return frame.Frame >= 0 ? frame : throw new ArgumentException($"Animation {Name} has no frame {frame}.", nameof(frame));
    }

    /// <summary>The place named <paramref name="name"/>: frame <paramref name="frame"/> of the segment named <paramref name="segment"/>.</summary>
    internal SamusAnimationPlace Place(SamusAnimationPlaceName name, SamusAnimationSegmentName segment, int frame) =>
        new(name, this, Frame(segment, frame));

    /// <summary>
    /// <paramref name="frame"/>'s name, as the installation keys a frame of this animation: its segment's name and the frame
    /// within the segment.
    /// </summary>
    [InstalledKey]
    internal string NameOf(SamusAnimationFrame frame)
    {
        SamusAnimationFrame named = Require(frame);
        return $"{SamusAnimationSegmentNameKeys.Installed.Of(named.Segment)}.{named.Frame.ToString(CultureInfo.InvariantCulture)}";
    }

    /// <summary>The frame of this animation the installation names <paramref name="name"/> (<see cref="NameOf"/>).</summary>
    /// <exception cref="InvalidDataException">The name names no frame of this animation.</exception>
    internal SamusAnimationFrame FrameNamed(string? name)
    {
        int separator = name?.LastIndexOf('.') ?? -1;
        if (name is null || separator <= 0 || !SamusAnimationSegmentNameKeys.Installed.TryParse(name[..separator], out SamusAnimationSegmentName segment) ||
            !int.TryParse(name.AsSpan(separator + 1), NumberStyles.None, CultureInfo.InvariantCulture, out int frame))
            throw new InvalidDataException($"'{name}' names no animation frame.");
        return Array.Exists(segments, candidate => candidate.Name == segment)
            ? new SamusAnimationFrame(segment, frame)
            : throw new InvalidDataException($"'{name}' names no frame of the {Name} animation.");
    }

    /// <summary>The step of the animation <paramref name="frame"/> is on, or null when it lies past its segment's end cue.</summary>
    internal SamusPoseAnimationStep? TryStepAt(SamusAnimationFrame frame)
    {
        SamusAnimationSegment segment = Segment(Require(frame).Segment);
        return frame.Frame < segment.Delays.Count ? new SamusAnimationDelay(segment.Delays[frame.Frame])
            : frame.Frame == segment.CueFrame ? segment.End
            : null;
    }

    /// <summary>
    /// The frame after <paramref name="frame"/>: the next of its segment up to its end cue, and from a cue that continues
    /// the next segment's first. Past the last segment's cue the frames run on past the animation; past any other segment's
    /// cue the frame after is the one the import decoded (<see cref="SamusFramesBeyondAnimation.FrameAfterCue"/>).
    /// </summary>
    /// <exception cref="InvalidDataException">The installation holds no frame after a frame past a segment's cue.</exception>
    internal SamusAnimationFrame Next(SamusAnimationFrame frame, SamusFramesBeyondAnimation framesBeyondAnimation)
    {
        Ensure.NotNull(framesBeyondAnimation);
        int index = IndexOf(Require(frame).Segment);
        SamusAnimationSegment segment = segments[index];
        if (frame.Frame < segment.CueFrame || index == segments.Length - 1)
            return frame with { Frame = frame.Frame + 1 };
        return frame.Frame == segment.CueFrame && segment.End.ContinuesToNextSegment
            ? new SamusAnimationFrame(segments[index + 1].Name, 0)
            : framesBeyondAnimation.FrameAfterCue(Name, frame);
    }

    /// <summary>Orders two frames of this animation as it plays them: an earlier segment first, then an earlier frame.</summary>
    /// <exception cref="ArgumentException">Either frame names a segment the animation does not have.</exception>
    internal int Compare(SamusAnimationFrame left, SamusAnimationFrame right)
    {
        int bySegment = IndexOf(left.Segment).CompareTo(IndexOf(right.Segment));
        return bySegment != 0 ? bySegment : left.Frame.CompareTo(right.Frame);
    }

    /// <summary>The place of the segment named <paramref name="name"/> among the animation's segments, in play order.</summary>
    /// <exception cref="ArgumentException">The animation has no such segment.</exception>
    private int IndexOf(SamusAnimationSegmentName name)
    {
        int index = Array.FindIndex(segments, candidate => candidate.Name == name);
        return index >= 0 ? index : throw new ArgumentException($"Animation {Name} has no segment {name}.", nameof(name));
    }
}
