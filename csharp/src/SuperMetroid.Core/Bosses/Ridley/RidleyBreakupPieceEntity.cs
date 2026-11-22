using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// A piece of Lower Norfair Ridley's breakup explosion (#2146): it flies apart by its own AI and animation, answers
/// Ridley's hitboxes, and takes the shared reactions its definition names. It joins the breakup that spawned it as it is
/// built, and holds the state its setup builds from it (#2554).
/// </summary>
/// <param name="death">Lower Norfair Ridley's death, which spawns the breakup.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the piece occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">The part of Ridley the piece is.</param>
/// <param name="breakup">The breakup the piece joins.</param>
internal sealed partial class RidleyBreakupPieceEntity(NorfairRidleyDeath death, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    RidleyExplosionSpawnSettings settings, RidleyBreakupSource breakup)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation, IEnemyFrameHitboxes
{
    /// <summary>The piece's flight, which its setup builds from the breakup it joins.</summary>
    internal RidleyExplosionFragmentState Fragment { get; } = death.InitializeNorfairRidleyExplosion(settings, place, breakup);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => death.RunNorfairRidleyExplosionMain(Occupant, Fragment, Ensure.NotNull(frame).Samus);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => RidleyBodyAnimation.AdvanceRidleyBreakupAnimation(Place, Fragment);

    /// <summary>None: the game defines no hitboxes for a breakup piece's frames (#2335).</summary>
    /// <param name="probe">The colliding edges and walker.</param>
    public EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe)
    {
        throw EnemyCollision.UncoveredFrame(Place);
    }
}
