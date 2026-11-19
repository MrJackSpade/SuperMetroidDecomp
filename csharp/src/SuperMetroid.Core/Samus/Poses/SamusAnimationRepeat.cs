namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Continues the animation from <paramref name="Target"/>, a frame of the same animation.</summary>
/// <param name="Target">The frame the animation continues from, a delay of the same animation.</param>
internal sealed record SamusAnimationRepeat(SamusAnimationFrame Target) : SamusAnimationCue
{
    /// <summary>Plays the segment named <paramref name="segment"/> again from its first frame (#2530).</summary>
    internal SamusAnimationRepeat(SamusAnimationSegmentName segment) : this(new SamusAnimationFrame(segment, 0))
    {
    }
}
