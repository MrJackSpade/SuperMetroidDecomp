namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>One WreckedShipGhost occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The WreckedShipGhost species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WreckedShipGhostEntity(WreckedShipGhostBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunWreckedShipGhostInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunWreckedShipGhostMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => WreckedShipGhostBehavior.AdvanceWreckedShipGhostTypedAnimation;
}
