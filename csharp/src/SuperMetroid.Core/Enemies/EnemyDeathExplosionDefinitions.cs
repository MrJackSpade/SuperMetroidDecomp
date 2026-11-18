using SuperMetroid.Core.Enemies.Deaths;

namespace SuperMetroid.Core.Enemies;

/// <summary>The typed death animations shared by generic and family-specific enemy deaths.</summary>
internal static class EnemyDeathExplosionDefinitions
{
    /// <summary>The blank-frame wait a drop runs after it is collected or expires.</summary>
    internal const EnemyDeathAndDropSegment NoDropTail = EnemyDeathAndDropSegment.RespawnTail;

    /// <summary>
    /// The wide spread a Fake Kraid death explosion's sprites land in, up to 32 pixels from it each way: 0-63 pixels right
    /// of and below the 32 they are centred back by, each way a choice of the random number.
    /// </summary>
    internal static readonly EnemyDeathSpriteSpread WideSpriteSpread = new(HorizontalScatter: new(0, 6), VerticalScatter: new(8, 6), Radius: 32);

    /// <summary>
    /// The narrow spread a big death explosion's sprites land in, up to 16 pixels from it each way: 0-31 pixels right of
    /// and below the 16 they are centred back by, each way a choice of the random number.
    /// </summary>
    internal static readonly EnemyDeathSpriteSpread NarrowSpriteSpread = new(HorizontalScatter: new(0, 5), VerticalScatter: new(8, 5), Radius: 16);

    /// <summary>
    /// The explosion a death that takes it from a leftover number dies in (<see cref="LeftoverNumberDeathExplosion{TExplosion}"/>),
    /// given the enemies' five explosions by name.
    /// </summary>
    internal static readonly LeftoverNumberDeathExplosion<EnemyDeathExplosion> LeftoverNumbered = new(
        small: EnemyDeathExplosion.Small,
        samusContact: EnemyDeathExplosion.SamusContact,
        normal: EnemyDeathExplosion.Normal,
        fakeKraid: EnemyDeathExplosion.FakeKraid,
        big: EnemyDeathExplosion.Big);
}
