using SuperMetroid.Core.Enemies.NoobTube;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The n00b tube's crack, glass shard and released-air bubble programs (#641), one per list a spawn, branch or family step enters, in authored order.
/// Generated from the verified lists; see <see cref="NoobTubeProjectileAnimations"/>.
/// </summary>
internal enum NoobTubeProjectileSegment
{
    /// <summary>Enemy projectile animation script: noob tube crack, part 0.</summary>
    Crack,
    /// <summary>The animation's repeating part.</summary>
    CrackLoop,
    /// <summary>Enemy projectile animation script: noob tube shard, parameter 0, part 0.</summary>
    Shard0,
    /// <summary>The animation's repeating part.</summary>
    Shard0Loop,
    /// <summary>The animation's repeating part.</summary>
    Shard0Loop2,
    /// <summary>Enemy projectile animation script: noob tube shard, parameter 2, part 0.</summary>
    Shard1,
    /// <summary>The animation's repeating part.</summary>
    Shard1Loop,
    /// <summary>The animation's repeating part.</summary>
    Shard1Loop2,
    /// <summary>Enemy projectile animation script: noob tube shard, parameter 4, part 0.</summary>
    Shard2,
    /// <summary>The animation's repeating part.</summary>
    Shard2Loop,
    /// <summary>The animation's repeating part.</summary>
    Shard2Loop2,
    /// <summary>Enemy projectile animation script: noob tube shard, parameter 6, part 0.</summary>
    Shard3,
    /// <summary>The animation's repeating part.</summary>
    Shard3Loop,
    /// <summary>The animation's repeating part.</summary>
    Shard3Loop2,
    /// <summary>Enemy projectile animation script: noob tube shard, parameter 8, part 0.</summary>
    Shard4,
    /// <summary>The animation's repeating part.</summary>
    Shard4Loop,
    /// <summary>The animation's repeating part.</summary>
    Shard4Loop2,
    /// <summary>Enemy projectile animation script: noob tube shard, parameter a, part 0.</summary>
    Shard5,
    /// <summary>The animation's repeating part.</summary>
    Shard5Loop,
    /// <summary>The animation's repeating part.</summary>
    Shard5Loop2,
    /// <summary>Enemy projectile animation script: noob tube shard, parameter c, part 0.</summary>
    Shard6,
    /// <summary>The animation's repeating part.</summary>
    Shard6Loop,
    /// <summary>The animation's repeating part.</summary>
    Shard6Loop2,
    /// <summary>Enemy projectile animation script: noob tube shard, parameter e, part 0.</summary>
    Shard7,
    /// <summary>The animation's repeating part.</summary>
    Shard7Loop,
    /// <summary>The animation's repeating part.</summary>
    Shard7Loop2,
    /// <summary>Enemy projectile animation script: noob tube shard, parameter 10, part 0.</summary>
    Shard8,
    /// <summary>The animation's repeating part.</summary>
    Shard8Loop,
    /// <summary>The animation's repeating part.</summary>
    Shard8Loop2,
    /// <summary>Enemy projectile animation script: noob tube shard, parameter 12, part 0.</summary>
    Shard9,
    /// <summary>The animation's repeating part.</summary>
    Shard9Loop,
    /// <summary>The animation's repeating part.</summary>
    Shard9Loop2,
    /// <summary>Enemy projectile animation script: noob tube released air bubbles.</summary>
    ReleasedAirBubble,
}
