using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>Chooses the death animation an enemy's death explosion starts.</summary>
internal static class EnemyDeathExplosions
{
    /// <summary>The death animation <paramref name="explosion"/> starts.</summary>
    internal static EnemyDeathProjectileSegment Animation(EnemyDeathExplosion explosion) => explosion switch
    {
        EnemyDeathExplosion.Small => EnemyDeathProjectileSegment.SmallExplosion,
        EnemyDeathExplosion.SamusContact => EnemyDeathProjectileSegment.KilledBySamusContact,
        EnemyDeathExplosion.Normal => EnemyDeathProjectileSegment.NormalExplosion,
        EnemyDeathExplosion.MiniKraid => EnemyDeathProjectileSegment.MiniKraidExplosion,
        EnemyDeathExplosion.Big => EnemyDeathProjectileSegment.BigExplosion,
        _ => throw new ArgumentOutOfRangeException(nameof(explosion), explosion, "Undefined enemy death explosion."),
    };

    /// <summary>
    /// The explosion a Super Missile kill raises <paramref name="explosion"/> to: the small, contact and normal
    /// explosions become the normal explosion, and Mini-Kraid's and the big explosion stay.
    /// </summary>
    internal static EnemyDeathExplosion RaisedBySuperMissile(EnemyDeathExplosion explosion) => explosion switch
    {
        EnemyDeathExplosion.Small or EnemyDeathExplosion.SamusContact or EnemyDeathExplosion.Normal =>
            EnemyDeathExplosion.Normal,
        EnemyDeathExplosion.MiniKraid or EnemyDeathExplosion.Big => explosion,
        _ => throw new ArgumentOutOfRangeException(nameof(explosion), explosion, "Undefined enemy death explosion."),
    };
}
