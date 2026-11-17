namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>One named step of a colour effect's animation that shows nothing, with the value it acts on.</summary>
public abstract record ColourEffectCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private ColourEffectCue()
    {
    }

    /// <summary>Queues a sound of the enemies' library.</summary>
    /// <param name="Sound">The enemies'-library sound a <see cref="QueueSound"/> cue queues.</param>
    public sealed record QueueSound(SoundEffectId Sound) : ColourEffectCue;

    /// <summary>Publishes the phase of the heat pulse Samus's heat shimmer follows.</summary>
    /// <param name="Phase">The heat phase a <see cref="PublishHeatPhase"/> cue publishes, as the glow shows its look.</param>
    public sealed record PublishHeatPhase(HeatPhase Phase) : ColourEffectCue;
}
