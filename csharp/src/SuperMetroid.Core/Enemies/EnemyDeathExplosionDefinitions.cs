using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>The typed death programs shared by generic and family-specific enemy deaths.</summary>
internal static class EnemyDeathExplosionDefinitions
{
    /// <summary>The blank-frame wait a drop runs after it is collected or expires ($86:ECA3).</summary>
    internal const EnemyDeathProjectileProgram NoDropTail = EnemyDeathProjectileProgram.RespawnTail;

    /// <summary>The death program of each mutually exclusive enemy-death animation variant ($86:EFD5-$86:EFDE).</summary>
    internal static EnemyDeathProjectileProgram Program(ushort animation) => (EnemyDeathAnimation)animation switch
    {
        EnemyDeathAnimation.SmallExplosion => EnemyDeathProjectileProgram.SmallExplosion,
        EnemyDeathAnimation.KilledBySamusContact => EnemyDeathProjectileProgram.KilledBySamusContact,
        EnemyDeathAnimation.NormalExplosion => EnemyDeathProjectileProgram.NormalExplosion,
        EnemyDeathAnimation.MiniKraidExplosion => EnemyDeathProjectileProgram.MiniKraidExplosion,
        EnemyDeathAnimation.BigExplosion => EnemyDeathProjectileProgram.BigExplosion,
        _ => throw new ArgumentOutOfRangeException(nameof(animation), animation, "Enemy death animation must be zero through four."),
    };
}
