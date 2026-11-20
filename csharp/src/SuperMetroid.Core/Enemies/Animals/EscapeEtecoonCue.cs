namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One cue of the escaping Etecoons' animations: what it does, with the values it acts on.</summary>
internal abstract record EscapeEtecoonCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EscapeEtecoonCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of this escape etecoon cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the escape etecoon cue is made into.</typeparam>
    /// <param name="cases">What each case is made into.</param>
    internal abstract TResult Match<TResult>(EscapeEtecoonCueCases<TResult> cases);


    /// <summary>
    /// A low-tide run starts (#2167): once the rising acid is above its low surface (<see cref="EscapeAnimalMotionDefinitions.LowAcidSurfaceY"/>) the owner starts the high-tide run the same way
    /// instead.
    /// </summary>
    /// <param name="Running">The way the Etecoon runs.</param>
    internal sealed record LowTideRunStarts(HorizontalDirection Running) : EscapeEtecoonCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(EscapeEtecoonCueCases<TResult> cases) => Ensure.NotNull(cases).LowTideRunStarts(this);
    }

    /// <summary>Moves the Etecoon horizontally.</summary>
    /// <param name="Offset">The cue's signed pixel offset.</param>
    internal sealed record Move(int Offset) : EscapeEtecoonCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(EscapeEtecoonCueCases<TResult> cases) => Ensure.NotNull(cases).Move(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(EscapeEtecoonCueKind Kind) : EscapeEtecoonCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(EscapeEtecoonCueCases<TResult> cases) => Ensure.NotNull(cases).Signal(this);
    }
}

/// <summary>What each case of a <see cref="EscapeEtecoonCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the escape etecoon cue is made into.</typeparam>
/// <param name="LowTideRunStarts">Makes a <see cref="EscapeEtecoonCue.LowTideRunStarts"/> into the result.</param>
/// <param name="Move">Makes a <see cref="EscapeEtecoonCue.Move"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="EscapeEtecoonCue.Signal"/> into the result.</param>
internal sealed record EscapeEtecoonCueCases<TResult>(Func<EscapeEtecoonCue.LowTideRunStarts, TResult> LowTideRunStarts, Func<EscapeEtecoonCue.Move, TResult> Move, Func<EscapeEtecoonCue.Signal, TResult> Signal);
