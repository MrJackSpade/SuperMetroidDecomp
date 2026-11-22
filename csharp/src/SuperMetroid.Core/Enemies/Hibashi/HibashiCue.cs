namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>One cue of the Hibashi's animations: what it does, with the values it acts on.</summary>
internal abstract record HibashiCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private HibashiCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this hibashi cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(HibashiCueReactions<TContext> reactions, TContext context);


    /// <summary>Moves and sizes the collision part for one step of the rising eruption.</summary>
    /// <param name="Step">The eruption step the cue applies to the collision part.</param>
    internal sealed record EruptionStep(HibashiEruptionStep Step) : HibashiCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(HibashiCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).EruptionStep(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(HibashiCueKind Kind) : HibashiCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(HibashiCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }
}
