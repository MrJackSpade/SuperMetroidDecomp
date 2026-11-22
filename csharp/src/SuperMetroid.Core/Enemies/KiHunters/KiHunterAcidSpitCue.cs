namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>One cue of the Ki-Hunter acid's animations (#1843): what it does, with the values it acts on.</summary>
internal abstract partial record KiHunterAcidSpitCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private KiHunterAcidSpitCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this ki hunter acid spit cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(KiHunterAcidSpitCueReactions<TContext> reactions, TContext context);


    /// <summary>Launches the acid out of the mouth the cue's way (#2163).</summary>
    /// <param name="Direction">The way the acid leaves.</param>
    internal sealed partial record Launch(HorizontalDirection Direction) : KiHunterAcidSpitCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(KiHunterAcidSpitCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Launch(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(KiHunterAcidSpitCueKind Kind) : KiHunterAcidSpitCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(KiHunterAcidSpitCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }
}
