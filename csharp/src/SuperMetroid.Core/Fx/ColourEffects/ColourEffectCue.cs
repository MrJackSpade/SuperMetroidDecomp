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
    public sealed record QueueSound(SoundEffectId? Sound) : ColourEffectCue;

    /// <summary>Later frames show in these colours.</summary>
    /// <param name="Target">The colours a <see cref="SetColours"/> cue makes the effect's later frames show in.</param>
    public sealed record SetColours(ColourEffectColours Target) : ColourEffectCue;

    /// <summary>Publishes the phase of the heat pulse Samus's heat shimmer follows.</summary>
    /// <param name="Phase">The heat phase a <see cref="PublishHeatPhase"/> cue publishes, as the glow shows its look.</param>
    public sealed record PublishHeatPhase(HeatPhase Phase) : ColourEffectCue;

    /// <summary>The effect watches <see cref="Condition"/> from the next update on.</summary>
    /// <param name="Condition">The condition a <see cref="Watch"/> cue makes the effect watch.</param>
    public sealed record Watch(ColourEffectCondition Condition) : ColourEffectCue;
}
