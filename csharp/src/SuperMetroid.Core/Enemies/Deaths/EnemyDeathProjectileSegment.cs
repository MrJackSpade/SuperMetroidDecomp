namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// The generic enemy-death explosions, the drops they become and the respawn tail (#641) animation segments, in authored order.
/// Their steps are <see cref="EnemyDeathProjectileAnimations"/>.
/// </summary>
internal enum EnemyDeathProjectileSegment
{
    /// <summary>The respawn tail animation.</summary>
    RespawnTail,
    /// <summary>One burst of the big explosion, which its animation repeats.</summary>
    BigExplosion,
    /// <summary>The big explosion becoming its drop once its bursts are done.</summary>
    BigExplosionEnd,
    /// <summary>One burst of Fake Kraid's explosion, which its animation repeats.</summary>
    FakeKraidExplosion,
    /// <summary>Fake Kraid's explosion becoming its drop once its bursts are done.</summary>
    FakeKraidExplosionEnd,
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
