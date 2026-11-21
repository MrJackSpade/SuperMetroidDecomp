namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One cue of the Chozo statue's animations: what it does, with the values it acts on.</summary>
internal abstract partial record ChozoStatueCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private ChozoStatueCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this chozo statue cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(ChozoStatueCueReactions<TContext> reactions, TContext context);


    /// <summary>Clears spikes under the cue's footstep offset.</summary>
    /// <param name="OffsetX">How far right of the statue a <see cref="Footstep"/> cue probes for spikes, in pixels.</param>
    internal sealed partial record Footstep(int OffsetX) : ChozoStatueCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(ChozoStatueCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Footstep(this, context);
    }

    /// <summary>Moves the statue by the cue's carry step and carries Samus.</summary>
    /// <param name="Step">The facing and carry pose a <see cref="Move"/> cue moves by.</param>
    internal sealed partial record Move(ChozoCarryStep Step) : ChozoStatueCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(ChozoStatueCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Move(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(ChozoStatueCueKind Kind) : ChozoStatueCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(ChozoStatueCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }
}

/// <summary>What the owner of a <see cref="ChozoStatueCue"/> does on each of its cases (#2553).</summary>
/// <param name="Footstep">Reacts to a <see cref="ChozoStatueCue.Footstep"/>.</param>
/// <param name="Move">Reacts to a <see cref="ChozoStatueCue.Move"/>.</param>
/// <param name="Signal">Reacts to a <see cref="ChozoStatueCue.Signal"/>.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record ChozoStatueCueReactions<TContext>(Action<ChozoStatueCue.Footstep, TContext> Footstep, Action<ChozoStatueCue.Move, TContext> Move, Action<ChozoStatueCue.Signal, TContext> Signal);
