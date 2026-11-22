namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One cue of Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle animations: what it does, with the values it acts on.</summary>
internal abstract record MotherBrainEffectCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MotherBrainEffectCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this mother brain effect cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(MotherBrainEffectCueReactions<TContext> reactions, TContext context);


    /// <summary>Sets how far the projectile collides across and down from its centre.</summary>
    /// <param name="Radius">The collision half-extents <see cref="SetRadius"/> installs.</param>
    internal sealed record SetRadius(Extent16<RoomPixels> Radius) : MotherBrainEffectCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MotherBrainEffectCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SetRadius(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(MotherBrainEffectCueKind Kind) : MotherBrainEffectCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MotherBrainEffectCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }
}
