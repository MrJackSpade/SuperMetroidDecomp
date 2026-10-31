namespace SuperMetroid.Core.Enemies.MorphBallEye;

/// <summary>Where a morph-ball eye is mounted (#1472).</summary>
/// <param name="Facing">The way it faces while it waits.</param>
/// <param name="Part">Which part of the morph-ball eye it is.</param>
/// <param name="MountDirection">Its mount direction, 0 through 15.</param>
internal sealed record MorphBallEyeSpawnSettings(HorizontalDirection Facing, MorphBallEyePart Part, int MountDirection) : EnemySpawnSettings
{
    /// <summary>Its mount direction, 0 through 15.</summary>
    public int MountDirection { get; init; } = Ensure.BetweenInclusive(0, 15, MountDirection);
}
