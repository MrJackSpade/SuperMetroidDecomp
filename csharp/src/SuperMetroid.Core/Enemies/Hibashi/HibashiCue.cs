namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>One cue of the Hibashi's animations: what it does, with the values it acts on.</summary>
internal abstract record HibashiCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private HibashiCue()
    {
    }

    /// <summary>Moves and sizes the collision part for one step of the rising eruption.</summary>
    /// <param name="Step">The eruption step the cue applies to the collision part.</param>
    internal sealed record EruptionStep(HibashiEruptionStep Step) : HibashiCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(HibashiCueKind Kind) : HibashiCue;
}
