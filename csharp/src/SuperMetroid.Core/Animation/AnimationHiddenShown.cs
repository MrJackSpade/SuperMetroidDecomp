namespace SuperMetroid.Core.Animation;

/// <summary>
/// The play reached a beat of <paramref name="Segment"/> that shows nothing for <paramref name="Duration"/> updates (#2558):
/// an outcome only a family whose animations hide comes to (#2553).
/// </summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <param name="Segment">The segment whose beat hides the owner.</param>
/// <param name="Duration">The updates the owner stays hidden.</param>
internal sealed partial record AnimationHiddenShown<TSegment, TFrame>(TSegment Segment, int Duration)
    : IHidingAnimationShown<TSegment, TFrame>, IUnpausedHidingAnimationShown<TSegment, TFrame>
    where TSegment : struct
    where TFrame : struct
{
    /// <inheritdoc/>
    void IHidingAnimationShown<TSegment, TFrame>.React<TContext>(HidingAnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context) =>
        Ensure.NotNull(reactions).Hidden(this, context);

    /// <inheritdoc/>
    TResult IUnpausedHidingAnimationShown<TSegment, TFrame>.Match<TContext, TResult>(UnpausedHidingAnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context) =>
        Ensure.NotNull(cases).Hidden(this, context);
}
