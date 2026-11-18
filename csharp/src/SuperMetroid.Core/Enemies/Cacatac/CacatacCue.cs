namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>One cue of Cacatac's animations: what it does, with the values it acts on (#2432).</summary>
internal abstract record CacatacCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CacatacCue()
    {
    }

    /// <summary>Spits a spike.</summary>
    /// <param name="Direction">The launch direction the spike flies in.</param>
    internal sealed record SpawnSpike(CacatacSpikeDirection Direction) : CacatacCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(CacatacCueKind Kind) : CacatacCue;
}
