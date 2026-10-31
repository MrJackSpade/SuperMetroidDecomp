namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One DeadTourian occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The DeadTourian species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadTourianEntity(DeadTourianBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction,
        IShowsCorpseRot
{
    /// <inheritdoc/>
    public CorpseRotView ShownRot => species.ShownDeadTourianRot(Slot);

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeDeadTourianCorpse;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunDeadTourianMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => DeadTourianBehavior.AdvanceDeadTourianTypedAnimation;

    /// <summary>The corpse's rot trigger, when its header names it as the shot reaction.</summary>
    /// <param name="reaction">The reaction the header or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        DeadTourianBehavior.HasDeadTourianCorpseShotCallback(Slot) && reaction == EnemyCollision.ShotAi(Slot);
}
