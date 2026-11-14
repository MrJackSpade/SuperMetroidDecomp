namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One Work Robot walk step: distance, facings, wall/ledge/fall continuations (#633).</summary>
internal sealed record WorkRobotMoveStep
{
    /// <summary>Horizontal pixels.</summary>
    public required int HorizontalPixels { get; init; }

    /// <summary>The way the step faces the robot.</summary>
    public required HorizontalDirection Facing { get; init; }

    /// <summary>The animation a wall turns the walk into.</summary>
    public required WorkRobotSegment WallAnimation { get; init; }

    /// <summary>Check ledge.</summary>
    public required bool CheckLedge { get; init; }

    /// <summary>Ledge probe direction.</summary>
    public required int LedgeProbeDirection { get; init; }

    /// <summary>The animation a ledge turns the walk into, when the step watches for ledges.</summary>
    public required WorkRobotSegment? FallAnimation { get; init; }

    /// <summary>The way a ledge turns the robot, when the step watches for ledges.</summary>
    public required HorizontalDirection? FallFacing { get; init; }
}
