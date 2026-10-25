namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The projectiles the n00b-tube PLM spawns. The values are arbitrary and nothing reads them; the projectile engine maps
/// each to its projectile kind (#1414).
/// </summary>
public enum NoobTubeProjectileDefinition
{
    /// <summary>The tube's crack.</summary>
    Crack = 1,

    /// <summary>A glass shard.</summary>
    Shard,

    /// <summary>A released-air bubble.</summary>
    ReleasedAirBubble,
}
