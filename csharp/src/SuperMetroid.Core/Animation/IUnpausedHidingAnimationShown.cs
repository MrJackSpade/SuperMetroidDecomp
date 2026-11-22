namespace SuperMetroid.Core.Animation;

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
