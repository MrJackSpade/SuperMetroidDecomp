namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>One Flies occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Flies species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class FliesEntity(FliesBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => FliesBehavior.InitializeFly(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunFliesMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => FliesBehavior.AdvanceFliesTypedAnimation(Slot, context);
}
