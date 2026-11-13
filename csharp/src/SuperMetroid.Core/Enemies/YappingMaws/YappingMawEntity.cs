namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One YappingMaw occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The YappingMaw species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class YappingMawEntity(YappingMawBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeYappingMaw;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunYappingMawMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => species.AdvanceYappingMawTypedAnimation;

    /// <summary>The Yapping Maw's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.YappingMawShot;
}
