namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>One Platforms occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Platforms species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PlatformsEntity(PlatformsBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => PlatformsBehavior.InitializePlatform;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunPlatformsMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => PlatformsBehavior.AdvancePlatformsTypedAnimation;

    /// <summary>The Tripper's own shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        Slot.DefinitionId == EnemyDefinitionId.Tripper && callback == EnemyInteractionCallback.TripperShot;
}
