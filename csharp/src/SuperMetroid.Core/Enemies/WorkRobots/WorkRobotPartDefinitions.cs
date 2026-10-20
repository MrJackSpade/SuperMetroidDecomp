namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>The definitions the WorkRobot species installs and the part each one is.</summary>
internal static class WorkRobotPartDefinitions
{
    /// <summary>Each WorkRobot definition and its part.</summary>
    internal static IReadOnlyDictionary<EnemyDefinitionId, WorkRobotPart> Parts { get; } = new Dictionary<EnemyDefinitionId, WorkRobotPart>
    {
        [EnemyDefinitionId.Robot] = WorkRobotPart.Robot,
        [EnemyDefinitionId.RobotNoPower] = WorkRobotPart.RobotNoPower,
    };
}
