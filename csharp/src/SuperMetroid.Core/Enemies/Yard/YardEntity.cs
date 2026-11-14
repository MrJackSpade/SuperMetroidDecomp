namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One Yard occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Yard species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class YardEntity(YardEnemyBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => YardEnemyBehavior.InitializeYard(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunYardMainAi(Slot, frame);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => species.AdvanceYardTypedAnimation;

    /// <summary>The Yard's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.YardShot;
}
