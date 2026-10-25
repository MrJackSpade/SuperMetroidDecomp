namespace SuperMetroid.Core.Enemies;

/// <summary>The population parameter words verification seeds and reads for Rio-like followers (#1412).</summary>
internal static class EnemyParameterMasks
{
    /// <summary>Parameter-one bit 15: the follower parameter, which production reads as a negative parameter.</summary>
    internal const ushort FollowerBit = 0x8000;
}
