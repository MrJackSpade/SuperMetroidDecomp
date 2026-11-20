namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One cue of the Work Robot's animations: what it does, with the values it acts on (#2151).</summary>
internal abstract record WorkRobotCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private WorkRobotCue()
    {
    }

    /// <summary>What the function for this case makes of the cue, whichever case it is.</summary>
    /// <typeparam name="TResult">What the cue is made into.</typeparam>
    /// <param name="step">Makes a <see cref="Step"/> into the result.</param>
    /// <param name="tryLaser">Makes a <see cref="TryLaser"/> into the result.</param>
    /// <param name="signal">Makes a <see cref="Signal"/> into the result.</param>
    internal abstract TResult Match<TResult>(Func<Step, TResult> step, Func<TryLaser, TResult> tryLaser, Func<Signal, TResult> signal);

    /// <summary>Takes one walk step; a wall, or a ledge where the step watches for one, redirects the walk.</summary>
    /// <param name="Move">The step.</param>
    internal sealed record Step(WorkRobotMoveStep Move) : WorkRobotCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Step, TResult> step, Func<TryLaser, TResult> tryLaser, Func<Signal, TResult> signal) => Ensure.NotNull(step)(this);
    }

    /// <summary>Fires the cue's laser when the cooldown allows, continuing at its firing animation.</summary>
    /// <param name="Laser">The laser it fires.</param>
    /// <param name="Firing">The firing animation it continues at.</param>
    internal sealed record TryLaser(WorkRobotLaser Laser, WorkRobotSegment Firing) : WorkRobotCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Step, TResult> step, Func<TryLaser, TResult> tryLaser, Func<Signal, TResult> signal) => Ensure.NotNull(tryLaser)(this);
    }

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(WorkRobotCueKind Kind) : WorkRobotCue
    {
        /// <inheritdoc/>
        internal override TResult Match<TResult>(Func<Step, TResult> step, Func<TryLaser, TResult> tryLaser, Func<Signal, TResult> signal) => Ensure.NotNull(signal)(this);
    }
}
