namespace SuperMetroid.Core.Animation;

/// <summary>A beat a family's segments hold (#2553): an <see cref="AnimationBeat{TFrame, TEvent}"/>, or, in a family whose animations hide, a hidden stretch.</summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal interface IAnimationBeat<TFrame, TEvent>
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>The frame the beat shows: a frame's or a held frame's; none for any other beat.</summary>
    TFrame? ShownFrame { get; }
}

/// <summary>A beat of a family whose animations may pause and hide (#2553): any <see cref="AnimationBeat{TFrame, TEvent}"/>, or a hidden stretch.</summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal interface IHidingAnimationBeat<TFrame, TEvent> : IAnimationBeat<TFrame, TEvent>
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>What <paramref name="play"/> does on this beat as it passes it: a hidden stretch, or any other beat.</summary>
    /// <typeparam name="TPlay">The play passing the beat.</typeparam>
    /// <typeparam name="TResult">What the beat comes to.</typeparam>
    /// <param name="cases">What the play does on a hidden stretch and on any other beat.</param>
    /// <param name="play">The play, which the beat may move.</param>
    TResult Match<TPlay, TResult>(HidingAnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play);
}

/// <summary>A beat of a family whose animations never pause but may hide (#2553): a beat that is no pause, or a hidden stretch.</summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal interface IUnpausedHidingAnimationBeat<TFrame, TEvent> : IAnimationBeat<TFrame, TEvent>
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>What <paramref name="play"/> does on this beat as it passes it: a hidden stretch, or any beat that is no pause.</summary>
    /// <typeparam name="TPlay">The play passing the beat.</typeparam>
    /// <typeparam name="TResult">What the beat comes to.</typeparam>
    /// <param name="cases">What the play does on a hidden stretch and on any beat that is no pause.</param>
    /// <param name="play">The play, which the beat may move.</param>
    TResult Match<TPlay, TResult>(UnpausedHidingAnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play);
}

/// <summary>What a play does on a hidden stretch and on any other beat it passes (#2553).</summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
/// <typeparam name="TPlay">The play passing the beats.</typeparam>
/// <typeparam name="TResult">What a beat comes to.</typeparam>
/// <param name="Beat">Passes any beat but a hidden stretch.</param>
/// <param name="Hidden">Passes a hidden stretch.</param>
internal sealed partial record HidingAnimationBeatCases<TFrame, TEvent, TPlay, TResult>(
    PlayCase<AnimationBeat<TFrame, TEvent>, TPlay, TResult> Beat,
    PlayCase<AnimationHiddenBeat<TFrame, TEvent>, TPlay, TResult> Hidden)
    where TFrame : struct
    where TEvent : notnull;

/// <summary>What a play does on a hidden stretch and on any beat that is no pause (#2553).</summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
/// <typeparam name="TPlay">The play passing the beats.</typeparam>
/// <typeparam name="TResult">What a beat comes to.</typeparam>
/// <param name="Unpaused">Passes a beat that is no pause.</param>
/// <param name="Hidden">Passes a hidden stretch.</param>
internal sealed partial record UnpausedHidingAnimationBeatCases<TFrame, TEvent, TPlay, TResult>(
    PlayCase<AnimationBeat<TFrame, TEvent>.Unpaused, TPlay, TResult> Unpaused,
    PlayCase<AnimationHiddenBeat<TFrame, TEvent>, TPlay, TResult> Hidden)
    where TFrame : struct
    where TEvent : notnull;
