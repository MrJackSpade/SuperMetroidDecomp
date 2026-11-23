namespace SuperMetroid.Core.Animation;

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
