namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One DeadTorizo occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The DeadTorizo species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadTorizoEntity(DeadTorizoBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction,
        IShowsCorpseRotRows
{
    /// <inheritdoc/>
    public CorpseRotRows ShownRotRows => species.ShownDeadTorizoRows(Slot);

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeDeadTorizo;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunDeadTorizoMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceDeadTorizoTypedAnimation;

    /// <summary>The dead Torizo's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.DeadTorizoTouchAndShot;
}
