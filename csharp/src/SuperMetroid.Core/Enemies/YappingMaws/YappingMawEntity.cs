namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>One YappingMaw occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The YappingMaw species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class YappingMawEntity(YappingMawBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunYappingMawInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunYappingMawMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceYappingMawTypedAnimation;

    /// <summary>The Yapping Maw's own shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        callback == EnemyInteractionCallback.YappingMawShot;
}
