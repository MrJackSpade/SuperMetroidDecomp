namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>One Wrecked Ship ghost occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WreckedShipGhostEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => WreckedShipGhostBehavior.InitializeWreckedShipGhost(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => WreckedShipGhostBehavior.RunWreckedShipGhostMainAi(Slot, frame);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => WreckedShipGhostBehavior.AdvanceWreckedShipGhostTypedAnimation;
}
