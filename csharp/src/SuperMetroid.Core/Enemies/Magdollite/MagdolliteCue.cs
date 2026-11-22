namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>One cue of the Magdollite's animations: what it does, with the values it acts on (#2071).</summary>
internal abstract record MagdolliteCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private MagdolliteCue()
    {
    }

    /// <summary>Carries out the reaction <paramref name="reactions"/> has to this magdollite cue, through the case it is.</summary>
    /// <param name="reactions">What its owner does on each case.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract void React<TContext>(MagdolliteCueReactions<TContext> reactions, TContext context);


    /// <summary>Records an enemy-library sound while the Magdollite is on screen.</summary>
    /// <param name="Sound">The sound the cue plays.</param>
    internal sealed record PlaySoundIfOnScreen(SoundEffectId Sound) : MagdolliteCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MagdolliteCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).PlaySoundIfOnScreen(this, context);
    }

    /// <summary>Moves the part.</summary>
    /// <param name="Offset">How far the part moves.</param>
    internal sealed record Move(Offset16<RoomPixels> Offset) : MagdolliteCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MagdolliteCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Move(this, context);
    }

    /// <summary>Moves the part and the one following it, the pillar it stands on.</summary>
    /// <param name="Offset">How far both parts move.</param>
    internal sealed record MoveWithPillar(Offset16<RoomPixels> Offset) : MagdolliteCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MagdolliteCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).MoveWithPillar(this, context);
    }

    /// <summary>Places the part at the throw origin shifted by the cue's offset.</summary>
    /// <param name="Offset">Where from the throw origin the part is placed.</param>
    internal sealed record ShiftFromThrowOrigin(Offset16<RoomPixels> Offset) : MagdolliteCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MagdolliteCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).ShiftFromThrowOrigin(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(MagdolliteCueKind Kind) : MagdolliteCue
    {
        /// <inheritdoc/>
        internal override void React<TContext>(MagdolliteCueReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).Signal(this, context);
    }
}
