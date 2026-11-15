namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One cue of the Chozo statue's animations: what it does, with the values it acts on.</summary>
internal abstract record ChozoStatueCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private ChozoStatueCue()
    {
    }

    /// <summary>Clears spikes under the cue's footstep offset.</summary>
    /// <param name="OffsetX">How far right of the statue a <see cref="Footstep"/> cue probes for spikes, in pixels.</param>
    internal sealed record Footstep(int OffsetX) : ChozoStatueCue;

    /// <summary>Moves the statue by the cue's carry step and carries Samus.</summary>
    /// <param name="Step">The facing and carry pose a <see cref="Move"/> cue moves by.</param>
    internal sealed record Move(ChozoCarryStep Step) : ChozoStatueCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(ChozoStatueCueKind Kind) : ChozoStatueCue;
}
