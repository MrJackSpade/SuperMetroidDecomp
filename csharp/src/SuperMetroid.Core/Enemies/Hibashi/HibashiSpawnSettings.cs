namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>How long a Hibashi rests and whether it is the collision part (#1472).</summary>
/// <param name="InactiveTime">Updates, 0 through 65535, it rests between flames.</param>
/// <param name="IsCollisionPart">Whether it is the collision part.</param>
internal sealed record HibashiSpawnSettings(int InactiveTime, bool IsCollisionPart) : EnemySpawnSettings
{
    /// <summary>Updates, 0 through 65535, it rests between flames.</summary>
    public int InactiveTime { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, InactiveTime);
}
