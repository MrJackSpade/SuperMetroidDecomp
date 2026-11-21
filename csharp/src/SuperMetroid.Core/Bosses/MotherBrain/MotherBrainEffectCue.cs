namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One cue of Mother Brain's onion rings, bombs, breath, drool, door fragments and subtitle animations: what it does, with the values it acts on.</summary>
internal abstract partial record MotherBrainEffectCue
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
    internal sealed partial record SetRadius(Extent16<RoomPixels> Radius) : MotherBrainEffectCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MotherBrainEffectCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).SetRadius(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(MotherBrainEffectCueKind Kind) : MotherBrainEffectCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MotherBrainEffectCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }
}

/// <summary>What the owner of a <see cref="MotherBrainEffectCue"/> does on each of its cases (#2553).</summary>
/// <param name="SetRadius">Reacts to a <see cref="MotherBrainEffectCue.SetRadius"/>.</param>
/// <param name="Signal">Reacts to a <see cref="MotherBrainEffectCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record MotherBrainEffectCueReactions<TContext>(Action<MotherBrainEffectCue.SetRadius, TContext> SetRadius, Action<MotherBrainEffectCue.Signal, TContext> Signal);
