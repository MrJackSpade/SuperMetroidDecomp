namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The steps the Work Robot's walk cues take (#2151): each moves the robot four pixels, faces it, and names the walk a
/// wall turns it into and, for a step that watches for ledges, the fall a ledge turns it into.
/// </summary>
internal static class WorkRobotMoveStepDefinitions
{
    /// <summary>The pixels one step moves the robot.</summary>
    private const int StepPixels = 4;

    /// <summary>A step forward facing left, watching for ledges.</summary>
    internal static WorkRobotMoveStep LeftForwardWatchingLedges { get; } = new()
    {
        HorizontalPixels = -StepPixels,
        Facing = HorizontalDirection.Left,
        WallAnimation = WorkRobotSegment.FacingLeftHitWallMovingForwards,
        Ledge = new(HorizontalDirection.Left, WorkRobotSegment.ApproachingFallLeft, HorizontalDirection.Right),
    };

    /// <summary>A step forward facing left.</summary>
    internal static WorkRobotMoveStep LeftForward { get; } = new()
    {
        HorizontalPixels = -StepPixels,
        Facing = HorizontalDirection.Left,
        WallAnimation = WorkRobotSegment.FacingLeftHitWallMovingForwards,
        Ledge = null,
    };

    /// <summary>A step backward facing left, watching for ledges.</summary>
    internal static WorkRobotMoveStep LeftBackwardWatchingLedges { get; } = new()
    {
        HorizontalPixels = StepPixels,
        Facing = HorizontalDirection.Left,
        WallAnimation = WorkRobotSegment.FacingLeftWalkingForwards,
        Ledge = new(HorizontalDirection.Right, WorkRobotSegment.ApproachingFallRight, HorizontalDirection.Left),
    };

    /// <summary>A step backward facing left.</summary>
    internal static WorkRobotMoveStep LeftBackward { get; } = new()
    {
        HorizontalPixels = StepPixels,
        Facing = HorizontalDirection.Left,
        WallAnimation = WorkRobotSegment.FacingLeftWalkingForwards,
        Ledge = null,
    };

    /// <summary>A step forward facing right, watching for ledges.</summary>
    internal static WorkRobotMoveStep RightForwardWatchingLedges { get; } = new()
    {
        HorizontalPixels = StepPixels,
        Facing = HorizontalDirection.Right,
        WallAnimation = WorkRobotSegment.FacingRightHitWallMovingForwards,
        Ledge = new(HorizontalDirection.Right, WorkRobotSegment.ApproachingFallRight, HorizontalDirection.Left),
    };

    /// <summary>A step forward facing right.</summary>
    internal static WorkRobotMoveStep RightForward { get; } = new()
    {
        HorizontalPixels = StepPixels,
        Facing = HorizontalDirection.Right,
        WallAnimation = WorkRobotSegment.FacingRightHitWallMovingForwards,
        Ledge = null,
    };

    /// <summary>A step backward facing right, watching for ledges.</summary>
    internal static WorkRobotMoveStep RightBackwardWatchingLedges { get; } = new()
    {
        HorizontalPixels = -StepPixels,
        Facing = HorizontalDirection.Right,
        WallAnimation = WorkRobotSegment.FacingLeftWalkingForwards,
        Ledge = new(HorizontalDirection.Left, WorkRobotSegment.ApproachingFallLeft, HorizontalDirection.Right),
    };

    /// <summary>A step backward facing right.</summary>
    internal static WorkRobotMoveStep RightBackward { get; } = new()
    {
        HorizontalPixels = -StepPixels,
        Facing = HorizontalDirection.Right,
        WallAnimation = WorkRobotSegment.FacingLeftWalkingForwards,
        Ledge = null,
    };
}
