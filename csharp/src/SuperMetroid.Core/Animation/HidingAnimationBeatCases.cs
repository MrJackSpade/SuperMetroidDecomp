namespace SuperMetroid.Core.Animation;

/// <summary>What a play does on a hidden stretch and on any other beat it passes (#2553).</summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
/// <typeparam name="TPlay">The play passing the beats.</typeparam>
/// <typeparam name="TResult">What a beat comes to.</typeparam>
/// <param name="Beat">Passes any beat but a hidden stretch.</param>
/// <param name="Hidden">Passes a hidden stretch.</param>
internal sealed record HidingAnimationBeatCases<TFrame, TEvent, TPlay, TResult>(
    PlayCase<AnimationBeat<TFrame, TEvent>, TPlay, TResult> Beat,
    PlayCase<AnimationHiddenBeat<TFrame, TEvent>, TPlay, TResult> Hidden)
    where TFrame : struct
    where TEvent : notnull;
