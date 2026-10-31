namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>How a Choot jumps and falls (#1472).</summary>
/// <param name="PatternIndex">Its falling pattern, 0 through 255.</param>
/// <param name="PatternLoops">The loops, 0 through 255, of its falling pattern.</param>
/// <param name="JumpDelay">Updates, 0 through 65535, before its first jump.</param>
internal sealed record ChootSpawnSettings(int PatternIndex, int PatternLoops, int JumpDelay) : EnemySpawnSettings
{
    /// <summary>Its falling pattern, 0 through 255.</summary>
    public int PatternIndex { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, PatternIndex);

    /// <summary>The loops, 0 through 255, of its falling pattern.</summary>
    public int PatternLoops { get; init; } = Ensure.BetweenInclusive(0, byte.MaxValue, PatternLoops);

    /// <summary>Updates, 0 through 65535, before its first jump.</summary>
    public int JumpDelay { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue, JumpDelay);
}
