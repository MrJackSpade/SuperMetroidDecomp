namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Where Samus's animation stands (#2048): a frame of one segment of the animation she animates by, the segment by its place
/// among the animation's named segments (<see cref="SamusPoseAnimation.Segments"/>). The default is the animation's first
/// frame, whatever the animation.
/// </summary>
/// <remarks>
/// A frame past the segment's end cue is one of the values the cue carries; a frame past the last segment's end lies past
/// the animation itself. The game reads both as it reads any frame (<see cref="Compatibility.FramesBeyondAnimation{TAnimation, TStep}"/>);
/// only such reads, and frames written for one animation onto another, follow the game's layout of the animations.
/// </remarks>
/// <param name="Segment">The segment's place among the animation's segments, from zero.</param>
/// <param name="Frame">The frame within the segment, from zero.</param>
public readonly record struct SamusAnimationFrame(int Segment, int Frame)
{
    /// <summary>The first frame of an animation.</summary>
    public static SamusAnimationFrame Start => default;

    /// <summary>
    /// The frame after an animation's first: a spin starts on it, skipping its still first frame, and a standing pose aiming
    /// up entered from one keeps the gun raised on it.
    /// </summary>
    public static SamusAnimationFrame AfterStart => new(0, 1);
}
