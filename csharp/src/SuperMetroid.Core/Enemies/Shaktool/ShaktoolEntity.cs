namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>One Shaktool occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Shaktool species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ShaktoolEntity(ShaktoolBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeShaktool;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunShaktoolMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceShaktoolTypedAnimation;

    /// <summary>The Shaktool's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.ShaktoolShot;
}
