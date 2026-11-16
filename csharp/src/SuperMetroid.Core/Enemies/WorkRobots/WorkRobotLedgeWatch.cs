namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The ledge a Work Robot walk step watches for (#2183): the side its foot probe reaches out to, one robot's width
/// beyond its centre, and the fall a missing floor there turns the walk into.
/// </summary>
/// <param name="ProbeSide">The side the probe reaches out to.</param>
/// <param name="FallAnimation">The animation a ledge turns the walk into.</param>
/// <param name="FallFacing">The way a ledge turns the robot.</param>
internal sealed record WorkRobotLedgeWatch(HorizontalDirection ProbeSide, WorkRobotSegment FallAnimation, HorizontalDirection FallFacing);
