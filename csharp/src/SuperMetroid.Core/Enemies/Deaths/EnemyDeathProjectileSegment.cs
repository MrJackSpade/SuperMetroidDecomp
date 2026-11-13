namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// The generic enemy-death explosions, the drops they become and the respawn tail (#641): one segment per animation a spawn, branch or family step enters, in authored order.
/// Their steps are <see cref="EnemyDeathProjectileAnimations"/>.
/// </summary>
internal enum EnemyDeathProjectileSegment
{
    /// <summary>The respawn tail animation.</summary>
    RespawnTail,
    /// <summary>The big explosion animation.</summary>
    BigExplosion,
    /// <summary>The loop.</summary>
    BigExplosionLoop,
    /// <summary>The mini Kraid explosion animation.</summary>
    MiniKraidExplosion,
    /// <summary>The loop.</summary>
    MiniKraidExplosionLoop,
    /// <summary>The normal explosion animation.</summary>
    NormalExplosion,
    /// <summary>The small explosion animation.</summary>
    SmallExplosion,
    /// <summary>The small energy animation.</summary>
    SmallEnergy,
    /// <summary>The big energy animation.</summary>
    BigEnergy,
    /// <summary>The missiles animation.</summary>
    Missiles,
    /// <summary>The super missiles animation.</summary>
    SuperMissiles,
    /// <summary>The power bombs animation.</summary>
    PowerBombs,
    /// <summary>The killed by Samus contact animation.</summary>
    KilledBySamusContact,
}
