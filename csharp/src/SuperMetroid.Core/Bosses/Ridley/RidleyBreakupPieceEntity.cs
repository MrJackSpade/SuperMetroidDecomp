using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// A piece of Lower Norfair Ridley's breakup explosion (#2146): it flies apart by its own AI and animation, answers
/// Ridley's hitboxes, and takes the shared reactions its definition names.
/// </summary>
/// <param name="ridley">The Lower Norfair Ridley owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the piece occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class RidleyBreakupPieceEntity(NorfairRidleyBehavior ridley, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes
{
    /// <inheritdoc/>
    public void Initialize() => ridley.Death.InitializeNorfairRidleyExplosion(Occupant, Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => ridley.Death.RunNorfairRidleyExplosionMain(Occupant, frame.Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => RidleyBodyAnimation.AdvanceRidleyBreakupAnimation(Place);

    /// <summary>None: the game defines no hitboxes for a breakup piece's frames (#2335).</summary>
    /// <param name="probe">The colliding edges and walker.</param>
    /// <param name="reaction">No reaction.</param>
    public EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe)
    {
        throw EnemyCollision.UncoveredFrame(Place);
    }
}
