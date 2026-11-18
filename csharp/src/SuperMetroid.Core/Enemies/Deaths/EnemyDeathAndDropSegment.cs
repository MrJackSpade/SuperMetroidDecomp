namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// The generic enemy-death explosions, the drops they become and the respawn pause (#641) animation segments, in authored order.
/// Their steps are <see cref="EnemyDeathAndDropAnimations"/>.
/// </summary>
internal enum EnemyDeathAndDropSegment
{
    /// <summary>An unseen pause before the enemy it stands for respawns, after which it is removed.</summary>
    RespawnTail,
    /// <summary>One burst of the big explosion, which its animation repeats.</summary>
    BigExplosion,
    /// <summary>The big explosion becoming its drop once its bursts are done.</summary>
    BigExplosionEnd,
    /// <summary>One burst of Fake Kraid's explosion, which its animation repeats.</summary>
    FakeKraidExplosion,
    /// <summary>Fake Kraid's explosion becoming its drop once its bursts are done.</summary>
    FakeKraidExplosionEnd,
    /// <summary>
    /// A fireball growing into a flaming ring with the kill sound, then a huge fireball and a wide ring fading out
    /// before it becomes a drop.
    /// </summary>
    NormalExplosion,
    /// <summary>
    /// A spark growing to a fireball with the kill sound, then a flaming ring, a starburst and embers before it becomes
    /// a drop.
    /// </summary>
    SmallExplosion,
    /// <summary>A small energy drop glowing dark, ringed and bright, then scattering, repeating.</summary>
    SmallEnergy,
    /// <summary>A big energy drop glowing bright, ringed and dark, then scattering, repeating.</summary>
    BigEnergy,
    /// <summary>A missile drop flashing, repeating.</summary>
    Missiles,
    /// <summary>A super missile drop flashing, repeating.</summary>
    SuperMissiles,
    /// <summary>A power bomb drop fading from bright to dim and flashing, repeating.</summary>
    PowerBombs,
    /// <summary>
    /// A cluster of fireballs, starbursts and embers with the contact-kill sound, four embers then ashes flying apart,
    /// before it becomes a drop.
    /// </summary>
    KilledBySamusContact,
}
