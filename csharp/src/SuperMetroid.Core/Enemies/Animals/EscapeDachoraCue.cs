namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One cue of the escaping Dachora's animations: what it does, with the values it acts on.</summary>
internal abstract record EscapeDachoraCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private EscapeDachoraCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of this escape dachora cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the escape dachora cue is made into.</typeparam>
    /// <param name="cases">What each case is made into.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(EscapeDachoraCueCases<TContext, TResult> cases, TContext context);


    /// <summary>
    /// A low-tide lap ends (#2167): once the rising acid is above its low surface (<see cref="EscapeAnimalMotionDefinitions.LowAcidSurfaceY"/>) the owner takes the Dachora on into the high-tide
    /// laps, counting on the laps it had left.
    /// </summary>
    /// <param name="Lap">The way the lap ran.</param>
    internal sealed record LowTideLapEnds(HorizontalDirection Lap) : EscapeDachoraCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(EscapeDachoraCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).LowTideLapEnds(this, context);
    }

    /// <summary>
    /// A lap ends, after a low-tide lap's tide test (#2167): once the critters have escaped the owner starts the Dachora's
    /// run for the escape.
    /// </summary>
    /// <param name="Lap">The way the lap ran.</param>
    internal sealed record LapEnds(HorizontalDirection Lap) : EscapeDachoraCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(EscapeDachoraCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).LapEnds(this, context);
    }

    /// <summary>Moves the Dachora by the cue's pixels (#2151).</summary>
    /// <param name="Offset">The cue's signed pixel offset.</param>
    internal sealed record Move(int Offset) : EscapeDachoraCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(EscapeDachoraCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Move(this, context);
    }
}

/// <summary>What each case of a <see cref="EscapeDachoraCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the escape dachora cue is made into.</typeparam>
/// <param name="LowTideLapEnds">Makes a <see cref="EscapeDachoraCue.LowTideLapEnds"/> into the result.</param>
/// <param name="LapEnds">Makes a <see cref="EscapeDachoraCue.LapEnds"/> into the result.</param>
/// <param name="Move">Makes a <see cref="EscapeDachoraCue.Move"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record EscapeDachoraCueCases<TContext, TResult>(Func<EscapeDachoraCue.LowTideLapEnds, TContext, TResult> LowTideLapEnds, Func<EscapeDachoraCue.LapEnds, TContext, TResult> LapEnds, Func<EscapeDachoraCue.Move, TContext, TResult> Move);
