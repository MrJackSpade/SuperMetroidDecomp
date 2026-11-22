namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One cue of the escaping Etecoons' animations: what it does, with the values it acts on.</summary>
internal abstract record EscapeEtecoonCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EscapeEtecoonCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of this escape etecoon cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the escape etecoon cue is made into.</typeparam>
    /// <param name="cases">What each case is made into.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(EscapeEtecoonCueCases<TContext, TResult> cases, TContext context);


    /// <summary>
    /// A low-tide run starts (#2167): once the rising acid is above its low surface (<see cref="EscapeAnimalMotionDefinitions.LowAcidSurfaceY"/>) the owner starts the high-tide run the same way
    /// instead.
    /// </summary>
    /// <param name="Running">The way the Etecoon runs.</param>
    internal sealed record LowTideRunStarts(HorizontalDirection Running) : EscapeEtecoonCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(EscapeEtecoonCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).LowTideRunStarts(this, context);
    }

    /// <summary>Moves the Etecoon horizontally.</summary>
    /// <param name="Offset">The cue's signed pixel offset.</param>
    internal sealed record Move(int Offset) : EscapeEtecoonCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(EscapeEtecoonCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Move(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(EscapeEtecoonCueKind Kind) : EscapeEtecoonCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(EscapeEtecoonCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Signal(this, context);
    }
}
