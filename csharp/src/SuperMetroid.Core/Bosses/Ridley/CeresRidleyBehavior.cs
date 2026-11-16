using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The Ceres Ridley species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class CeresRidleyBehavior
{
    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
    private readonly RidleyEncounter _ridleyEncounter;

    /// <summary>Advances Ridley's manually drawn tail segments.</summary>
    private readonly RidleyTail _ridleyTail;
    /// <summary>Publishes Ceres escape and self-destruct room status.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>Samus's passage through a door, which holds Ridley's opening wait.</summary>
    private readonly DoorPassage _doorPassage;
    /// <summary>Ceres Ridley's flight: liftoff, hovering, fireballs, lunges, swoops, the fake retreat and taking the baby, and the movement integration they share.</summary>
    internal CeresRidleyFlight Flight { get; }

    /// <summary>The Baby Metroid's container in Ceres Ridley's room, and the sprites and door drawn around the fight.</summary>
    internal CeresRidleyBabyScene BabyScene { get; }

    /// <summary>Ridley's fade-in, which Norfair Ridley's behaviour runs through this instance too.</summary>
    internal RidleyFadeIn FadeIn { get; }

    /// <summary>What Ceres Ridley's body does to Samus and what Samus's weapons do to Ceres Ridley.</summary>
    internal CeresRidleyHits Hits { get; }

    /// <summary>Ceres Ridley's turning-floor getaway and the station's self-destruct countdown that follows.</summary>
    internal CeresRidleyGetaway Getaway { get; }
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;




    /// <summary>Connects Ceres Ridley's enemy AI, drawing, collision and room-main collaborators.</summary>
    internal CeresRidleyBehavior(CeresRidleyBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.Presentation.Artwork;
        _ridleyTail = dependencies.RidleyTail;
        _ridleyEncounter = dependencies.RidleyEncounter;
        _roomStatus = dependencies.RoomStatus;
        _doorPassage = dependencies.DoorPassage;
        Flight = new CeresRidleyFlight(new CeresRidleyFlightDependencies { Lifecycle = dependencies.Lifecycle, Room = dependencies.Room, Earthquake = dependencies.Earthquake, Audio = dependencies.Audio, Places = dependencies.Places, RidleyEncounter = _ridleyEncounter });
        BabyScene = new CeresRidleyBabyScene(dependencies.Room, dependencies.Earthquake, dependencies.Audio, dependencies.Presentation.Surfaces, _ridleyEncounter);
        FadeIn = new RidleyFadeIn(dependencies.Audio);
        Hits = new CeresRidleyHits(dependencies.Collision, _ridleyEncounter);
        Getaway = new CeresRidleyGetaway(new CeresRidleyGetawayDependencies { FrameResults = dependencies.FrameResults, Room = dependencies.Room, Audio = dependencies.Audio, Presentation = dependencies.Presentation.Surfaces, RidleyEncounter = _ridleyEncounter, RoomStatus = _roomStatus, BossArtwork = _bossArtwork, UnauthoredBoostCadence = dependencies.UnauthoredBoostCadence });
    }

    /// <summary>
    /// Runs the Ceres step reached through the end of the battle. The
    /// reveal, liftoff, hover, two authored termination conditions, upward retreat,
    /// colour handoff, and <see cref="CeresEscapeStatus.RidleyEscaped"/> publication all keep the game's order.
    /// Ridley's later rotating getaway presentation remains a room-main concern, not enemy AI.
    /// </summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="samusProjectiles">Samus's beams and missiles, a missile among which the tail may whip at.</param>
    internal void RunCeresRidleyMain(
        RoomEnemyPlace place,
        SamusState samus,
        SamusProjectileSystem samusProjectiles)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireCeresRidley(place);
        place.Health = BossHealthDefinitions.RefilledFullHealth;

        // The colour selection runs after the boss step and before movement. None of the
        // step branches mutate the flash timer, so selecting the two
        // draw colours here is equivalent while keeping their decision ahead of the
        // shared movement/composition continuation below.
        if (state.MovementAnimationEnabled && _ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            CeresRidleyHits.UpdateRidleyHurtFlash(place, state, place.FrameCounter);

        if (!state.LiftedOff)
            RunCeresRidleyReveal(place, state);
        else if (!RunCeresRidleyPhase(place, state, samus))
            return;

        // Once the eye fade enables composite animation, Ceres Ridley's main behaviour performs the
        // common Ridley 1/256-pixel movement pass after every update. Keeping that order
        // matters in the rise and swoop: velocity changes affect position in the very same frame.
        if (state.MovementAnimationEnabled && _ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
        {
            Flight.IntegrateRidleyMovement(place, state, wallImpact: RidleyWallImpact.CeresWall);
            CeresRidleyFlight.TickRidleyWingAnimation(state);
            _ridleyTail.RandomlyUpdateCeresRidleyTailCurliness(state);
            _ridleyTail.TickRidleyTail(place, state, samus, samusProjectiles);
            CeresRidleyHits.UpdateCeresRidleyHealthBand(state);
        }
        if (_ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            CeresRidleyBabyScene.TickCeresBaby(place, state);
    }

    /// <summary>Runs the phase of Ceres Ridley's reveal he is in, before he lifts off.</summary>
    private void RunCeresRidleyReveal(RoomEnemyPlace place, RidleyEnemyState state)
    {
        switch (state.RevealPhase)
        {
            case RidleyRevealPhase.WaitForDoorTransition:
                // The wait holds even though enemy animations run during the fade.
                if (_doorPassage.IsUnderWay)
                    return;
                state.RevealPhase = RidleyRevealPhase.InitialDelay;
                state.PhaseTimer = 512;
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                return;

            case RidleyRevealPhase.InitialDelay:
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                return;

            case RidleyRevealPhase.FadeInEyes:
                RidleyFadeIn.TickEyeFadeIn(state);
                return;

            case RidleyRevealPhase.FadeInBody:
                FadeIn.TickBodyFadeIn(place, state);
                return;

            case RidleyRevealPhase.WaitBeforeRoar:
                if (--state.PhaseTimer < 0)
                {
                    CeresRidleyFlight.StartRidleyAnimation(state, place, RidleyAnimations.OpeningRoar);
                    state.PhaseTimer = 252;
                    state.RevealPhase = RidleyRevealPhase.WaitBeforeLiftoff;
                }
                return;

            case RidleyRevealPhase.WaitBeforeLiftoff:
                if (--state.PhaseTimer < 0)
                {
                    state.FadeStep = 0;
                    CeresRidleyFlight.StartRidleyAnimation(state, place, RidleyAnimations.TransitionToFlying);
                    // Ridley's roar before flying primes the wing
                    // timer, enables every tail segment, and selects neutral tail AI in
                    // the same transition that installs the flying animation.
                    state.WingAnimationTimer = 8;
                    state.WingAnimationTimerDelta = 8;
                    foreach (RidleyTailSegment segment in state.TailSegments)
                        segment.Active = true;
                    state.TailAction = RidleyTailAction.Neutral;
                    state.RevealPhase = RidleyRevealPhase.TakingOff;
                }
                return;

            case RidleyRevealPhase.TakingOff:
                state.Velocity = EightEightVector<RoomPixels>.Zero;
                return;

            default:
                throw new InvalidDataException($"Undefined {nameof(RidleyRevealPhase)} {state.RevealPhase}.");
        }
    }

    /// <summary>
    /// Runs the Ceres phase Ridley is in once he has lifted off, returning whether his movement and the Baby follow it:
    /// the getaway and the self-destruct leave both to the room.
    /// </summary>
    private bool RunCeresRidleyPhase(RoomEnemyPlace place, RidleyEnemyState state, SamusState samus)
    {
        switch (state.CeresPhase)
        {
            case CeresRidleyPhase.LiftoffAccelerating:
                // The rising step adds -16 to the 1/256-pixel Y velocity before the common movement pass. Ridley rises from rest
                // until he passes line 112 of his chamber, so the speed stays a few pixels an update.
                state.Velocity = state.Velocity.WithY(state.Velocity.Y - SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16));
                if (CoordinateAxis.Difference(place.Position.Y.Whole, 112) < 0)
                {
                    state.CeresPhase = CeresRidleyPhase.LiftoffDecelerating;
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
                CeresRidleyFlight.StartRidleyAnimation(state, place, RidleyAnimations.CeresLunge);
                state.CeresPhase = CeresRidleyPhase.LungeMain;
                state.PhaseTimer = 64;
                CeresRidleyFlight.TickCeresRidleyLunge(place, state, samus);
                return true;

            case CeresRidleyPhase.LungeMain:
                CeresRidleyFlight.TickCeresRidleyLunge(place, state, samus);
                return true;

            case CeresRidleyPhase.SwoopSetup:
                state.CeresPhase = CeresRidleyPhase.SwoopMoveToPosition;
                state.PhaseTimer = 10;
                state.SwoopAngleAccumulator = RotationAngle.Zero;
                state.IdleTailWhipEnabled = false;
                CeresRidleyFlight.TickCeresRidleySwoopMoveToPosition(place, state);
                return true;

            case CeresRidleyPhase.SwoopMoveToPosition:
                CeresRidleyFlight.TickCeresRidleySwoopMoveToPosition(place, state);
                return true;

            case CeresRidleyPhase.SwoopDescendingAimingDown:
                CeresRidleyFlight.TickCeresRidleySwoopPhase(
                    state,
                    turn: new RidleySwoopTurn(RotationAngle.FromSteps(0, 32), RotationDirection.Clockwise),
                    targetAngle: RotationAngle.Zero - RotationAngle.FromSteps(4),
                    targetSpeed: SignedEightEight.FromPixels(3),
                    nextPhase: CeresRidleyPhase.SwoopDescendingAimingLeft,
                    nextTimer: 36);
                return true;

            case CeresRidleyPhase.SwoopDescendingAimingLeft:
                CeresRidleyFlight.TickCeresRidleySwoopPhase(
                    state,
                    turn: new RidleySwoopTurn(RotationAngle.FromSteps(2), RotationDirection.Clockwise),
                    targetAngle: RotationAngle.ThreeQuarterTurn,
                    targetSpeed: SignedEightEight.FromPixels(3),
                    nextPhase: CeresRidleyPhase.SwoopAscendingAimingUp,
                    nextTimer: 28);
                if (state.CeresPhase == CeresRidleyPhase.SwoopAscendingAimingUp)
                {
                    // The swoop requests one aimed tail fling at the exact bottom
                    // of the swoop. Neutral tail AI consumes and clears it later this same
                    // enemy-main pass; treating it as a persistent animation mode makes the
                    // tail lag an entire attack behind Ridley.
                    state.TailWhipRequested = true;
                }
                return true;

            case CeresRidleyPhase.SwoopAscendingAimingUp:
                CeresRidleyFlight.TickCeresRidleySwoopPhase(
                    state,
                    turn: new RidleySwoopTurn(RotationAngle.FromSteps(2), RotationDirection.Clockwise),
                    targetAngle: RotationAngle.Zero - SwoopClimbTarget,
                    targetSpeed: SignedEightEight.FromPixels(3),
                    nextPhase: CeresRidleyPhase.SwoopAscendingAimingUpFaster,
                    nextTimer: 1);
                return true;

            case CeresRidleyPhase.SwoopAscendingAimingUpFaster:
                CeresRidleyFlight.UpdateRidleySwoopVelocity(
                    state, turn: new RidleySwoopTurn(RotationAngle.FromSteps(3), RotationDirection.Clockwise), targetAngle: RotationAngle.Zero - SwoopClimbTarget,
                    targetSpeed: SignedEightEight.FromPixels(3));
                if (--state.PhaseTimer < 0)
                {
                    state.CeresPhase = CeresRidleyPhase.Hovering;
                    state.HoverCounter = 0;
                    state.IdleTailWhipEnabled = true;
                }
                return true;

            case CeresRidleyPhase.FakeRetreatMoveToPosition:
                CeresRidleyFlight.TickCeresRidleyFakeRetreatMoveToPosition(place, state);
                return true;

            case CeresRidleyPhase.FakeRetreatRising:
                CeresRidleyFlight.TickCeresRidleyFakeRetreatRising(place, state);
                return true;

            case CeresRidleyPhase.WaitBeforeRetrievingBaby:
                if (--state.PhaseTimer < 0)
                {
                    CeresRidleyFlight.StartRidleyAnimation(state, place, RidleyAnimations.RetrieveBabyMetroid);
                    state.CeresPhase = CeresRidleyPhase.RetrieveBaby;
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
                // The getaway handoff installs an empty step and publishes the room-main cutscene
                // state. The renderer stops emitting Ridley's ordinary composite at
                // this point because the rotating view owns his departure; hide the ordinary actor so
                // it cannot remain parked over that separately scoped presentation seam.
                state.CeresPhase = CeresRidleyPhase.Inactive;
                place.Presence = place.Presence.With(EnemyPresence.Hidden);
                _ridleyEncounter.CeresStatus = CeresEscapeStatus.RidleyEscaped;
                CeresRidleyGetaway.SetupCeresRidleyGetawayFloor(state);
                return true;

            case CeresRidleyPhase.Inactive:
                // The inactive phase is empty; room main owns the getaway.
                return false;

            case CeresRidleyPhase.ActivateSelfDestruct:
                Getaway.TickCeresRidleySelfDestruct(state);
                return false;

            case CeresRidleyPhase.SelfDestructAlarmOnly:
                CeresRidleyGetaway.UpdateCeresSelfDestructAlarm(state, place.FrameCounter);
                return false;

            default:
                throw new InvalidDataException($"Undefined {nameof(CeresRidleyPhase)} {state.CeresPhase}.");
        }
    }

    /// <summary>The fastest 1/256-pixel speed the acceleration clamps to toward a lower target: five pixels per update.</summary>
    internal static readonly SignedEightEight CeresRidleyMaximumSpeed = SignedEightEight.FromPixels(5);
    /// <summary>The fastest 1/256-pixel speed toward a higher target: five pixels per update the other way.</summary>
    internal static readonly SignedEightEight CeresRidleyMinimumSpeed = -CeresRidleyMaximumSpeed;

    /// <summary>The heading a swoop climbs toward, turned the swooping way: fifteen thirty-seconds of a turn.</summary>
    internal static readonly RotationAngle SwoopClimbTarget = RotationAngle.FromSteps(120);

    /// <summary>Runs Ceres Ridley's main step.</summary>
    internal void RunCeresRidleyMainAi(RoomEnemyPlace place, IEnemyAiFrame frame) =>
        RunCeresRidleyMain(place, frame.Samus, frame.SamusProjectiles);
}
