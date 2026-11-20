namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>One cue of the Hibashi's animations: what it does, with the values it acts on.</summary>
internal abstract record HibashiCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private HibashiCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this hibashi cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(HibashiCueReactions reactions);


    /// <summary>Moves and sizes the collision part for one step of the rising eruption.</summary>
    /// <param name="Step">The eruption step the cue applies to the collision part.</param>
    internal sealed record EruptionStep(HibashiEruptionStep Step) : HibashiCue
    {
        /// <inheritdoc/>
        internal override void React(HibashiCueReactions reactions) => Ensure.NotNull(reactions).EruptionStep(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(HibashiCueKind Kind) : HibashiCue
    {
        /// <inheritdoc/>
        internal override void React(HibashiCueReactions reactions) => Ensure.NotNull(reactions).Signal(this);
    }
}

/// <summary>What the owner of a <see cref="HibashiCue"/> does on each of its cases (#2553).</summary>
/// <param name="EruptionStep">Reacts to a <see cref="HibashiCue.EruptionStep"/>.</param>
/// <param name="Signal">Reacts to a <see cref="HibashiCue.Signal"/>.</param>
internal sealed record HibashiCueReactions(Action<HibashiCue.EruptionStep> EruptionStep, Action<HibashiCue.Signal> Signal);
