using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>The typed death animations shared by generic and family-specific enemy deaths.</summary>
internal static class EnemyDeathExplosionDefinitions
{
    /// <summary>The blank-frame wait a drop runs after it is collected or expires.</summary>
    internal const EnemyDeathProjectileSegment NoDropTail = EnemyDeathProjectileSegment.RespawnTail;

    /// <summary>
    /// The wide spread a Mini-Kraid death explosion's sprites land in, up to 32 pixels from it each way: 0-63 pixels right
    /// of and below the 32 they are centred back by, the random number modulo 64 across and the random number divided by
    /// 256, modulo 64, down.
    /// </summary>
    internal static readonly EnemyDeathSpriteSpread WideSpriteSpread = new(HorizontalScatter: new(0, 6), VerticalScatter: new(8, 6), Radius: 32);

    /// <summary>
    /// The narrow spread a big death explosion's sprites land in, up to 16 pixels from it each way: 0-31 pixels right of
    /// and below the 16 they are centred back by, the random number modulo 32 across and the random number divided by
    /// 256, modulo 32, down.
    /// </summary>
    internal static readonly EnemyDeathSpriteSpread NarrowSpriteSpread = new(HorizontalScatter: new(0, 5), VerticalScatter: new(8, 5), Radius: 16);

    /// <summary>
    /// The death explosions in their authored order, the small explosion first: the order the deaths that take their
    /// explosion from a leftover number count in (<see cref="LeftoverNumberDeathExplosion"/>).
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
