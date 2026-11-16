namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One cue of the Work Robot's animations: what it does, with the values it acts on (#2151).</summary>
internal abstract record WorkRobotCue
{
    /// <summary>Closes the hierarchy to the cues declared here.</summary>
    private WorkRobotCue()
    {
    }

    /// <summary>Takes one walk step; a wall, or a ledge where the step watches for one, redirects the walk.</summary>
    /// <param name="Move">The step.</param>
    internal sealed record Step(WorkRobotMoveStep Move) : WorkRobotCue;

    /// <summary>Fires the cue's laser when the cooldown allows, continuing at its firing animation.</summary>
    /// <param name="Laser">The laser it fires.</param>
    /// <param name="Firing">The firing animation it continues at.</param>
    internal sealed record TryLaser(WorkRobotLaser Laser, WorkRobotSegment Firing) : WorkRobotCue;

    /// <summary>Continues at the cue's walk.</summary>
    /// <param name="Walking">The walk it continues at.</param>
    internal sealed record Walk(WorkRobotSegment Walking) : WorkRobotCue;

    /// <summary>A cue that signals what it does and carries nothing else.</summary>
    /// <param name="Kind">What the cue does.</param>
    internal sealed record Signal(WorkRobotCueKind Kind) : WorkRobotCue;
}
