namespace SuperMetroid.Core.Animation;

/// <summary>
/// One beat of an animation segment (#1600): a frame shown for a duration, a frame held for good (#1607), a pause, an
/// event its owner reacts to, or a marker (#2301), a moment of the animation its owner reacts to as it does to an event.
/// Every beat but a pause is an <see cref="Unpaused"/> one, what a family whose animations never pause holds (#2553).
/// </summary>
/// <typeparam name="TFrame">The frame identity.</typeparam>
/// <typeparam name="TEvent">The events the owner reacts to.</typeparam>
internal abstract record AnimationBeat<TFrame, TEvent> : IHidingAnimationBeat<TFrame, TEvent>
    where TFrame : struct
    where TEvent : notnull
{
    /// <summary>Allows only the beats below.</summary>
    private AnimationBeat()
    {
    }

    /// <inheritdoc/>
    TFrame? IAnimationBeat<TFrame, TEvent>.ShownFrame => ShownFrame;

    /// <inheritdoc/>
    TResult IHidingAnimationBeat<TFrame, TEvent>.Match<TPlay, TResult>(HidingAnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play) =>
        Ensure.NotNull(cases).Beat(this, ref play);

    /// <summary>What <paramref name="play"/> does on this beat as it passes it (#2553): a pause, or any other beat.</summary>
    /// <typeparam name="TPlay">The play passing the beat.</typeparam>
    /// <typeparam name="TResult">What the beat comes to.</typeparam>
    /// <param name="cases">What the play does on a pause and on any other beat.</param>
    /// <param name="play">The play, which the beat may move.</param>
    internal abstract TResult Match<TPlay, TResult>(AnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play);

    /// <summary>
    /// A beat that is no pause: a frame, a held frame, an event or a marker, what an animation that never pauses plays (#2553).
    /// </summary>
    internal abstract record Unpaused : AnimationBeat<TFrame, TEvent>, IUnpausedHidingAnimationBeat<TFrame, TEvent>
    {
        /// <summary>Allows only the beats below that are no pause.</summary>
        private protected Unpaused()
        {
        }

        /// <inheritdoc/>
        TResult IUnpausedHidingAnimationBeat<TFrame, TEvent>.Match<TPlay, TResult>(UnpausedHidingAnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play) =>
            Ensure.NotNull(cases).Unpaused(this, ref play);

        /// <inheritdoc/>
        internal sealed override TResult Match<TPlay, TResult>(AnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play) =>
            Ensure.NotNull(cases).Unpaused(this, ref play);

        /// <summary>What <paramref name="play"/> does on this beat as it passes it (#2553).</summary>
        /// <typeparam name="TPlay">The play passing the beat.</typeparam>
        /// <typeparam name="TResult">What the beat comes to.</typeparam>
        /// <param name="cases">What the play does on each beat that is no pause.</param>
        /// <param name="play">The play, which the beat may move.</param>
        internal abstract TResult Match<TPlay, TResult>(UnpausedAnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play);
    }

    /// <summary>A frame shown for <paramref name="Duration"/> updates.</summary>
    /// <param name="Shown">The frame.</param>
    /// <param name="Duration">The updates it shows for.</param>
    internal sealed record Frame(TFrame Shown, int Duration) : Unpaused
    {
        /// <inheritdoc/>
        internal override TResult Match<TPlay, TResult>(UnpausedAnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play) =>
            Ensure.NotNull(cases).Frame(this, ref play);
    }

    /// <summary>
    /// A frame shown from now on (#1607): the animation rests on it until its owner plays another, so it ends its segment.
    /// </summary>
    /// <param name="Shown">The frame.</param>
    internal sealed record HeldFrame(TFrame Shown) : Unpaused
    {
        /// <inheritdoc/>
        internal override TResult Match<TPlay, TResult>(UnpausedAnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play) =>
            Ensure.NotNull(cases).HeldFrame(this, ref play);
    }

    /// <summary>A pause of <paramref name="Duration"/> updates that shows no new frame: the last one stays.</summary>
    /// <param name="Duration">The updates it lasts.</param>
    internal sealed record Pause(int Duration) : AnimationBeat<TFrame, TEvent>
    {
        /// <inheritdoc/>
        internal override TResult Match<TPlay, TResult>(AnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play) =>
            Ensure.NotNull(cases).Pause(this, ref play);
    }

    /// <summary>An event the owner reacts to as the animation reaches it, before the next frame shows.</summary>
    /// <param name="Raised">The event.</param>
    internal sealed record Event(TEvent Raised) : Unpaused
    {
        /// <inheritdoc/>
        internal override TResult Match<TPlay, TResult>(UnpausedAnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play) =>
            Ensure.NotNull(cases).Event(this, ref play);
    }

    /// <summary>
    /// A moment of the animation its owner reacts to as it reaches it, before the next frame shows, as it does to an
    /// <see cref="Event"/> (#2301): Kraid's arm reaching its pause, a walk reaching its last frame. It marks where the
    /// animation stands for an owner that waits on that moment, and plays nothing of its own: it takes no time, and the
    /// frames shown and their timing are the beats' around it.
    /// </summary>
    /// <param name="Raised">The moment, raised as the owner's event.</param>
    internal sealed record Marker(TEvent Raised) : Unpaused
    {
        /// <inheritdoc/>
        internal override TResult Match<TPlay, TResult>(UnpausedAnimationBeatCases<TFrame, TEvent, TPlay, TResult> cases, ref TPlay play) =>
            Ensure.NotNull(cases).Marker(this, ref play);
    }

    /// <summary>The frame the beat shows: a frame's or a held frame's; none for a pause or an event.</summary>
    internal TFrame? ShownFrame => this switch
    {
        Frame frame => frame.Shown,
        HeldFrame held => held.Shown,
        _ => null,
    };
}
