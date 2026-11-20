namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One cue of the Yapping Maw's animations: what it does, with the values it acts on.</summary>
internal abstract record YappingMawCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private YappingMawCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this yapping maw cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    internal abstract void React(YappingMawCueReactions reactions);


    /// <summary>Holds a grabbed Samus at the mouth offset of the cue's aim (#2071).</summary>
    /// <param name="Aim">The aim whose mouth offset Samus is held at.</param>
    internal sealed record HoldSamus(YappingMawAim Aim) : YappingMawCue
    {
        /// <inheritdoc/>
        internal override void React(YappingMawCueReactions reactions) => Ensure.NotNull(reactions).HoldSamus(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(YappingMawCueKind Kind) : YappingMawCue
    {
        /// <inheritdoc/>
        internal override void React(YappingMawCueReactions reactions) => Ensure.NotNull(reactions).Signal(this);
    }
}

/// <summary>What the owner of a <see cref="YappingMawCue"/> does on each of its cases (#2553).</summary>
/// <param name="HoldSamus">Reacts to a <see cref="YappingMawCue.HoldSamus"/>.</param>
/// <param name="Signal">Reacts to a <see cref="YappingMawCue.Signal"/>.</param>
internal sealed record YappingMawCueReactions(Action<YappingMawCue.HoldSamus> HoldSamus, Action<YappingMawCue.Signal> Signal);
