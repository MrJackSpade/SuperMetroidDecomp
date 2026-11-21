namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One cue of Crocomire's body animations: what it does, with the values it acts on.</summary>
internal abstract partial record CrocomireBodyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CrocomireBodyCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of this crocomire body cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the crocomire body cue is made into.</typeparam>
    /// <param name="cases">What each case is made into.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(CrocomireBodyCueCases<TContext, TResult> cases, TContext context);


    /// <summary>Takes the cue's step (#2163): its foot dust first where it raises any, then its move where its condition allows.</summary>
    /// <param name="Move">The step.</param>
    internal sealed partial record Step(CrocomireStep Move) : CrocomireBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(CrocomireBodyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Step(this, context);
    }

    /// <summary>Spawns a big dust cloud the cue's offset from the body.</summary>
    /// <param name="Offset">The cloud's horizontal offset in pixels from the body.</param>
    internal sealed partial record Dust(int Offset) : CrocomireBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(CrocomireBodyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Dust(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(CrocomireBodyCueKind Kind) : CrocomireBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(CrocomireBodyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Signal(this, context);
    }
}

/// <summary>What each case of a <see cref="CrocomireBodyCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the crocomire body cue is made into.</typeparam>
/// <param name="Step">Makes a <see cref="CrocomireBodyCue.Step"/> into the result.</param>
/// <param name="Dust">Makes a <see cref="CrocomireBodyCue.Dust"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="CrocomireBodyCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record CrocomireBodyCueCases<TContext, TResult>(Func<CrocomireBodyCue.Step, TContext, TResult> Step, Func<CrocomireBodyCue.Dust, TContext, TResult> Dust, Func<CrocomireBodyCue.Signal, TContext, TResult> Signal);
