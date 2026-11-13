namespace SuperMetroid.Core.Animation;

/// <summary>
/// One beat of an animation segment (#1600): a frame shown for a duration, a frame held for good (#1607), a pause, or an
/// event its owner reacts to.
/// </summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal abstract record AnimationBeat<TFrame, TEvent>
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Allows only the beats below.</summary>
    private AnimationBeat()
    {
    }

    /// <summary>A frame shown for <paramref name="Duration"/> updates.</summary>
    /// <param name="Shown">The frame.</param>
    /// <param name="Duration">The updates it shows for.</param>
    internal sealed record Frame(TFrame Shown, int Duration) : AnimationBeat<TFrame, TEvent>;

    /// <summary>
    /// A frame shown from now on (#1607): the animation rests on it until its owner plays another, so it ends its segment.
    /// </summary>
    /// <param name="Shown">The frame.</param>
    internal sealed record HeldFrame(TFrame Shown) : AnimationBeat<TFrame, TEvent>;

    /// <summary>A pause of <paramref name="Duration"/> updates that shows no new frame: the last one stays.</summary>
    /// <param name="Duration">The updates it lasts.</param>
    internal sealed record Pause(int Duration) : AnimationBeat<TFrame, TEvent>;

    /// <summary>An event the owner reacts to as the animation reaches it, before the next frame shows.</summary>
    /// <param name="Raised">The event.</param>
    internal sealed record Event(TEvent Raised) : AnimationBeat<TFrame, TEvent>;

    /// <summary>The frame the beat shows: a frame's or a held frame's; none for a pause or an event.</summary>
    internal TFrame? ShownFrame => this switch
    {
        Frame frame => frame.Shown,
        HeldFrame held => held.Shown,
        _ => null,
    };
}
