namespace SuperMetroid.Core.Enemies;

/// <summary>
/// An entity whose reactions and AI-state handlers are the shared ones (#1312). A species entity derives from it and
/// implements its own participations; an override replaces a shared answer where the species owns it. It takes only the
/// common collision paths until the species overrides <see cref="Capabilities"/>, and it owns no private shot callback
/// unless the species implements <see cref="IEnemyPrivateShotCallback"/>.
/// </summary>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation the system composes.</param>
internal abstract class CommonParticipationEnemyEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : IEnemyHurtAi, IEnemyFrozenAi, IEnemyTouchReaction, IEnemyShotReaction, IEnemyBombReaction, IEnemyPowerBombReaction
{
    /// <summary>The slot the enemy occupies; the shared predicates read its live state.</summary>
    protected RoomEnemySlot Slot { get; } = slot;

    /// <summary>The shared participation.</summary>
    protected EnemyCommonParticipation Common { get; } = common;

    /// <inheritdoc/>
    public virtual EnemyCapabilities Capabilities => EnemyCapabilities.None;

    /// <inheritdoc/>
    public virtual EnemyHurtAiHandler? FindHurt() => Common.FindHurt(Slot);

    /// <inheritdoc/>
    public virtual EnemyFrozenPreemption? FindFrozenPreemption() => Common.FindFrozenPreemption(Slot);

    /// <inheritdoc/>
    public virtual IReadOnlyList<Action<RoomEnemySlot>> FrozenHandlers() => Common.FrozenHandlers(Slot);

    /// <inheritdoc/>
    public virtual EnemyTouchHandler? FindTouch() => Common.FindTouch(Slot);

    /// <inheritdoc/>
    public virtual EnemyShotReaction? FindShot() => Common.FindShot(Slot);

    /// <inheritdoc/>
    public virtual Action<EnemyBombContext>? FindBombSpecial(EnemyBombContext context) => Common.FindBombSpecial(context);

    /// <inheritdoc/>
    public virtual Action<EnemyBombContext>? FindBombSpecialAfterNoOp(EnemyBombContext context) =>
        Common.FindBombSpecialAfterNoOp(context);

    /// <inheritdoc/>
    public virtual EnemyBombReaction? FindBombCommon(EnemyBombContext context) => Common.FindBombCommon(context);

    /// <inheritdoc/>
    public virtual EnemyPowerBombReaction? FindPowerBomb() => Common.FindPowerBomb(Slot);
}
