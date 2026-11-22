namespace SuperMetroid.Core.Animation;

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
