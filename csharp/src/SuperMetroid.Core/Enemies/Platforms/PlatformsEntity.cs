namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>One Platforms occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Platforms species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PlatformsEntity(PlatformsBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => PlatformsBehavior.InitializePlatform;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunPlatformsMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => PlatformsBehavior.AdvancePlatformsTypedAnimation;

    /// <summary>The Tripper's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        Slot.DefinitionId == EnemyDefinitionId.Tripper && reaction == EnemyReaction.TripperShot;
}
