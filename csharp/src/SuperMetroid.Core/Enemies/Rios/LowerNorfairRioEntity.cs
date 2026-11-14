namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>One Lower Norfair Rio occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Lower Norfair Rio species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class LowerNorfairRioEntity(LowerNorfairRioBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeLowerNorfairRio(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunLowerNorfairRioMainAi(Slot, frame);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => LowerNorfairRioBehavior.AdvanceLowerNorfairRioTypedAnimation;
}
