namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>One Choot occupant: its own AI, with the shared reactions.</summary>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ChootEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => ChootBehavior.InitializeChoot(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => ChootBehavior.RunChootMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => ChootBehavior.AdvanceChootTypedAnimation(Slot, context);
}
