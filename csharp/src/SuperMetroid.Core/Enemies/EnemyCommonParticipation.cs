namespace SuperMetroid.Core.Enemies;

/// <summary>
/// What the enemy system shares across species: the shared reaction lists. Entities compose it for every participation
/// they do not own, keeping the shared lists' first-match precedence.
/// </summary>
/// <param name="reactions">The enemy system's shared reactions, resolved at use because the system builds them on first access.</param>
internal sealed class EnemyCommonParticipation(Func<EnemySharedReactions> reactions)
{
    /// <summary>The shared reaction lists.</summary>
    private EnemySharedReactions Reactions => reactions();

    /// <summary>The first hurt handler matching <paramref name="slot"/>, or null.</summary>
    internal EnemyHurtHandler? FindHurt(RoomEnemySlot slot) => Reactions.FindHurt(slot);

    /// <summary>The first frozen preemption matching <paramref name="slot"/>, or null.</summary>
    internal EnemyFrozenPreemption? FindFrozenPreemption(RoomEnemySlot slot) => Reactions.FindFrozenPreemption(slot);

    /// <summary>Every frozen handler matching <paramref name="slot"/>, in order.</summary>
    internal IReadOnlyList<Action<RoomEnemySlot>> FrozenHandlers(RoomEnemySlot slot) => Reactions.FrozenHandlers(slot);

    /// <summary>The first touch handler matching <paramref name="slot"/>, or null.</summary>
    internal EnemyTouchHandler? FindTouch(RoomEnemySlot slot) => Reactions.FindTouch(slot);

    /// <summary>The first shot reaction matching <paramref name="slot"/>, or null.</summary>
    internal EnemyShotReaction? FindShot(RoomEnemySlot slot) => Reactions.FindShot(slot);

    /// <summary>The first special bomb handler matching <paramref name="context"/>, or null.</summary>
    internal Action<EnemyBombContext>? FindBombSpecial(EnemyBombContext context) => Reactions.FindBombSpecial(context);

    /// <summary>The first after-no-op bomb handler matching <paramref name="context"/>, or null.</summary>
    internal Action<EnemyBombContext>? FindBombSpecialAfterNoOp(EnemyBombContext context) => Reactions.FindBombSpecialAfterNoOp(context);

    /// <summary>The first common bomb reaction matching <paramref name="context"/>, or null.</summary>
    internal EnemyBombReaction? FindBombCommon(EnemyBombContext context) => Reactions.FindBombCommon(context);

    /// <summary>The first power-bomb reaction matching <paramref name="slot"/>, or null.</summary>
    internal EnemyPowerBombReaction? FindPowerBomb(RoomEnemySlot slot) => Reactions.FindPowerBomb(slot);
}
