namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One cue of the Ki-Hunter acid's animations (#1843): what it does, with the values it acts on.</summary>
internal abstract record KiHunterAcidSpitCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private KiHunterAcidSpitCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this ki hunter acid spit cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(KiHunterAcidSpitCueReactions reactions);


    /// <summary>Launches the acid out of the mouth the cue's way (#2163).</summary>
    /// <param name="Direction">The way the acid leaves.</param>
    internal sealed record Launch(HorizontalDirection Direction) : KiHunterAcidSpitCue
    {
        /// <inheritdoc/>
        internal override void React(KiHunterAcidSpitCueReactions reactions) => Ensure.NotNull(reactions).Launch(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(KiHunterAcidSpitCueKind Kind) : KiHunterAcidSpitCue
    {
        /// <inheritdoc/>
        internal override void React(KiHunterAcidSpitCueReactions reactions) => Ensure.NotNull(reactions).Signal(this);
    }
}

/// <summary>What the owner of a <see cref="KiHunterAcidSpitCue"/> does on each of its cases (#2553).</summary>
/// <param name="Launch">Reacts to a <see cref="KiHunterAcidSpitCue.Launch"/>.</param>
/// <param name="Signal">Reacts to a <see cref="KiHunterAcidSpitCue.Signal"/>.</param>
internal sealed record KiHunterAcidSpitCueReactions(Action<KiHunterAcidSpitCue.Launch> Launch, Action<KiHunterAcidSpitCue.Signal> Signal);
