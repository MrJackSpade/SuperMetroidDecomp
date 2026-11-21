namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// Holds the animation on this frame without reloading its timer: the timer stays expired, so the next tick wraps it
/// and moves on to the frame after. The aimed falling poses use it to spend one extra tick before their held frame.
/// </summary>
internal sealed partial record SamusAnimationHold : SamusAnimationCue
{
    /// <inheritdoc/>
    internal override TResult Match<TContext, TResult>(SamusAnimationCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Hold(this, context);

    /// <inheritdoc/>
    internal override bool ContinuesToNextSegment => true;
}
