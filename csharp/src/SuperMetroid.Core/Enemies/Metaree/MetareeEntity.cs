namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>One Metaree occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Metaree species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MetareeEntity(MetareeBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => MetareeBehavior.RunMetareeInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunMetareeMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => MetareeBehavior.AdvanceMetareeTypedAnimation;

    /// <summary>The Metaree's own shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        callback == EnemyInteractionCallback.MetareeShot;
}
