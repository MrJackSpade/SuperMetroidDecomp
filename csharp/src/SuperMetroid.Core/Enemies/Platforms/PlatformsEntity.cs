namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>One Platforms occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Platforms species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PlatformsEntity(PlatformsBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's PlatformEnemyState, held by its physical slot.</summary>
    internal PlatformEnemyState? State
    {
        get => Slot.SpeciesState<PlatformEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunPlatformsInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunPlatformsMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => PlatformsBehavior.AdvancePlatformsTypedAnimation;
}
