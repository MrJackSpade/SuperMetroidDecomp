namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One Work Robot walk step: distance, facing, and the wall and ledge continuations (#633).</summary>
internal sealed record WorkRobotMoveStep
{
    /// <summary>The signed pixels the step moves the robot, positive toward larger X.</summary>
    public required int HorizontalPixels { get; init; }

    /// <summary>The way the step faces the robot.</summary>
    public required HorizontalDirection Facing { get; init; }

    /// <summary>The animation a wall turns the walk into.</summary>
    public required WorkRobotSegment WallAnimation { get; init; }

    /// <summary>The ledge the step watches for, and the fall a ledge turns the walk into; none for a step that watches for none (#2183).</summary>
    public required WorkRobotLedgeWatch? Ledge { get; init; }
}
