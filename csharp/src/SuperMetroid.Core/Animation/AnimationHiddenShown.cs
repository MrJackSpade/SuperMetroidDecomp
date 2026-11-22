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

/// <summary>What a play of a family whose animations may pause and hide comes to (#2553): any outcome, or a hidden stretch.</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
internal interface IHidingAnimationShown<TSegment, TFrame>
    where TSegment : struct
    where TFrame : struct
{
    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this outcome.</summary>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="reactions">What its owner does on a hidden stretch and on any other outcome.</param>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    void React<TContext>(HidingAnimationShownReactions<TSegment, TFrame, TContext> reactions, TContext context);
}

/// <summary>What a play of a family whose animations never pause but may hide comes to (#2553): an outcome that is no pause, or a hidden stretch.</summary>
/// <typeparam name="TSegment">The segment identity.</typeparam>
/// <typeparam name="TFrame">The frame identity.</typeparam>
internal interface IUnpausedHidingAnimationShown<TSegment, TFrame>
    where TSegment : struct
    where TFrame : struct
{
    /// <summary>What <paramref name="cases"/> make of this outcome.</summary>
    /// <typeparam name="TContext">What the case's answer reads besides the case.</typeparam>
    /// <typeparam name="TResult">What an outcome comes to.</typeparam>
    /// <param name="cases">What its owner makes of a hidden stretch and of any outcome that is no pause.</param>
    /// <param name="context">What the case's answer reads besides the case, handed to it so the cases are built once.</param>
    TResult Match<TContext, TResult>(UnpausedHidingAnimationShownCases<TSegment, TFrame, TContext, TResult> cases, TContext context);
}

/// <summary>What the owner of a family whose animations may pause and hide does on each outcome of a step (#2553).</summary>
/// <typeparam name="TSegment">The segments the animation plays.</typeparam>
/// <typeparam name="TFrame">The frames it shows.</typeparam>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
/// <param name="Shown">Reacts to any outcome but a hidden stretch.</param>
/// <param name="Hidden">Shows nothing for the stretch's updates.</param>
internal sealed partial record HidingAnimationShownReactions<TSegment, TFrame, TContext>(
    Action<AnimationShown<TSegment, TFrame>, TContext> Shown,
    Action<AnimationHiddenShown<TSegment, TFrame>, TContext> Hidden)
    where TSegment : struct
    where TFrame : struct;

/// <summary>What the owner of a family whose animations never pause but may hide makes of each outcome of a step (#2553).</summary>
/// <typeparam name="TSegment">The segments the animation plays.</typeparam>
/// <typeparam name="TFrame">The frames it shows.</typeparam>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
/// <typeparam name="TResult">What an outcome comes to.</typeparam>
/// <param name="Shown">Any outcome that is no pause and no hidden stretch.</param>
/// <param name="Hidden">A stretch of updates showing nothing.</param>
internal sealed partial record UnpausedHidingAnimationShownCases<TSegment, TFrame, TContext, TResult>(
    Func<AnimationShown<TSegment, TFrame>.Unpaused, TContext, TResult> Shown,
    Func<AnimationHiddenShown<TSegment, TFrame>, TContext, TResult> Hidden)
    where TSegment : struct
    where TFrame : struct;
