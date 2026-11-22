namespace SuperMetroid.Core.Animation;

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
