namespace SuperMetroid.Core.Animation;

/// <summary>What a play comes to at each end of its animation (#2553).</summary>
/// <typeparam name="TPlay">The play reaching the end.</typeparam>
/// <typeparam name="TResult">What an end comes to.</typeparam>
/// <param name="Stop">Reaches a stop.</param>
/// <param name="Loop">Reaches a loop.</param>
/// <param name="Hold">Reaches a hold.</param>
internal sealed record AnimationEndCases<TPlay, TResult>(
    PlayCase<AnimationEnd.Stop, TPlay, TResult> Stop,
    PlayCase<AnimationEnd.Loop, TPlay, TResult> Loop,
    PlayCase<AnimationEnd.Hold, TPlay, TResult> Hold);
