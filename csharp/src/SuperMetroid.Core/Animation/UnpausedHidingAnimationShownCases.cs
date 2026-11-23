namespace SuperMetroid.Core.Animation;

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
