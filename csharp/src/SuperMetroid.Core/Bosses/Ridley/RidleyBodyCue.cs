namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One cue of Ridley's body animations: what it does, with the values it acts on.</summary>
internal abstract record RidleyBodyCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private RidleyBodyCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of this ridley body cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the ridley body cue is made into.</typeparam>
    /// <param name="cases">What each case is made into.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(RidleyBodyCueCases<TContext, TResult> cases, TContext context);


    /// <summary>Moves Ridley by the cue's displacement.</summary>
    /// <param name="Displacement">How far a <see cref="Move"/> cue moves Ridley.</param>
    internal sealed record Move(Offset16<RoomPixels> Displacement) : RidleyBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(RidleyBodyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Move(this, context);
    }

    /// <summary>Selects how far Ridley's legs reach, which sets his claws' height.</summary>
    /// <param name="Height">The claw height a <see cref="SetClawHeight"/> cue selects.</param>
    internal sealed record SetClawHeight(RidleyClawHeight Height) : RidleyBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(RidleyBodyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).SetClawHeight(this, context);
    }

    /// <summary>Faces the cue's way and mirrors the tail (#2165).</summary>
    /// <param name="Facing">The way Ridley then faces.</param>
    internal sealed record Flip(HorizontalDirection Facing) : RidleyBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(RidleyBodyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Flip(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(RidleyBodyCueKind Kind) : RidleyBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(RidleyBodyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Signal(this, context);
    }
}
