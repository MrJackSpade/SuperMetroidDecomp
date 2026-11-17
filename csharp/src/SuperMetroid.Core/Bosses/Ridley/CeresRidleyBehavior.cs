using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ceres Ridley's encounter (#2229): his reveal's Ceres timings, his flight and attacks, the Baby he carries, the shots
/// that count toward his hundred hits, and his handoff to the station's escape. His body runs the parts both Ridleys share
/// (<see cref="RidleyBody"/>); none runs through Lower Norfair Ridley's encounter.
/// </summary>
[RoomLifetime]
internal sealed class CeresRidleyBehavior : IRidleyRevealEncounter<CeresRidleyState>
{
    /// <summary>The room's view of Ridley and the station's escape progress, which gates his movement and the Baby.</summary>
    private readonly RidleyEncounter _ridleyEncounter;

    /// <summary>The station's escape, which his getaway begins and whose step his AI runs once he has got away.</summary>
    private readonly CeresEscape _escape;

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
        _escape = dependencies.Escape;
        Body = dependencies.RidleyBody;
        Flight = new CeresRidleyFlight(new CeresRidleyFlightDependencies { Lifecycle = dependencies.Lifecycle, Room = dependencies.Room, Audio = dependencies.Audio, Places = dependencies.Places, RidleyEncounter = _ridleyEncounter, Presentation = dependencies.Presentation });
        BabyScene = new CeresRidleyBabyScene(dependencies.Room, dependencies.Earthquake, dependencies.Audio, dependencies.Presentation, _ridleyEncounter);
        Hits = new CeresRidleyHits(_ridleyEncounter);
    }

    /// <summary>Ceres Ridley's wait once Samus's door has finished, its first update the one the door finishes on.</summary>
    public int InitialDelayUpdates => CeresRidleyFightDefinitions.RevealDelayUpdates;

    /// <summary>Ceres Ridley's hold after his roar, on whose last update he takes off.</summary>
    public int RoarHoldUpdates => CeresRidleyFightDefinitions.RoarHoldUpdates;

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

        // The colour selection runs after the boss step and before movement. None of the
        // step branches mutate the flash timer, so selecting the two
        // draw colours here is equivalent while keeping their decision ahead of the
        // shared movement/composition continuation below.
        if (state.MovementAnimationEnabled && _ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            RidleyBodyContact.UpdateHurtFlash(place, state, place.FrameCounter);

        if (!state.LiftedOff)
            Body.Reveal.Run(place, state, this);
        else if (!RunCeresRidleyPhase(place, state, samus))
            return;

        // Once the eye fade enables composite animation, Ceres Ridley's main behaviour performs the
        // common Ridley 1/256-pixel movement pass after every update. Keeping that order
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
        switch (state.Phase)
        {
            case CeresRidleyPhase.LiftoffAccelerating:
                // The rising step adds -16 to the 1/256-pixel Y velocity before the common movement pass. Ridley rises from rest
                // until he passes line 112 of his chamber, so the speed stays a few pixels an update.
                state.Flight.Velocity = state.Flight.Velocity.WithY(state.Flight.Velocity.Y - CeresRidleyFlightDefinitions.LiftoffClimbStep);
                if (RidleyFlight.SignedPlace(place).Y < CeresRidleyArenaDefinitions.LiftoffSlowingLineY)
                {
                    state.Phase = CeresRidleyPhase.LiftoffDecelerating;
                    CeresRidleyFlight.TickCeresRidleyLiftoffDecelerating(place, state);
                }
                return true;

            case CeresRidleyPhase.LiftoffDecelerating:
                CeresRidleyFlight.TickCeresRidleyLiftoffDecelerating(place, state);
                return true;

            case CeresRidleyPhase.Hovering:
                Flight.TickCeresRidleyHover(place, state, samus);
                return true;

            case CeresRidleyPhase.FireballMoveToPosition:
                Flight.TickCeresRidleyFireballMove(place, state);
                return true;

            case CeresRidleyPhase.FireballShooting:
                Flight.TickCeresRidleyFireballShooting(place, state);
                return true;

            case CeresRidleyPhase.LungeSetup:
                RidleyFlight.StartAnimation(state.Flight, place, RidleyAnimations.CeresLunge);
                state.Phase = CeresRidleyPhase.LungeMain;
                state.PhaseTimer.LastFor(CeresRidleyFlightDefinitions.LungeUpdates);
                CeresRidleyFlight.TickCeresRidleyLunge(place, state, samus);
                return true;

            case CeresRidleyPhase.LungeMain:
                CeresRidleyFlight.TickCeresRidleyLunge(place, state, samus);
                return true;

            case CeresRidleyPhase.SwoopSetup:
                state.Phase = CeresRidleyPhase.SwoopMoveToPosition;
                state.PhaseTimer.LastFor(CeresRidleyFlightDefinitions.SwoopDiveUpdates);
                state.Flight.SwoopAngleAccumulator = RotationAngle.Zero;
                state.Tail.IdleWhipEnabled = false;
                CeresRidleyFlight.TickCeresRidleySwoopMoveToPosition(place, state);
                return true;

            case CeresRidleyPhase.SwoopMoveToPosition:
                CeresRidleyFlight.TickCeresRidleySwoopMoveToPosition(place, state);
                return true;

            case CeresRidleyPhase.SwoopDescendingAimingDown:
                CeresRidleyFlight.TickCeresRidleySwoopPhase(
                    state,
                    turn: CeresRidleyFlightDefinitions.SwoopDiveTurn,
                    targetAngle: CeresRidleyFlightDefinitions.SwoopDiveAim,
                    targetSpeed: CeresRidleyFlightDefinitions.SwoopSpeed,
                    nextPhase: CeresRidleyPhase.SwoopDescendingAimingLeft,
                    nextUpdates: CeresRidleyFlightDefinitions.SwoopSweepUpdates);
                return true;

            case CeresRidleyPhase.SwoopDescendingAimingLeft:
                CeresRidleyFlight.TickCeresRidleySwoopPhase(
                    state,
                    turn: CeresRidleyFlightDefinitions.SwoopSweepTurn,
                    targetAngle: CeresRidleyFlightDefinitions.SwoopSweepAim,
                    targetSpeed: CeresRidleyFlightDefinitions.SwoopSpeed,
                    nextPhase: CeresRidleyPhase.SwoopAscendingAimingUp,
                    nextUpdates: CeresRidleyFlightDefinitions.SwoopClimbUpdates);
                if (state.Phase == CeresRidleyPhase.SwoopAscendingAimingUp)
                {
                    // The swoop requests one aimed tail fling at the exact bottom
                    // of the swoop. Neutral tail AI consumes and clears it later this same
                    // enemy-main pass; treating it as a persistent animation mode makes the
                    // tail lag an entire attack behind Ridley.
                    state.Tail.WhipRequested = true;
                }
                return true;

            case CeresRidleyPhase.SwoopAscendingAimingUp:
                CeresRidleyFlight.TickCeresRidleySwoopPhase(
                    state,
                    turn: CeresRidleyFlightDefinitions.SwoopClimbTurn,
                    targetAngle: RotationAngle.Zero - RidleyFlight.SwoopClimbTarget,
                    targetSpeed: CeresRidleyFlightDefinitions.SwoopSpeed,
                    nextPhase: CeresRidleyPhase.SwoopAscendingAimingUpFaster,
                    nextUpdates: CeresRidleyFlightDefinitions.SwoopFinalClimbUpdates);
                return true;

            case CeresRidleyPhase.SwoopAscendingAimingUpFaster:
                RidleyFlight.UpdateSwoopVelocity(state.Flight, turn: CeresRidleyFlightDefinitions.SwoopFinalClimbTurn, targetAngle: RotationAngle.Zero - RidleyFlight.SwoopClimbTarget,
                    targetSpeed: CeresRidleyFlightDefinitions.SwoopSpeed);
                if (state.PhaseTimer.StepRunsOut())
                {
                    state.Phase = CeresRidleyPhase.Hovering;
                    state.HoverCounter = 0;
                    state.Tail.IdleWhipEnabled = true;
                }
                return true;

            case CeresRidleyPhase.FakeRetreatMoveToPosition:
                CeresRidleyFlight.TickCeresRidleyFakeRetreatMoveToPosition(place, state);
                return true;

            case CeresRidleyPhase.FakeRetreatRising:
                CeresRidleyFlight.TickCeresRidleyFakeRetreatRising(place, state);
                return true;

            case CeresRidleyPhase.WaitBeforeRetrievingBaby:
                if (state.PhaseTimer.StepRunsOut())
                {
                    RidleyFlight.StartAnimation(state.Flight, place, RidleyAnimations.RetrieveBabyMetroid);
                    state.Phase = CeresRidleyPhase.RetrieveBaby;
                    CeresRidleyFlight.TickCeresRidleyRetrieveBaby(place, state);
                }
                return true;

            case CeresRidleyPhase.RetrieveBaby:
                CeresRidleyFlight.TickCeresRidleyRetrieveBaby(place, state);
                return true;

            case CeresRidleyPhase.RealRetreatRising:
                Flight.TickCeresRidleyRetreat(place, state);
                return true;

            case CeresRidleyPhase.RetreatDelay:
                Flight.TickCeresRidleyRetreatDelay(state);
                return true;

            case CeresRidleyPhase.PublishEscapeHandoff:
                // The getaway handoff leaves Ridley with nothing to do and publishes his escape. The turning floor's
                // view shows his departure, so his body is hidden.
                state.Phase = CeresRidleyPhase.Escaped;
                place.Presence = place.Presence.With(EnemyPresence.Hidden);
                _escape.BeginGetaway();
                return true;

            case CeresRidleyPhase.Escaped:
                // The escape runs its own step here, in his place in the update: nothing while the room main flies the
                // getaway, then the self-destruct warning and its alarm.
                _escape.RunRidleysStep(place.FrameCounter);
                return false;

            default:
                throw new InvalidDataException($"Undefined {nameof(CeresRidleyPhase)} {state.Phase}.");
        }
    }
}
