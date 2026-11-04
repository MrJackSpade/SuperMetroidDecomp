namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>One WreckedShipGhost occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WreckedShipGhostEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => WreckedShipGhostBehavior.InitializeWreckedShipGhost;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => WreckedShipGhostBehavior.RunWreckedShipGhostMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => WreckedShipGhostBehavior.AdvanceWreckedShipGhostTypedAnimation;
}
