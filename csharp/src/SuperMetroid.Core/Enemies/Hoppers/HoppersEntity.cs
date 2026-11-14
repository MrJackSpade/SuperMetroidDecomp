namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>One Hoppers occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Hoppers species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class HoppersEntity(HoppersBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeHopper(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunHoppersMainAi(Slot, frame);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => species.AdvanceHoppersTypedAnimation;
}
