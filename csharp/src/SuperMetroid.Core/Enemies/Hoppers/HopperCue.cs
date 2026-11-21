namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>One cue of the Sidehoppers' and Dessgeegas' animations: what it does, with the values it acts on.</summary>
internal abstract partial record HopperCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private HopperCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this hopper cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(HopperCueReactions<TContext> reactions, TContext context);


    /// <summary>Queues an enemy-library sound.</summary>
    /// <param name="Sound">The sound the cue queues.</param>
    internal sealed partial record PlaySound(SoundEffectId Sound) : HopperCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(HopperCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).PlaySound(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(HopperCueKind Kind) : HopperCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(HopperCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }
}

/// <summary>What the owner of a <see cref="HopperCue"/> does on each of its cases (#2553).</summary>
/// <param name="PlaySound">Reacts to a <see cref="HopperCue.PlaySound"/>.</param>
/// <param name="Signal">Reacts to a <see cref="HopperCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record HopperCueReactions<TContext>(Action<HopperCue.PlaySound, TContext> PlaySound, Action<HopperCue.Signal, TContext> Signal);
