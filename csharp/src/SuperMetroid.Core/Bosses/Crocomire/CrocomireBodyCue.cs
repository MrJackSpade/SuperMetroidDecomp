namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One cue of Crocomire's body animations: what it does, with the values it acts on.</summary>
internal abstract record CrocomireBodyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CrocomireBodyCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of this crocomire body cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the crocomire body cue is made into.</typeparam>
    /// <param name="cases">What each case is made into.</param>
    internal abstract TResult Match<TResult>(CrocomireBodyCueCases<TResult> cases);


    /// <summary>Takes the cue's step (#2163): its foot dust first where it raises any, then its move where its condition allows.</summary>
    /// <param name="Move">The step.</param>
    internal sealed record Step(CrocomireStep Move) : CrocomireBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(CrocomireBodyCueCases<TResult> cases) => Ensure.NotNull(cases).Step(this);
    }

    /// <summary>Spawns a big dust cloud the cue's offset from the body.</summary>
    /// <param name="Offset">The cloud's horizontal offset in pixels from the body.</param>
    internal sealed record Dust(int Offset) : CrocomireBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(CrocomireBodyCueCases<TResult> cases) => Ensure.NotNull(cases).Dust(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(CrocomireBodyCueKind Kind) : CrocomireBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(CrocomireBodyCueCases<TResult> cases) => Ensure.NotNull(cases).Signal(this);
    }
}

/// <summary>What each case of a <see cref="CrocomireBodyCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the crocomire body cue is made into.</typeparam>
/// <param name="Step">Makes a <see cref="CrocomireBodyCue.Step"/> into the result.</param>
/// <param name="Dust">Makes a <see cref="CrocomireBodyCue.Dust"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="CrocomireBodyCue.Signal"/> into the result.</param>
internal sealed record CrocomireBodyCueCases<TResult>(Func<CrocomireBodyCue.Step, TResult> Step, Func<CrocomireBodyCue.Dust, TResult> Dust, Func<CrocomireBodyCue.Signal, TResult> Signal);
