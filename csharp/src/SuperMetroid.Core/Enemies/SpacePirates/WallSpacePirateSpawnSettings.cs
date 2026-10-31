namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How a wall space pirate jumps (#1472).</summary>
/// <param name="Options">Its options.</param>
/// <param name="JumpSpan">Pixels, 0 through 65535, between its walls.</param>
internal sealed record WallSpacePirateSpawnSettings(WallSpacePirateParameterFlags Options, int JumpSpan) : EnemySpawnSettings
{
    /// <summary>Pixels, 0 through 65535, between its walls.</summary>
    public int JumpSpan { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, JumpSpan);
}
