namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>One cue of Ridley's body animations: what it does, with the values it acts on.</summary>
internal abstract partial record RidleyBodyCue
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
    internal sealed partial record Move(Offset16<RoomPixels> Displacement) : RidleyBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(RidleyBodyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Move(this, context);
    }

    /// <summary>Selects how far Ridley's legs reach, which sets his claws' height.</summary>
    /// <param name="Height">The claw height a <see cref="SetClawHeight"/> cue selects.</param>
    internal sealed partial record SetClawHeight(RidleyClawHeight Height) : RidleyBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(RidleyBodyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).SetClawHeight(this, context);
    }

    /// <summary>Faces the cue's way and mirrors the tail (#2165).</summary>
    /// <param name="Facing">The way Ridley then faces.</param>
    internal sealed partial record Flip(HorizontalDirection Facing) : RidleyBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(RidleyBodyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Flip(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(RidleyBodyCueKind Kind) : RidleyBodyCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(RidleyBodyCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Signal(this, context);
    }
}

/// <summary>What each case of a <see cref="RidleyBodyCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the ridley body cue is made into.</typeparam>
/// <param name="Move">Makes a <see cref="RidleyBodyCue.Move"/> into the result.</param>
/// <param name="SetClawHeight">Makes a <see cref="RidleyBodyCue.SetClawHeight"/> into the result.</param>
/// <param name="Flip">Makes a <see cref="RidleyBodyCue.Flip"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="RidleyBodyCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed partial record RidleyBodyCueCases<TContext, TResult>(Func<RidleyBodyCue.Move, TContext, TResult> Move, Func<RidleyBodyCue.SetClawHeight, TContext, TResult> SetClawHeight, Func<RidleyBodyCue.Flip, TContext, TResult> Flip, Func<RidleyBodyCue.Signal, TContext, TResult> Signal);
