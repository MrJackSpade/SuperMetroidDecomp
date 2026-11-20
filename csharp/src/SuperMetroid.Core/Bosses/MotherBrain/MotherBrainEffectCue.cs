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
    internal abstract void React(MotherBrainEffectCueReactions reactions);


    /// <summary>Sets how far the projectile collides across and down from its centre.</summary>
    /// <param name="Radius">The collision half-extents <see cref="SetRadius"/> installs.</param>
    internal sealed record SetRadius(Extent16<RoomPixels> Radius) : MotherBrainEffectCue
    {
        /// <inheritdoc/>
        internal override void React(MotherBrainEffectCueReactions reactions) => Ensure.NotNull(reactions).SetRadius(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(MotherBrainEffectCueKind Kind) : MotherBrainEffectCue
    {
        /// <inheritdoc/>
        internal override void React(MotherBrainEffectCueReactions reactions) => Ensure.NotNull(reactions).Signal(this);
    }
}

/// <summary>What the owner of a <see cref="MotherBrainEffectCue"/> does on each of its cases (#2553).</summary>
/// <param name="SetRadius">Reacts to a <see cref="MotherBrainEffectCue.SetRadius"/>.</param>
/// <param name="Signal">Reacts to a <see cref="MotherBrainEffectCue.Signal"/>.</param>
internal sealed record MotherBrainEffectCueReactions(Action<MotherBrainEffectCue.SetRadius> SetRadius, Action<MotherBrainEffectCue.Signal> Signal);
