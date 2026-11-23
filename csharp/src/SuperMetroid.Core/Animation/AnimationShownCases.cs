namespace SuperMetroid.Core.Animation;

/// <summary>What an animation's owner makes of each outcome of a step (#2553).</summary>
/// <typeparam name="TSegment">The segments the animation plays.</typeparam>
/// <typeparam name="TFrame">The frames it shows.</typeparam>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
/// <typeparam name="TResult">What an outcome comes to.</typeparam>
/// <param name="Frame">A new frame shown for its updates.</param>
/// <param name="HeldFrame">A frame shown for good.</param>
/// <param name="Paused">A wait of its updates with no new frame.</param>
/// <param name="Held">A rest on the frame shown last.</param>
/// <param name="Ended">The animation's end.</param>
internal sealed partial record AnimationShownCases<TSegment, TFrame, TContext, TResult>(
    Func<AnimationShown<TSegment, TFrame>.Frame, TContext, TResult> Frame,
    Func<AnimationShown<TSegment, TFrame>.HeldFrame, TContext, TResult> HeldFrame,
    Func<AnimationShown<TSegment, TFrame>.Paused, TContext, TResult> Paused,
    Func<AnimationShown<TSegment, TFrame>.Held, TContext, TResult> Held,
    Func<AnimationShown<TSegment, TFrame>.Ended, TContext, TResult> Ended)
    where TSegment : struct
    where TFrame : struct;
