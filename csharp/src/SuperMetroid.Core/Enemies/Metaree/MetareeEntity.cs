namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>One Metaree occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Metaree species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MetareeEntity(MetareeBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => MetareeBehavior.RunMetareeInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunMetareeMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => MetareeBehavior.AdvanceMetareeTypedAnimation;
}
