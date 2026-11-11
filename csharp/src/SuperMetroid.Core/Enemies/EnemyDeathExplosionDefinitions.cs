using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>The typed death programs shared by generic and family-specific enemy deaths.</summary>
internal static class EnemyDeathExplosionDefinitions
{
    /// <summary>The blank-frame wait a drop runs after it is collected or expires.</summary>
    internal const EnemyDeathProjectileProgram NoDropTail = EnemyDeathProjectileProgram.RespawnTail;

    /// <summary>
    /// The random field that places a death explosion's sprite 0-63 pixels right of the 32 it is centred back by: the
    /// low six bits.
    /// </summary>
    internal static readonly RandomSampleField WideSpriteHorizontalScatter = new(0, 6);

    /// <summary>
    /// The random field that places a death explosion's sprite 0-63 pixels below the 32 it is centred back by: bits eight
    /// to thirteen of the random sample.
    /// </summary>
    internal static readonly RandomSampleField WideSpriteVerticalScatter = new(8, 6);

    /// <summary>
    /// The random field that places a death explosion's sprite 0-31 pixels right of the 16 it is centred back by: the
    /// low five bits.
    /// </summary>
    internal static readonly RandomSampleField NarrowSpriteHorizontalScatter = new(0, 5);

    /// <summary>
    /// The random field that places a death explosion's sprite 0-31 pixels below the 16 it is centred back by: bits eight
    /// to twelve of the random sample.
    /// </summary>
    internal static readonly RandomSampleField NarrowSpriteVerticalScatter = new(8, 5);

    /// <summary>
    /// The explosion a death started with a leftover number starts for each number from zero: the explosions in their
    /// authored order (<see cref="EnemyDeathExplosions.ForLeftoverNumber"/>).
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
