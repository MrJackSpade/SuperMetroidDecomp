namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>Which pose a work robot starts in (#1472).</summary>
/// <param name="Pose">Its pose, 0 through 65535; a pose outside 0 through 3 starts the first.</param>
internal sealed record WorkRobotSpawnSettings(int Pose) : EnemySpawnSettings
{
    /// <summary>Its pose, 0 through 65535; a pose outside 0 through 3 starts the first.</summary>
    public int Pose { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, Pose);
}
