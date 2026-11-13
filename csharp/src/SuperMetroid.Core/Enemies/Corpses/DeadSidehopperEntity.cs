namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One dead Sidehopper occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The dead Sidehopper species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadSidehopperEntity(DeadSidehopperBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction,
        IShowsCorpseRotRows
{
    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => DeadSidehopperBehavior.ShownDeadSidehopperRows(Slot);

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeDeadSidehopper;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunDeadSidehopperMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => DeadSidehopperBehavior.AdvanceDeadSidehopperTypedAnimation;

    /// <summary>The dead Sidehopper's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.DeadSidehopperShot;
}
