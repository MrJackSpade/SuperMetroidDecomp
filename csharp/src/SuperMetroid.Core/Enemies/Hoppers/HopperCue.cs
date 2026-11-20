namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>One cue of the Sidehoppers' and Dessgeegas' animations: what it does, with the values it acts on.</summary>
internal abstract record HopperCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private HopperCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this hopper cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(HopperCueReactions reactions);


    /// <summary>Queues an enemy-library sound.</summary>
    /// <param name="Sound">The sound the cue queues.</param>
    internal sealed record PlaySound(SoundEffectId Sound) : HopperCue
    {
        /// <inheritdoc/>
        internal override void React(HopperCueReactions reactions) => Ensure.NotNull(reactions).PlaySound(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(HopperCueKind Kind) : HopperCue
    {
        /// <inheritdoc/>
        internal override void React(HopperCueReactions reactions) => Ensure.NotNull(reactions).Signal(this);
    }
}

/// <summary>What the owner of a <see cref="HopperCue"/> does on each of its cases (#2553).</summary>
/// <param name="PlaySound">Reacts to a <see cref="HopperCue.PlaySound"/>.</param>
/// <param name="Signal">Reacts to a <see cref="HopperCue.Signal"/>.</param>
internal sealed record HopperCueReactions(Action<HopperCue.PlaySound> PlaySound, Action<HopperCue.Signal> Signal);
