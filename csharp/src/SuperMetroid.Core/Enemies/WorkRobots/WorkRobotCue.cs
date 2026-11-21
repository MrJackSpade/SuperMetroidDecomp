namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One cue of the Work Robot's animations: what it does, with the values it acts on (#2151).</summary>
internal abstract partial record WorkRobotCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private WorkRobotCue()
    {
    }

    /// <summary>What <paramref name="cases"/> makes of the cue, through the case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="cases">What each case of the cue is made into.</param>
    /// <typeparam name="TContext">What the case's reaction reads besides the case.</typeparam>
    /// <param name="context">What the case's reaction reads besides the case, handed to it so the cases are built once.</param>
    internal abstract TResult Match<TContext, TResult>(WorkRobotCueCases<TContext, TResult> cases, TContext context);

    /// <summary>Takes one walk step; a wall, or a ledge where the step watches for one, redirects the walk.</summary>
    /// <param name="Move">The step.</param>
    internal sealed partial record Step(WorkRobotMoveStep Move) : WorkRobotCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(WorkRobotCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Step(this, context);
    }

    /// <summary>Fires the cue's laser when the cooldown allows, continuing at its firing animation.</summary>
    /// <param name="Laser">The laser it fires.</param>
    /// <param name="Firing">The firing animation it continues at.</param>
    internal sealed partial record TryLaser(WorkRobotLaser Laser, WorkRobotSegment Firing) : WorkRobotCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(WorkRobotCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).TryLaser(this, context);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed partial record Signal(WorkRobotCueKind Kind) : WorkRobotCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TContext, TResult>(WorkRobotCueCases<TContext, TResult> cases, TContext context) => Ensure.NotNull(cases).Signal(this, context);
    }
}

/// <summary>What each case of a <see cref="WorkRobotCue"/> is made into (#2553).</summary>
/// <typeparam name="TResult">What the cue is made into.</typeparam>
/// <param name="Step">Makes a <see cref="WorkRobotCue.Step"/> into the result.</param>
/// <param name="TryLaser">Makes a <see cref="WorkRobotCue.TryLaser"/> into the result.</param>
/// <param name="Signal">Makes a <see cref="WorkRobotCue.Signal"/> into the result.</param>
/// <typeparam name="TContext">What each case reads besides itself.</typeparam>
internal sealed record WorkRobotCueCases<TContext, TResult>(Func<WorkRobotCue.Step, TContext, TResult> Step, Func<WorkRobotCue.TryLaser, TContext, TResult> TryLaser, Func<WorkRobotCue.Signal, TContext, TResult> Signal);
