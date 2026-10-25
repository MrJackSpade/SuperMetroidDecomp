using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>The typed death programs shared by generic and family-specific enemy deaths.</summary>
internal static class EnemyDeathExplosionDefinitions
{
    /// <summary>The blank-frame wait a drop runs after it is collected or expires ($86:ECA3).</summary>
    internal const EnemyDeathProjectileProgram NoDropTail = EnemyDeathProjectileProgram.RespawnTail;

    /// <summary>
    /// The death program each death-animation selector starts: zero a small explosion, one the Samus-contact kill, two a
    /// normal explosion, three Mini-Kraid's explosion and four a big explosion. The selector is a number because
    /// callers pass computed values (some pass the enemy's remaining health), and its range check is observable.
    /// </summary>
    internal static EnemyDeathProjectileProgram Program(int animation) => animation switch
    {
        0 => EnemyDeathProjectileProgram.SmallExplosion,
        1 => EnemyDeathProjectileProgram.KilledBySamusContact,
        2 => EnemyDeathProjectileProgram.NormalExplosion,
        3 => EnemyDeathProjectileProgram.MiniKraidExplosion,
        4 => EnemyDeathProjectileProgram.BigExplosion,
        _ => throw new ArgumentOutOfRangeException(nameof(animation), animation, "Enemy death animation must be zero through four."),
    };
}
