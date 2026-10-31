using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>The typed death programs shared by generic and family-specific enemy deaths.</summary>
internal static class EnemyDeathExplosionDefinitions
{
    /// <summary>The blank-frame wait a drop runs after it is collected or expires ($86:ECA3).</summary>
    internal const EnemyDeathProjectileProgram NoDropTail = EnemyDeathProjectileProgram.RespawnTail;

    /// <summary>
    /// The explosion a Fireflea's contact death starts for each remaining health from zero: the explosions in their
    /// authored order.
    /// </summary>
    internal static readonly EnemyDeathExplosion[] ByLeftoverNumber =
    [
        EnemyDeathExplosion.Small,
        EnemyDeathExplosion.SamusContact,
        EnemyDeathExplosion.Normal,
        EnemyDeathExplosion.MiniKraid,
        EnemyDeathExplosion.Big,
    ];
}
