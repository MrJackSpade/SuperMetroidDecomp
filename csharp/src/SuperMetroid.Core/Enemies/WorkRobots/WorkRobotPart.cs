namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Which WorkRobot occupant a slot holds, one per definition the species installs.</summary>
internal enum WorkRobotPart
{
    /// <summary>The <see cref="EnemyDefinitionId.Robot"/> occupant.</summary>
    Robot,
    /// <summary>The <see cref="EnemyDefinitionId.RobotNoPower"/> occupant.</summary>
    RobotNoPower,
}
