namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>One Skree occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Skree species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SkreeEntity(SkreeBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's SkreeEnemyState, held by its physical slot.</summary>
    internal SkreeEnemyState? State
    {
        get => Slot.SpeciesState<SkreeEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => SkreeBehavior.RunSkreeInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunSkreeMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => SkreeBehavior.AdvanceSkreeTypedAnimation;
}
