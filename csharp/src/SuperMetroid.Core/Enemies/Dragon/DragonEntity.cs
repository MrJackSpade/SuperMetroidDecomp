namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>One Dragon occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Dragon species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DragonEntity(DragonBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeDragon;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunDragonMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => DragonBehavior.AdvanceDragonTypedAnimation;

    /// <summary>The Dragon's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == EnemyReaction.DragonShot;
}
