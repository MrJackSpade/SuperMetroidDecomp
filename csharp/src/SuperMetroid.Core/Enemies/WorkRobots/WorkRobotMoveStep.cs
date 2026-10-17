namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One Work Robot walk step: distance, laser velocities, wall/ledge/fall continuations (#633).</summary>
internal sealed record WorkRobotMoveStep
{
    /// <summary>Horizontal pixels.</summary>
    public required int HorizontalPixels { get; init; }

    /// <summary>The laser velocity the step faces the robot with; only its X is meaningful.</summary>
    public required EightEightVector<RoomPixels> FacingLaserVelocity { get; init; }

    /// <summary>The animation a wall turns the walk into.</summary>
    public required WorkRobotProgram WallProgram { get; init; }

    /// <summary>Check ledge.</summary>
    public required bool CheckLedge { get; init; }

    /// <summary>Ledge probe direction.</summary>
    public required int LedgeProbeDirection { get; init; }

    /// <summary>The animation a ledge turns the walk into, when the step watches for ledges.</summary>
    public required WorkRobotProgram? FallProgram { get; init; }

    /// <summary>The laser velocity a ledge turns the robot to, when the step watches for ledges; only its X is meaningful.</summary>
    public required EightEightVector<RoomPixels> FallLaserVelocity { get; init; }
}
