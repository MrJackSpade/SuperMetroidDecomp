namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>One gameplay cue of Botwoon's head animations: what it does, with the values it acts on.</summary>
internal abstract partial record BotwoonHeadCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private BotwoonHeadCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this botwoon head cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(BotwoonHeadCueReactions<TContext> reactions, TContext context);


    /// <summary>Sets the head's collision radius for the cue's facing (#2175): the facing of the animation that raises it.</summary>
    /// <param name="Facing">The way the head faces.</param>
    internal sealed partial record ApplyFacingRadius(CompassDirection Facing) : BotwoonHeadCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(BotwoonHeadCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ApplyFacingRadius(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(BotwoonHeadCueKind Kind) : BotwoonHeadCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(BotwoonHeadCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }
}
