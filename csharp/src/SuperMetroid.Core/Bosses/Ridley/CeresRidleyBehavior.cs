using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley's encounter (#2229): his reveal's Ceres timings, his flight and attacks, the Baby he carries, the shots
/// that count toward the hits that drive him off, and his handoff to the station's escape. His body runs the parts both Ridleys share
/// (<see cref="RidleyBody"/>); none runs through Lower Norfair Ridley's encounter.
/// </summary>
internal sealed partial class CeresRidleyBehavior : IRidleyRevealEncounter<CeresRidleyState>
{
    /// <summary>The room's view of Ridley and the station's escape progress, which gates his movement and the Baby.</summary>
    private readonly RidleyEncounter _ridleyEncounter;

    /// <summary>The station's escape, which his getaway begins and whose step his AI runs once he has got away.</summary>
    internal CeresEscape Escape { get; }

    /// <summary>The parts of his body both Ridleys run.</summary>
    internal RidleyBody Body { get; }

    /// <summary>Ceres Ridley's flight: liftoff, hovering, fireballs, lunges, swoops, the fake retreat and taking the baby.</summary>
    internal CeresRidleyFlight Flight { get; }

    /// <summary>The Baby Metroid's container in Ceres Ridley's room, and the Baby and door drawn around the fight.</summary>
    internal CeresRidleyBabyScene BabyScene { get; }

    /// <summary>What Samus's weapons do to Ceres Ridley.</summary>
    internal CeresRidleyHits Hits { get; }

    /// <summary>Connects Ceres Ridley's enemy AI, drawing, collision and room-main collaborators.</summary>
    internal CeresRidleyBehavior(CeresRidleyBehaviorDependencies dependencies)
    {
        _ridleyEncounter = dependencies.RidleyEncounter;
        Escape = dependencies.Escape;
        Body = dependencies.RidleyBody;
        Flight = new CeresRidleyFlight(new CeresRidleyFlightDependencies { Lifecycle = dependencies.Lifecycle, Room = dependencies.Room, Audio = dependencies.Audio, Places = dependencies.Places, RidleyEncounter = _ridleyEncounter, Presentation = dependencies.Presentation, GetawayWalls = dependencies.GetawayWalls });
        BabyScene = new CeresRidleyBabyScene(dependencies.Room, dependencies.Audio, dependencies.Presentation, _ridleyEncounter);
        Hits = new CeresRidleyHits(_ridleyEncounter);
    }

    /// <summary>Ceres Ridley's wait once Samus's door has finished, its first update the one the door finishes on.</summary>
    public int InitialDelayUpdates => CeresRidleyFightDefinitions.RevealDelayUpdates;

    /// <summary>Ceres Ridley stands in front of Samus once his body has faded in; he stood behind her while he was unseen.</summary>
    public EnemyStacking RevealedStacking => EnemyStacking.InFrontOfSamus;

    /// <summary>Begins Ceres Ridley's hold after his roar, on whose last update he takes off.</summary>
    /// <param name="ridley">His encounter.</param>
    public void BeginRoarHold(CeresRidleyState ridley) => Ensure.NotNull(ridley).PhaseTimer.LastFor(CeresRidleyFightDefinitions.RoarHoldUpdates);

    /// <summary>Steps his roar's hold; he takes off as it runs out.</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="ridley">His encounter.</param>
    public bool TakesOff(RoomEnemyPlace place, CeresRidleyState ridley) => Ensure.NotNull(ridley).PhaseTimer.StepRunsOut();

    /// <summary>
    /// Runs the Ceres step reached through the end of the battle. The
    /// reveal, liftoff, hover, two authored termination conditions, upward retreat,
    /// colour handoff, and <see cref="CeresEscapeStatus.RidleyEscaped"/> publication all keep the game's order.
    /// Ridley's later rotating getaway presentation remains a room-main concern, not enemy AI.
    /// </summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="state">His encounter, which his body holds.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="samusProjectiles">Samus's beams and missiles, a missile among which the tail may whip at.</param>
    internal void RunCeresRidleyMain(
        RoomEnemyPlace place,
        CeresRidleyState state,
        SamusState samus,
        SamusProjectileSystem samusProjectiles)
    {
        place.Health = BossHealthDefinitions.RefilledFullHealth;

        // His hurt flash picks his two draw colours after his step and before he moves; no step changes the flash.
        if (state.MovementAnimationEnabled && _ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            RidleyBodyContact.UpdateHurtFlash(place, state, place.FrameCounter);

        if (!state.LiftedOff)
            Body.Reveal.Run(place, state, this);
        else if (!RunCeresRidleyPhase(place, state, samus))
            return;

        // Once the eye fade enables composite animation, Ceres Ridley's main behaviour performs the
        // common Ridley movement pass after every update. Keeping that order
        // matters in the rise and swoop: velocity changes affect position in the very same frame.
        if (state.MovementAnimationEnabled && _ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
        {
            Body.Flight.IntegrateMovement(place, state.Flight, wallImpact: RidleyWallImpact.CeresWall);
            RidleyFlight.TickWingAnimation(state.Flight);
            Body.Tail.RandomlyUpdateCeresRidleyTailCurliness(state.Tail);
            Body.Tail.TickRidleyTail(place, state, samus, samusProjectiles);
            CeresRidleyHits.UpdateCeresRidleyHealthBand(state);
        }
        if (_ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            CeresRidleyBabyScene.TickCeresBaby(place, state.Baby);
    }

    /// <summary>
    /// Runs the Ceres phase Ridley is in once he has lifted off, returning whether his movement and the Baby follow it:
    /// once he has got away the escape leaves both to the room.
    /// </summary>
    private bool RunCeresRidleyPhase(RoomEnemyPlace place, CeresRidleyState state, SamusState samus)
    {
        CeresRidleyPhase phase = state.Phase;
        phase.Run(this, place, state, samus);
        return !phase.HandsTheRoomToTheEscape;
    }
}
