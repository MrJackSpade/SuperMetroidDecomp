namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One cue of the Yapping Maw's animations: what it does, with the values it acts on.</summary>
internal abstract partial record YappingMawCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private YappingMawCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this yapping maw cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(YappingMawCueReactions<TContext> reactions, TContext context);


    /// <summary>Holds a grabbed Samus at the mouth offset of the cue's aim (#2071).</summary>
    /// <param name="Aim">The aim whose mouth offset Samus is held at.</param>
    internal sealed partial record HoldSamus(YappingMawAim Aim) : YappingMawCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(YappingMawCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).HoldSamus(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(YappingMawCueKind Kind) : YappingMawCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(YappingMawCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }
}

/// <summary>What the owner of a <see cref="YappingMawCue"/> does on each of its cases (#2553).</summary>
/// <param name="HoldSamus">Reacts to a <see cref="YappingMawCue.HoldSamus"/>.</param>
/// <param name="Signal">Reacts to a <see cref="YappingMawCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record YappingMawCueReactions<TContext>(Action<YappingMawCue.HoldSamus, TContext> HoldSamus, Action<YappingMawCue.Signal, TContext> Signal);
