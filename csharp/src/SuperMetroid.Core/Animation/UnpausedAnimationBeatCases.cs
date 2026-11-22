namespace SuperMetroid.Core.Animation;

/// <summary>What a play does on each beat that is no pause (#2553).</summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
/// <typeparam name="TPlay">The play passing the beats.</typeparam>
/// <typeparam name="TResult">What a beat comes to.</typeparam>
/// <param name="Frame">Passes a frame.</param>
/// <param name="HeldFrame">Passes a frame held for good.</param>
/// <param name="Event">Passes an event.</param>
/// <param name="Marker">Passes a marker.</param>
internal sealed partial record UnpausedAnimationBeatCases<TFrame, TEvent, TPlay, TResult>(
    PlayCase<AnimationBeat<TFrame, TEvent>.Frame, TPlay, TResult> Frame,
    PlayCase<AnimationBeat<TFrame, TEvent>.HeldFrame, TPlay, TResult> HeldFrame,
    PlayCase<AnimationBeat<TFrame, TEvent>.Event, TPlay, TResult> Event,
    PlayCase<AnimationBeat<TFrame, TEvent>.Marker, TPlay, TResult> Marker)
    where TFrame : struct
    where TEvent : notnull;
