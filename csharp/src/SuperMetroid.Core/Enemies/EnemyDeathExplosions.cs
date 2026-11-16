using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>Chooses the death animation an enemy's death explosion starts.</summary>
internal static class EnemyDeathExplosions
{
    /// <summary>The death animation <paramref name="explosion"/> starts.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="explosion"/> is no death explosion.</exception>
    internal static Animation<EnemyDeathProjectileSegment> Animation(EnemyDeathExplosion explosion) =>
        EnemyDeathProjectileAnimations.Explosions.TryGetValue(explosion, out Animation<EnemyDeathProjectileSegment>? animation)
            ? animation
            : throw new ArgumentOutOfRangeException(nameof(explosion), explosion, "Undefined enemy death explosion.");

    /// <summary>
    /// The explosion a Super Missile kill raises <paramref name="explosion"/> to: the small, contact and normal
    /// explosions become the normal explosion, and Fake Kraid's and the big explosion stay.
    /// </summary>
    internal static EnemyDeathExplosion RaisedBySuperMissile(EnemyDeathExplosion explosion) => explosion switch
    {
        EnemyDeathExplosion.Small or EnemyDeathExplosion.SamusContact or EnemyDeathExplosion.Normal =>
            EnemyDeathExplosion.Normal,
        EnemyDeathExplosion.FakeKraid or EnemyDeathExplosion.Big => explosion,
        _ => throw new ArgumentOutOfRangeException(nameof(explosion), explosion, "Undefined enemy death explosion."),
    };
}
