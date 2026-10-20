namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>One Skree occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Skree species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SkreeEntity(SkreeBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => SkreeBehavior.InitializeSkree;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunSkreeMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => SkreeBehavior.AdvanceSkreeTypedAnimation;

    /// <summary>The Skree's own shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        callback == EnemyInteractionCallback.SkreeShot;
}
