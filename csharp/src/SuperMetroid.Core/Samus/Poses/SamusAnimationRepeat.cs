namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Continues the animation from <paramref name="Target"/>, a frame of the same animation.</summary>
/// <param name="Target">The frame the animation continues from, a delay of the same animation.</param>
internal sealed record SamusAnimationRepeat(SamusAnimationFrame Target) : SamusAnimationCue
{
    /// <summary>Continues the animation from frame <paramref name="frame"/> of its segment <paramref name="segment"/>.</summary>
    internal SamusAnimationRepeat(SamusAnimationSegmentName segment, int frame) : this(new SamusAnimationFrame(segment, frame))
    {
    }
}
