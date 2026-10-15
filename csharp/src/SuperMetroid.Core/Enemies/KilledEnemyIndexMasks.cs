namespace SuperMetroid.Core.Enemies;

/// <summary>How a death explosion's killed-enemy word records the enemy it came from.</summary>
internal static class KilledEnemyIndexMasks
{
    /// <summary>Bit 15: the killed enemy respawns from its snapshot when the explosion's respawn instruction runs.</summary>
    internal const ushort RespawnMarker = 0x8000;

    /// <summary>The killed enemy's native index beneath the respawn marker.</summary>
    internal const ushort IndexBits = 0x7fff;

    /// <summary>No killed enemy: a drop spawned directly rather than from a death.</summary>
    internal const ushort None = 0xffff;
}
