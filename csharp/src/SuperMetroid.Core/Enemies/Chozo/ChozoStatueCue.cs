namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One cue of the Chozo statue's animations: what it does, with the values it acts on.</summary>
internal abstract record ChozoStatueCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private ChozoStatueCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this chozo statue cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(ChozoStatueCueReactions reactions);


    /// <summary>Clears spikes under the cue's footstep offset.</summary>
    /// <param name="OffsetX">How far right of the statue a <see cref="Footstep"/> cue probes for spikes, in pixels.</param>
    internal sealed record Footstep(int OffsetX) : ChozoStatueCue
    {
        /// <inheritdoc/>
        internal override void React(ChozoStatueCueReactions reactions) => Ensure.NotNull(reactions).Footstep(this);
    }

    /// <summary>Moves the statue by the cue's carry step and carries Samus.</summary>
    /// <param name="Step">The facing and carry pose a <see cref="Move"/> cue moves by.</param>
    internal sealed record Move(ChozoCarryStep Step) : ChozoStatueCue
    {
        /// <inheritdoc/>
        internal override void React(ChozoStatueCueReactions reactions) => Ensure.NotNull(reactions).Move(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(ChozoStatueCueKind Kind) : ChozoStatueCue
    {
        /// <inheritdoc/>
        internal override void React(ChozoStatueCueReactions reactions) => Ensure.NotNull(reactions).Signal(this);
    }
}

/// <summary>What the owner of a <see cref="ChozoStatueCue"/> does on each of its cases (#2553).</summary>
/// <param name="Footstep">Reacts to a <see cref="ChozoStatueCue.Footstep"/>.</param>
/// <param name="Move">Reacts to a <see cref="ChozoStatueCue.Move"/>.</param>
/// <param name="Signal">Reacts to a <see cref="ChozoStatueCue.Signal"/>.</param>
internal sealed record ChozoStatueCueReactions(Action<ChozoStatueCue.Footstep> Footstep, Action<ChozoStatueCue.Move> Move, Action<ChozoStatueCue.Signal> Signal);
