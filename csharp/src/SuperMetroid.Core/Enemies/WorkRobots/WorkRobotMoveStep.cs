namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One Work Robot walk step: distance, facings, wall/ledge/fall continuations (#633).</summary>
internal sealed record WorkRobotMoveStep
{
    /// <summary>The whole pixels the step moves the robot, and Samus standing on it: negative left, positive right.</summary>
    public required int HorizontalPixels { get; init; }

    /// <summary>The way the step faces the robot.</summary>
    public required HorizontalDirection Facing { get; init; }

    /// <summary>The animation a wall turns the walk into.</summary>
    public required WorkRobotSegment WallAnimation { get; init; }

    /// <summary>Whether the step watches for a ledge ahead after it moves.</summary>
    public required bool CheckLedge { get; init; }

    /// <summary>Which way the ledge probe looks, a body's width away: -1 left, 1 right, 0 when the step does not watch.</summary>
    public required int LedgeProbeDirection { get; init; }

    /// <summary>The animation a ledge turns the walk into, when the step watches for ledges.</summary>
    public required WorkRobotSegment? FallAnimation { get; init; }

    /// <summary>The way a ledge turns the robot, when the step watches for ledges.</summary>
    public required HorizontalDirection? FallFacing { get; init; }
}
