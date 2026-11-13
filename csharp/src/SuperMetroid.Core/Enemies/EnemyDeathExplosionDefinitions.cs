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
    /// The random field that places a death explosion's sprite 0-63 pixels below the 32 it is centred back by: the random
    /// sample divided by 256, modulo 64.
    /// </summary>
    internal static readonly RandomSampleField WideSpriteVerticalScatter = new(8, 6);

    /// <summary>
    /// The random field that places a death explosion's sprite 0-31 pixels right of the 16 it is centred back by: the
    /// low five bits.
    /// </summary>
    internal static readonly RandomSampleField NarrowSpriteHorizontalScatter = new(0, 5);

    /// <summary>
    /// The random field that places a death explosion's sprite 0-31 pixels below the 16 it is centred back by: the random
    /// sample divided by 256, modulo 32.
    /// </summary>
    internal static readonly RandomSampleField NarrowSpriteVerticalScatter = new(8, 5);

    /// <summary>
    /// The death explosions in their authored order, the small explosion first: the order the deaths that pick their
    /// explosion by a number count in (<see cref="FirefleaTouchDeathExplosion"/>, <see cref="KraidNailTouchDeathExplosion"/>,
    /// <see cref="PowampBurstExplosion"/>).
    /// </summary>
    internal static readonly IReadOnlyList<EnemyDeathExplosion> AuthoredOrder =
    [
        EnemyDeathExplosion.Small,
        EnemyDeathExplosion.SamusContact,
        EnemyDeathExplosion.Normal,
        EnemyDeathExplosion.MiniKraid,
        EnemyDeathExplosion.Big,
    ];
}
