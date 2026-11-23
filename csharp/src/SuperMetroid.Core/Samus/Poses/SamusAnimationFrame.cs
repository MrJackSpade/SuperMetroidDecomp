namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// A frame of a Samus animation (#2048, #2395): a frame of one of the animation's named segments, which names it in any
/// animation that has that segment. Where Samus's animation stands, the frame a cue moves her animation to and the frames
/// her movement sets are all frames of this kind; the animation orders its own frames (<see cref="SamusPoseAnimation.FrameOrder"/>).
/// </summary>
/// <remarks>
/// A frame past a segment's end cue is no step of the animation: the game reads it, and a frame past the last segment's
/// end, as it reads any frame, and what each such frame reads is a named outcome the import decoded once
/// (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TFrame, TPlace, TStep}"/>).
/// </remarks>
/// <param name="Segment">The named segment.</param>
/// <param name="Frame">The frame within the segment, from zero.</param>
public readonly partial record struct SamusAnimationFrame(SamusAnimationSegmentName Segment, int Frame);
