namespace SuperMetroid.Core.Game;

/// <summary>One exploding normal bomb overlapping one enemy in the ordinary bomb pass.</summary>
/// <param name="Enemy">The enemy whose selected shot callback runs.</param>
/// <param name="Bomb">The exploding bomb slot, already marked with direction bit $10.</param>
/// <param name="OrdinaryProjectiles">Samus's beam/missile slots (callbacks read slot zero's aim).</param>
/// <param name="Samus">Samus, when the pass has one.</param>
/// <param name="SelectedShotAi">The named callback of the selected hitbox (the header's on the radius path), if named.</param>
/// <param name="SelectedShotAiPointer">The raw bank-local callback word of the selected hitbox.</param>
/// <param name="UsesExtendedHitboxes">Whether the overlap came from the enemy's authored extended hitboxes.</param>
/// <param name="HealthBefore">Enemy health before common bomb damage (set for the common stages).</param>
internal readonly record struct EnemyBombContext(
    RoomEnemySlot Enemy,
    SamusBombProjectileSlot Bomb,
    SamusProjectileSystem OrdinaryProjectiles,
    SamusState? Samus,
    EnemyInteractionCallback? SelectedShotAi,
    ushort SelectedShotAiPointer,
    bool UsesExtendedHitboxes,
    ushort HealthBefore = 0);

/// <summary>
/// A species reaction to a normal bomb that enters common bomb damage. The bomb pass runs its stages around the
/// common damage at the cartridge's positions; a reaction sets only the stages it has.
/// </summary>
/// <param name="matches">Whether the overlap uses this reaction (definition and selected callback).</param>
internal sealed class EnemyBombReaction(Func<EnemyBombContext, bool> matches)
{
    /// <summary>Whether the overlap uses this reaction.</summary>
    internal Func<EnemyBombContext, bool> Matches { get; } = Ensure.NotNull(matches);

    /// <summary>Whether the private callback returns before common damage (the bomb stays marked).</summary>
    internal Func<EnemyBombContext, bool>? Rejects { get; init; }

    /// <summary>Whether common damage runs the generic death animation on a lethal hit.</summary>
    internal bool RunsGenericDeath { get; init; } = true;

    /// <summary>Runs immediately before common damage.</summary>
    internal Action<EnemyBombContext>? BeforeCommon { get; init; }

    /// <summary>The species tail after common damage.</summary>
    internal Action<EnemyBombContext>? AfterCommon { get; init; }
}
