namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Which WorkRobot occupant an enemy place holds, one per definition the species installs.</summary>
internal enum WorkRobotPart
{
    /// <summary>The <see cref="EnemyDefinitionId.WorkRobot"/> occupant.</summary>
    Powered,
    /// <summary>The <see cref="EnemyDefinitionId.UnpoweredWorkRobot"/> occupant.</summary>
    Unpowered,
}
