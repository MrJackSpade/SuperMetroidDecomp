namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>One Magdollite occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Magdollite species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MagdolliteEntity(MagdolliteBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's MagdolliteEnemyState, held by its physical slot.</summary>
    internal MagdolliteEnemyState? State
    {
        get => Slot.SpeciesState<MagdolliteEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunMagdolliteInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunMagdolliteMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceMagdolliteTypedAnimation;
}
