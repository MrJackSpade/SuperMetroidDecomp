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
