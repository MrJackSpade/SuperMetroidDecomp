namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>One Alcoon occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Alcoon species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class AlcoonEntity(AlcoonBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunAlcoonInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunAlcoonMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceAlcoonTypedAnimation;
}
