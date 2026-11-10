namespace SuperMetroid.Core.Programs;

/// <summary>One beat of an animation segment (#1600): a frame shown for a duration, or an event its owner reacts to.</summary>
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

    /// <summary>An event the owner reacts to as the animation reaches it, before the next frame shows.</summary>
    /// <param name="Raised">The event.</param>
    internal sealed record Event(TEvent Raised) : AnimationBeat<TFrame, TEvent>;
}
