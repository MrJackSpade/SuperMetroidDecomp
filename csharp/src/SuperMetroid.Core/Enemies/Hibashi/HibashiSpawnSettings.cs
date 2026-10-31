namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>How long a Hibashi rests and whether it is the collision part (#1472).</summary>
/// <param name="InactiveTime">Updates, 0 through 65535, it rests between flames.</param>
/// <param name="Part">Which part of the Hibashi it is.</param>
internal sealed record HibashiSpawnSettings(int InactiveTime, HibashiPart Part) : EnemySpawnSettings
{
    /// <summary>Updates, 0 through 65535, it rests between flames.</summary>
    public int InactiveTime { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, InactiveTime);
}
