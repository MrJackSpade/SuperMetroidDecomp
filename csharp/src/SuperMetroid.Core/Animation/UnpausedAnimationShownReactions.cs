namespace SuperMetroid.Core.Animation;

/// <summary>
/// What the owner of an animation that never pauses does on each outcome of a step (#2553), each outcome carrying out its own.
/// </summary>
/// <typeparam name="TSegment">The segments the animation plays.</typeparam>
/// <typeparam name="TFrame">The frames it shows.</typeparam>
/// <param name="Frame">Shows a new frame for its updates.</param>
/// <param name="HeldFrame">Shows a frame for good.</param>
/// <param name="Held">Rests on the frame shown last.</param>
/// <param name="Ended">Ends the animation.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record UnpausedAnimationShownReactions<TSegment, TFrame, TContext>(
    Action<AnimationShown<TSegment, TFrame>.Frame, TContext> Frame,
    Action<AnimationShown<TSegment, TFrame>.HeldFrame, TContext> HeldFrame,
    Action<AnimationShown<TSegment, TFrame>.Held, TContext> Held,
    Action<AnimationShown<TSegment, TFrame>.Ended, TContext> Ended)
    where TSegment : struct
    where TFrame : struct;
