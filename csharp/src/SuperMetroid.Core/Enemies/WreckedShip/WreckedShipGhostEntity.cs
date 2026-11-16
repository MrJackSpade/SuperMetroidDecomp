namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>One Wrecked Ship ghost occupant: its own AI, with the shared reactions.</summary>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WreckedShipGhostEntity(RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => WreckedShipGhostBehavior.InitializeWreckedShipGhost(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => WreckedShipGhostBehavior.RunWreckedShipGhostMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => WreckedShipGhostBehavior.AdvanceWreckedShipGhostTypedAnimation(Place, context);
}
