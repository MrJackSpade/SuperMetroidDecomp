using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>Chooses the death program an enemy's death explosion starts.</summary>
internal static class EnemyDeathExplosions
{
    /// <summary>The death program <paramref name="explosion"/> starts.</summary>
    internal static EnemyDeathProjectileProgram Program(EnemyDeathExplosion explosion) => explosion switch
    {
        EnemyDeathExplosion.Small => EnemyDeathProjectileProgram.SmallExplosion,
        EnemyDeathExplosion.SamusContact => EnemyDeathProjectileProgram.KilledBySamusContact,
        EnemyDeathExplosion.Normal => EnemyDeathProjectileProgram.NormalExplosion,
        EnemyDeathExplosion.MiniKraid => EnemyDeathProjectileProgram.MiniKraidExplosion,
        EnemyDeathExplosion.Big => EnemyDeathProjectileProgram.BigExplosion,
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

    /// <summary>
    /// The explosion a death started with a leftover number selects. A few deaths pass whatever number their routine
    /// ends with as the explosion: a Fireflea its remaining health, Kraid's spit rock the shared health accumulator
    /// and Powamp its spike count. Zero through four select the explosions in their authored order and any higher
    /// number the small explosion.
    /// </summary>
    internal static EnemyDeathExplosion ForLeftoverNumber(int number) =>
        (uint)number < (uint)EnemyDeathExplosionDefinitions.ByLeftoverNumber.Length
            ? EnemyDeathExplosionDefinitions.ByLeftoverNumber[number]
            : EnemyDeathExplosion.Small;
}
