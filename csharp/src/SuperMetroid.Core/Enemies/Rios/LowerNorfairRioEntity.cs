namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One LowerNorfairRio occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The LowerNorfairRio species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class LowerNorfairRioEntity(LowerNorfairRioBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeLowerNorfairRio;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunLowerNorfairRioMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => LowerNorfairRioBehavior.AdvanceLowerNorfairRioTypedAnimation;
}
