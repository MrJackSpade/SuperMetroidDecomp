namespace SuperMetroid.Core.Enemies.Deaths;

/// <summary>
/// The generic enemy-death explosions' animation segments (#641, #2554), in authored order; each ends by becoming its
/// drop, which the drops' own animations show (<see cref="EnemyDeathAndDropSegment"/>). Their steps are
/// <see cref="EnemyDeathExplosionAnimations"/>.
/// </summary>
internal enum EnemyDeathExplosionSegment
{
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
    /// <summary>
    /// A cluster of fireballs, starbursts and embers with the contact-kill sound, four embers then ashes flying apart,
    /// before it becomes a drop.
    /// </summary>
    KilledBySamusContact,
}
