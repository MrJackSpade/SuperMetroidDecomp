namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>One Wrecked Ship ghost occupant: its own AI, with the shared reactions.</summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WreckedShipGhostEntity(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly WreckedShipGhostEnemyState state = WreckedShipGhostBehavior.InitializeWreckedShipGhost(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => WreckedShipGhostBehavior.RunWreckedShipGhostMainAi(Place, state, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => WreckedShipGhostBehavior.AdvanceWreckedShipGhostTypedAnimation(Place, state);
}
