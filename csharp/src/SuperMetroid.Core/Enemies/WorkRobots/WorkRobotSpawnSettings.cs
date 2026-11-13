namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Which pose a work robot starts in (#1472, #1519).</summary>
/// <param name="Pose">
/// The unpowered pose it holds: <see cref="WorkRobotSegment.NoPowerNeutral"/>, <see cref="WorkRobotSegment.NoPowerLeaningLeft"/>
/// or <see cref="WorkRobotSegment.NoPowerLeaningRight"/>.
/// </param>
internal sealed record WorkRobotSpawnSettings(WorkRobotSegment Pose) : EnemySpawnSettings;
