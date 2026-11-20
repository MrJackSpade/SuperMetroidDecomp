namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One cue of the Work Robot's animations: what it does, with the values it acts on (#2151).</summary>
internal abstract record WorkRobotCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private WorkRobotCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    internal abstract TResult Match<TResult>(WorkRobotCueCases<TResult> cases);

    /// <summary>Takes one walk step; a wall, or a ledge where the step watches for one, redirects the walk.</summary>
    /// <param name="Move">The step.</param>
    internal sealed record Step(WorkRobotMoveStep Move) : WorkRobotCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(WorkRobotCueCases<TResult> cases) => Ensure.NotNull(cases).Step(this);
    }

    /// <summary>Fires the cue's laser when the cooldown allows, continuing at its firing animation.</summary>
    /// <param name="Laser">The laser it fires.</param>
    /// <param name="Firing">The firing animation it continues at.</param>
    internal sealed record TryLaser(WorkRobotLaser Laser, WorkRobotSegment Firing) : WorkRobotCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(WorkRobotCueCases<TResult> cases) => Ensure.NotNull(cases).TryLaser(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(WorkRobotCueKind Kind) : WorkRobotCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(WorkRobotCueCases<TResult> cases) => Ensure.NotNull(cases).Signal(this);
    }
}

/// <summary>What each case of a <see cref="WorkRobotCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="Step">Makes a <see cref="WorkRobotCue.Step"/> into the result.</param>
/// <param name="TryLaser">Makes a <see cref="WorkRobotCue.TryLaser"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="WorkRobotCue.Signal"/> into the result.</param>
internal sealed record WorkRobotCueCases<TResult>(Func<WorkRobotCue.Step, TResult> Step, Func<WorkRobotCue.TryLaser, TResult> TryLaser, Func<WorkRobotCue.Signal, TResult> Signal);
