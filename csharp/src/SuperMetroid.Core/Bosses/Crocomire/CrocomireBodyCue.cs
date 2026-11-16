namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One cue of Crocomire's body animations: what it does, with the values it acts on.</summary>
internal abstract record CrocomireBodyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CrocomireBodyCue()
    {
    }

    /// <summary>Takes the cue's step (#2163): its foot dust first where it raises any, then its move where its condition allows.</summary>
    /// <param name="Move">The step.</param>
    internal sealed record Step(CrocomireStep Move) : CrocomireBodyCue;

    /// <summary>Spawns a big dust cloud the cue's offset from the body.</summary>
    /// <param name="Offset">The cloud's horizontal offset in pixels from the body.</param>
    internal sealed record Dust(int Offset) : CrocomireBodyCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(CrocomireBodyCueKind Kind) : CrocomireBodyCue;
}
