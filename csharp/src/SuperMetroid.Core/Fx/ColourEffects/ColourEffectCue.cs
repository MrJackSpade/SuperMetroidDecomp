namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>One named step of a colour effect's animation that shows nothing, with the value it acts on.</summary>
public abstract record ColourEffectCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private ColourEffectCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this colour effect cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(ColourEffectCueReactions reactions);


    /// <summary>Queues a sound of the enemies' library.</summary>
    /// <param name="Sound">The enemies'-library sound a <see cref="QueueSound"/> cue queues.</param>
    public sealed record QueueSound(SoundEffectId Sound) : ColourEffectCue
    {
        /// <inheritdoc/>
        internal override void React(ColourEffectCueReactions reactions) => Ensure.NotNull(reactions).QueueSound(this);
    }

    /// <summary>Publishes the phase of the heat pulse Samus's heat shimmer follows.</summary>
    /// <param name="Phase">The heat phase a <see cref="PublishHeatPhase"/> cue publishes, as the glow shows its look.</param>
    public sealed record PublishHeatPhase(HeatPhase Phase) : ColourEffectCue
    {
        /// <inheritdoc/>
        internal override void React(ColourEffectCueReactions reactions) => Ensure.NotNull(reactions).PublishHeatPhase(this);
    }
}

/// <summary>What the owner of a <see cref="ColourEffectCue"/> does on each of its cases (#2553).</summary>
/// <param name="QueueSound">Reacts to a <see cref="ColourEffectCue.QueueSound"/>.</param>
/// <param name="PublishHeatPhase">Reacts to a <see cref="ColourEffectCue.PublishHeatPhase"/>.</param>
public sealed record ColourEffectCueReactions(Action<ColourEffectCue.QueueSound> QueueSound, Action<ColourEffectCue.PublishHeatPhase> PublishHeatPhase);
