namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>One cue of Cacatac's animations: what it does, with the values it acts on (#2432).</summary>
internal abstract record CacatacCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private CacatacCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this cacatac cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(CacatacCueReactions reactions);


    /// <summary>Spits a spike.</summary>
    /// <param name="Direction">The launch direction the spike flies in.</param>
    internal sealed record SpawnSpike(CacatacSpikeDirection Direction) : CacatacCue
    {
        /// <inheritdoc/>
        internal override void React(CacatacCueReactions reactions) => Ensure.NotNull(reactions).SpawnSpike(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(CacatacCueKind Kind) : CacatacCue
    {
        /// <inheritdoc/>
        internal override void React(CacatacCueReactions reactions) => Ensure.NotNull(reactions).Signal(this);
    }
}

/// <summary>What the owner of a <see cref="CacatacCue"/> does on each of its cases (#2553).</summary>
/// <param name="SpawnSpike">Reacts to a <see cref="CacatacCue.SpawnSpike"/>.</param>
/// <param name="Signal">Reacts to a <see cref="CacatacCue.Signal"/>.</param>
internal sealed record CacatacCueReactions(Action<CacatacCue.SpawnSpike> SpawnSpike, Action<CacatacCue.Signal> Signal);
