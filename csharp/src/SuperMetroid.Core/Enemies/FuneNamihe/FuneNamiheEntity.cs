namespace SuperMetroid.Core.Enemies.FuneNamihe;

/// <summary>One FuneNamihe occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The FuneNamihe species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class FuneNamiheEntity(FuneNamiheBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's FuneNamiheEnemyState, held by its physical slot.</summary>
    internal FuneNamiheEnemyState? State
    {
        get => Slot.SpeciesState<FuneNamiheEnemyState>();
        set => Slot.SetSpeciesState(value);
    }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => FuneNamiheBehavior.RunFuneNamiheInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => FuneNamiheBehavior.RunFuneNamiheMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceFuneNamiheTypedAnimation;
}
