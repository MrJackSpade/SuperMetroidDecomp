namespace SuperMetroid.Core.Animation;

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
