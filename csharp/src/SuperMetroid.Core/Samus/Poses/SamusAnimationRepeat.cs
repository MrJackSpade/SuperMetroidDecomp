namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Continues the animation from <paramref name="Target"/>, a frame of the same animation.</summary>
/// <param name="Target">The frame the animation continues from, a delay of the same animation.</param>
internal sealed partial record SamusAnimationRepeat(SamusAnimationFrame Target) : SamusAnimationCue
{
    /// <inheritdoc/>
    internal override TResult Match<TContext, TResult>(SamusAnimationCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Repeat(this, context);

    /// <summary>Plays the segment named <paramref name="segment"/> again from its first frame (#2530).</summary>
    internal SamusAnimationRepeat(SamusAnimationSegmentName segment) : this(new SamusAnimationFrame(segment, 0))
    {
    }
}
