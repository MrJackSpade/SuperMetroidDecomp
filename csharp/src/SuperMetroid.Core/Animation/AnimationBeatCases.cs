namespace SuperMetroid.Core.Animation;

/// <summary>What a play does on a pause and on any other beat it passes (#2553).</summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
/// <typeparam name="TPlay">The play passing the beats.</typeparam>
/// <typeparam name="TResult">What a beat comes to.</typeparam>
/// <param name="Unpaused">Passes a beat that is no pause.</param>
/// <param name="Pause">Passes a pause.</param>
internal sealed partial record AnimationBeatCases<TFrame, TEvent, TPlay, TResult>(
    PlayCase<AnimationBeat<TFrame, TEvent>.Unpaused, TPlay, TResult> Unpaused,
    PlayCase<AnimationBeat<TFrame, TEvent>.Pause, TPlay, TResult> Pause)
    where TFrame : struct
    where TEvent : notnull;
