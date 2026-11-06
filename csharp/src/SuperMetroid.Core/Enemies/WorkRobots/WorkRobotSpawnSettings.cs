namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Which pose a work robot starts in (#1472, #1519).</summary>
/// <param name="Pose">
/// The unpowered pose it holds: <see cref="WorkRobotProgram.NoPowerNeutral"/>, <see cref="WorkRobotProgram.NoPowerLeaningLeft"/>
/// or <see cref="WorkRobotProgram.NoPowerLeaningRight"/>.
/// </param>
internal sealed record WorkRobotSpawnSettings(WorkRobotProgram Pose) : EnemySpawnSettings;
