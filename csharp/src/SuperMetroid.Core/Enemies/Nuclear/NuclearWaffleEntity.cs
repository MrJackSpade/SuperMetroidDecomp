namespace SuperMetroid.Core.Enemies.Nuclear;

/// <summary>One NuclearWaffle occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The NuclearWaffle species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NuclearWaffleEntity(NuclearWaffleBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeNuclearWaffle;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunNuclearWaffleMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => NuclearWaffleBehavior.AdvanceNuclearWaffleTypedAnimation;
}
