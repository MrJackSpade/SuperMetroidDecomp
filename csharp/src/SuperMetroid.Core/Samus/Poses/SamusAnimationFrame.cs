namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Where Samus's animation stands (#2048): a frame of one segment of the animation she animates by, the segment by its place
/// among the animation's named segments (<see cref="SamusPoseAnimation.Segments"/>). The default is the animation's first
/// frame, whatever the animation. Frames of one animation are ordered as it plays them: by segment, then by frame.
/// </summary>
/// <remarks>
/// A frame past the segment's end cue is one of the values the cue carries; a frame past the last segment's end lies past
/// the animation itself. The game reads both as it reads any frame, and what each such frame reads is a named outcome the
/// import decoded once (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
/// </remarks>
/// <param name="Segment">The segment's place among the animation's segments, from zero.</param>
/// <param name="Frame">The frame within the segment, from zero.</param>
public readonly record struct SamusAnimationFrame(int Segment, int Frame) : IComparable<SamusAnimationFrame>
{
    /// <summary>The first frame of an animation.</summary>
    public static SamusAnimationFrame Start => default;

    /// <summary>
    /// The frame after an animation's first: a spin starts on it, skipping its still first frame, and a standing pose aiming
    /// up entered from one keeps the gun raised on it.
    /// </summary>
    public static SamusAnimationFrame AfterStart => new(0, 1);

    /// <summary>Orders two frames of one animation as it plays them: an earlier segment first, then an earlier frame.</summary>
    public int CompareTo(SamusAnimationFrame other) =>
        Segment != other.Segment ? Segment.CompareTo(other.Segment) : Frame.CompareTo(other.Frame);

    /// <summary>Whether <paramref name="left"/> plays before <paramref name="right"/>.</summary>
    public static bool operator <(SamusAnimationFrame left, SamusAnimationFrame right) => left.CompareTo(right) < 0;

    /// <summary>Whether <paramref name="left"/> plays after <paramref name="right"/>.</summary>
    public static bool operator >(SamusAnimationFrame left, SamusAnimationFrame right) => left.CompareTo(right) > 0;

    /// <summary>Whether <paramref name="left"/> plays before <paramref name="right"/> or is it.</summary>
    public static bool operator <=(SamusAnimationFrame left, SamusAnimationFrame right) => left.CompareTo(right) <= 0;

    /// <summary>Whether <paramref name="left"/> plays after <paramref name="right"/> or is it.</summary>
    public static bool operator >=(SamusAnimationFrame left, SamusAnimationFrame right) => left.CompareTo(right) >= 0;
}
