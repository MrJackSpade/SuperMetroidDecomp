namespace SuperMetroid.Core.Animation;

/// <summary>
/// Beats built from what a run of them shares, so a shared duration or a run played back the other way is stated once
/// (#2557): a catalog names its frames and the duration once and its segments reference the run.
/// </summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal static partial class AnimationBeats<TFrame, TEvent>
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Each of <paramref name="frames"/> in turn, every one shown for <paramref name="duration"/> updates.</summary>
    /// <param name="duration">The updates each frame shows for.</param>
    /// <param name="frames">The frames, in the order they show.</param>
    internal static AnimationBeat<TFrame, TEvent>.Unpaused[] Steady(int duration, params ReadOnlySpan<TFrame> frames)
    {
        var beats = new AnimationBeat<TFrame, TEvent>.Unpaused[frames.Length];
        for (int index = 0; index < frames.Length; index++)
            beats[index] = new AnimationBeat<TFrame, TEvent>.Frame(frames[index], duration);
        return beats;
    }

    /// <summary>
    /// How many updates <paramref name="beats"/> play for, end to end (#2557): every frame's and pause's duration; an event or a
    /// marker takes none. A run another animation plays beside for exactly as long reads its length here.
    /// </summary>
    /// <param name="beats">The beats, none of them a frame held for good.</param>
    /// <exception cref="ArgumentException">A beat holds its frame for good, so the run never ends.</exception>
    internal static int UpdatesOf(IEnumerable<IHidingAnimationBeat<TFrame, TEvent>> beats) =>
        Ensure.NotNull(beats).Sum(beat => beat switch
        {
            AnimationBeat<TFrame, TEvent>.Frame frame => frame.Duration,
            AnimationBeat<TFrame, TEvent>.Pause pause => pause.Duration,
            AnimationHiddenBeat<TFrame, TEvent> hidden => hidden.Duration,
            AnimationBeat<TFrame, TEvent>.HeldFrame => throw new ArgumentException("A frame held for good never ends its run.", nameof(beats)),
            _ => 0,
        });

    /// <summary><paramref name="beats"/> played the other way round: the last beat first.</summary>
    /// <param name="beats">The beats, in the order they play forwards.</param>
    /// <typeparam name="TBeat">The kind of beat the run holds.</typeparam>
    internal static TBeat[] Backwards<TBeat>(IReadOnlyList<TBeat> beats)
        where TBeat : IHidingAnimationBeat<TFrame, TEvent> =>
        [.. Enumerable.Reverse(Ensure.NotNull(beats))];

    /// <summary>
    /// The looping cycle <paramref name="beats"/> turned the other way round from the same first beat: the first beat, then
    /// the rest from the last back to the second, as a wheel turning the other way shows its spokes.
    /// </summary>
    /// <param name="beats">The cycle's beats, in the order they play turning one way.</param>
    /// <typeparam name="TBeat">The kind of beat the cycle holds.</typeparam>
    internal static TBeat[] OtherWayRound<TBeat>(IReadOnlyList<TBeat> beats)
        where TBeat : IHidingAnimationBeat<TFrame, TEvent> =>
        [Ensure.NotNull(beats)[0], .. Enumerable.Reverse(beats.Skip(1))];
}
