namespace SuperMetroid.Core.Animation;

/// <summary>What a play does on a hidden stretch and on any beat that is no pause (#2553).</summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
/// <typeparam name="TPlay">The play passing the beats.</typeparam>
/// <typeparam name="TResult">What a beat comes to.</typeparam>
/// <param name="Unpaused">Passes a beat that is no pause.</param>
/// <param name="Hidden">Passes a hidden stretch.</param>
internal sealed record UnpausedHidingAnimationBeatCases<TFrame, TEvent, TPlay, TResult>(
    PlayCase<AnimationBeat<TFrame, TEvent>.Unpaused, TPlay, TResult> Unpaused,
    PlayCase<AnimationHiddenBeat<TFrame, TEvent>, TPlay, TResult> Hidden)
    where TFrame : struct
    where TEvent : notnull;
