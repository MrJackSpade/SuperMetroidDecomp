namespace SuperMetroid.Core.Animation;

/// <summary>How an animation ends once its last phase has played (#1600).</summary>
internal abstract record AnimationEnd
{
    /// <summary>Allows only the endings below.</summary>
    private AnimationEnd()
    {
    }

    /// <summary>What <paramref name="play"/> comes to at this end of its animation (#2553).</summary>
    /// <typeparam name="TPlay">The play reaching the end.</typeparam>
    /// <typeparam name="TResult">What the end comes to.</typeparam>
    /// <param name="cases">What the play does at each end.</param>
    /// <param name="play">The play, which the end may move.</param>
    internal abstract TResult Match<TPlay, TResult>(AnimationEndCases<TPlay, TResult> cases, ref TPlay play);

    /// <summary>The animation is over.</summary>
    internal static AnimationEnd Stops { get; } = new Stop();

    /// <summary>The animation rests on the frame it showed last until its owner plays another.</summary>
    internal static AnimationEnd Holds { get; } = new Hold();

    /// <summary>The animation is over.</summary>
    internal sealed record Stop : AnimationEnd
    {
        /// <inheritdoc/>
        internal override TResult Match<TPlay, TResult>(AnimationEndCases<TPlay, TResult> cases, ref TPlay play) =>
            Ensure.NotNull(cases).Stop(this, ref play);
    }

    /// <summary>
    /// The animation plays on from <see cref="Phase"/>: the first phase of its cycle, which
    /// <see cref="Animation{TSegment}.LeadingInto"/> places after the lead, so no definition names it by its place (#2414).
    /// </summary>
    internal sealed record Loop : AnimationEnd
    {
        /// <summary>Plays on from <paramref name="phase"/>.</summary>
        /// <param name="phase">The phase it plays on from.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is negative.</exception>
        internal Loop(int phase) => Phase = Ensure.AtLeastZero(phase);

        /// <summary>The phase it plays on from.</summary>
        internal int Phase { get; }

        /// <inheritdoc/>
        internal override TResult Match<TPlay, TResult>(AnimationEndCases<TPlay, TResult> cases, ref TPlay play) =>
            Ensure.NotNull(cases).Loop(this, ref play);
    }

    /// <summary>The animation rests on its last frame until its owner plays another.</summary>
    internal sealed record Hold : AnimationEnd
    {
        /// <inheritdoc/>
        internal override TResult Match<TPlay, TResult>(AnimationEndCases<TPlay, TResult> cases, ref TPlay play) =>
            Ensure.NotNull(cases).Hold(this, ref play);
    }
}

/// <summary>What a play comes to at each end of its animation (#2553).</summary>
/// <typeparam name="TPlay">The play reaching the end.</typeparam>
/// <typeparam name="TResult">What an end comes to.</typeparam>
/// <param name="Stop">Reaches a stop.</param>
/// <param name="Loop">Reaches a loop.</param>
/// <param name="Hold">Reaches a hold.</param>
internal sealed record AnimationEndCases<TPlay, TResult>(
    PlayCase<AnimationEnd.Stop, TPlay, TResult> Stop,
    PlayCase<AnimationEnd.Loop, TPlay, TResult> Loop,
    PlayCase<AnimationEnd.Hold, TPlay, TResult> Hold);
