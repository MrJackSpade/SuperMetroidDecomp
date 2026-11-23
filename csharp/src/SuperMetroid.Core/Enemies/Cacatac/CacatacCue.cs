namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>One cue of Cacatac's animations: what it does, with the values it acts on (#2432).</summary>
internal abstract partial record CacatacCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CacatacCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this cacatac cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(CacatacCueReactions<TContext> reactions, TContext context);


    /// <summary>Spits a spike.</summary>
    /// <param name="Direction">The launch direction the spike flies in.</param>
    internal sealed partial record SpawnSpike(CacatacSpikeDirection Direction) : CacatacCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CacatacCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SpawnSpike(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(CacatacCueKind Kind) : CacatacCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(CacatacCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }
}
