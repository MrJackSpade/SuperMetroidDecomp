namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>One Atomic occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Atomic species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class AtomicEntity(AtomicBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's AtomicEnemyState, held by its physical slot.</summary>
    internal AtomicEnemyState? State
    {
        get => Slot.SpeciesState<AtomicEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunAtomicInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => AtomicBehavior.RunAtomicMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => AtomicBehavior.AdvanceAtomicTypedAnimation;
}
