namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>One named step of a colour effect's animation that shows nothing, with the value it acts on.</summary>
public abstract partial record ColourEffectCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private ColourEffectCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this colour effect cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(ColourEffectCueReactions<TContext> reactions, TContext context);


    /// <summary>Queues a sound of the enemies' library.</summary>
    /// <param name="Sound">The enemies'-library sound a <see cref="QueueSound"/> cue queues.</param>
    public sealed partial record QueueSound(SoundEffectId Sound) : ColourEffectCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(ColourEffectCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).QueueSound(this, context);
    }

    /// <summary>Publishes the phase of the heat pulse Samus's heat shimmer follows.</summary>
    /// <param name="Phase">The heat phase a <see cref="PublishHeatPhase"/> cue publishes, as the glow shows its look.</param>
    public sealed partial record PublishHeatPhase(HeatPhase Phase) : ColourEffectCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(ColourEffectCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).PublishHeatPhase(this, context);
    }
}

/// <summary>What the owner of a <see cref="ColourEffectCue"/> does on each of its cases (#2553).</summary>
/// <param name="QueueSound">Reacts to a <see cref="ColourEffectCue.QueueSound"/>.</param>
/// <param name="PublishHeatPhase">Reacts to a <see cref="ColourEffectCue.PublishHeatPhase"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
public sealed record ColourEffectCueReactions<TContext>(Action<ColourEffectCue.QueueSound, TContext> QueueSound, Action<ColourEffectCue.PublishHeatPhase, TContext> PublishHeatPhase);
