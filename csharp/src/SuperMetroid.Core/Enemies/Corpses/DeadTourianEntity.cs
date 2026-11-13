namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One Tourian corpse occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Tourian corpses' species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadTourianEntity(DeadTourianBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction,
        IShowsCorpseRotRows
{
    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadTourianBehavior.ShownDeadTourianRows(Slot);

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => DeadTourianBehavior.InitializeDeadTourianCorpse;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunDeadTourianMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => DeadTourianBehavior.AdvanceDeadTourianTypedAnimation;

    /// <summary>The corpse's rot trigger, when its definition names it as the shot reaction.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        DeadTourianBehavior.HasDeadTourianCorpseShotReaction(Slot) && reaction == EnemyCollision.ShotAi(Slot);
}
