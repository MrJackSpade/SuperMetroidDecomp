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
        Flight = new CeresRidleyFlight(new CeresRidleyFlightDependencies { Lifecycle = dependencies.Lifecycle, Room = dependencies.Room, Earthquake = dependencies.Earthquake, Audio = dependencies.Audio, SlotTable = dependencies.SlotTable, RidleyEncounter = _ridleyEncounter });
        BabyScene = new CeresRidleyBabyScene(dependencies.Room, dependencies.Earthquake, dependencies.Audio, dependencies.Presentation.Surfaces, _ridleyEncounter);
        FadeIn = new RidleyFadeIn(dependencies.Audio);
        Hits = new CeresRidleyHits(dependencies.Room, dependencies.Collision, _ridleyEncounter);
        Getaway = new CeresRidleyGetaway(new CeresRidleyGetawayDependencies { FrameResults = dependencies.FrameResults, Room = dependencies.Room, Audio = dependencies.Audio, Presentation = dependencies.Presentation.Surfaces, RidleyEncounter = _ridleyEncounter, RoomStatus = _roomStatus, BossArtwork = _bossArtwork, UnauthoredBoostCadence = dependencies.UnauthoredBoostCadence });
    }

    /// <summary>
    /// The health Ceres Ridley's main AI restores every update, kept exact (#1627): a hit later in the update takes its
    /// damage from this count, and what remains is the health the rest of that update reads.
    /// </summary>
    private const int RestoredHealth = 32767;

    /// <summary>
    /// Runs the Ceres step reached through the end of the battle. The
    /// reveal, liftoff, hover, two authored termination conditions, upward retreat,
    /// palette handoff, and <see cref="CeresEscapeStatus.RidleyEscaped"/> publication all keep the game's order.
    /// Ridley's later rotating getaway presentation remains a room-main concern, not enemy AI.
    /// </summary>
    internal void RunCeresRidleyMain(
        RoomEnemySlot slot,
        SamusState? samus)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireCeresRidley(slot);
        slot.Health = RestoredHealth;

        // The palette selection runs after the boss step and before movement. None of the
        // step branches mutate the flash timer, so selecting the two
        // draw palettes here is equivalent while keeping their decision ahead of the
        // shared movement/composition continuation below.
        if (state.MovementAnimationEnabled && _ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            CeresRidleyHits.UpdateRidleyHurtFlash(slot, state, slot.FrameCounter);

        switch (state.Phase)
        {
            case RidleyPhase.WaitForDoorTransition:
                // The wait holds even though enemy animations run during the fade.
                if (_roomStatus.EnemyDoorTransitionActive)
                    break;
                state.Phase = RidleyPhase.InitialDelay;
                state.PhaseTimer = 512;
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                break;

            case RidleyPhase.InitialDelay:
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                break;

            case RidleyPhase.FadeInEyes:
                RidleyFadeIn.TickEyeFadeIn(state);
                break;

            case RidleyPhase.FadeInBody:
                FadeIn.TickBodyFadeIn(slot, state);
                break;

            case RidleyPhase.WaitBeforeRoar:
                if (--state.PhaseTimer < 0)
                {
                    CeresRidleyFlight.StartRidleyAnimation(state, slot, RidleyBodySegment.OpeningRoar);
                    state.PhaseTimer = 252;
                    state.Phase = RidleyPhase.WaitBeforeLiftoff;
                }
                break;

            case RidleyPhase.WaitBeforeLiftoff:
                if (--state.PhaseTimer < 0)
                {
                    state.FadeStep = 0;
                    CeresRidleyFlight.StartRidleyAnimation(state, slot, RidleyBodySegment.TransitionToFlying);
                    // Ridley's roar before flying primes the wing
                    // timer, enables every tail segment, and selects neutral tail AI in
                    // the same transition that installs the flying animation.
                    state.WingAnimationTimer = 8;
                    state.WingAnimationTimerDelta = 8;
                    foreach (RidleyTailSegment segment in state.TailSegments)
                        segment.Active = true;
                    state.TailAction = RidleyTailAction.Neutral;
                    state.Phase = RidleyPhase.ClearVelocity;
                }
                break;

            case RidleyPhase.ClearVelocity:
                state.Velocity = EightEightVector<RoomPixels>.Zero;
                break;

            case RidleyPhase.CeresLiftoffAccelerating:
                // The rising step adds -16 to the 1/256-pixel Y velocity before the common movement pass. Ridley rises from rest
                // until he passes line 112 of his chamber, so the speed stays a few pixels an update.
                state.Velocity = state.Velocity.WithY(state.Velocity.Y - SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16));
                if (CoordinateAxis.Difference(slot.Position.Y.Whole, 112) < 0)
                {
                    state.Phase = RidleyPhase.CeresLiftoffDecelerating;
                    CeresRidleyFlight.TickCeresRidleyLiftoffDecelerating(slot, state);
                }
                break;

            case RidleyPhase.CeresLiftoffDecelerating:
                CeresRidleyFlight.TickCeresRidleyLiftoffDecelerating(slot, state);
                break;

            case RidleyPhase.CeresHovering:
                Flight.TickCeresRidleyHover(slot, state, samus);
                break;

            case RidleyPhase.CeresFireballMoveToPosition:
                Flight.TickCeresRidleyFireballMove(slot, state);
                break;

            case RidleyPhase.CeresFireballShooting:
                Flight.TickCeresRidleyFireballShooting(slot, state);
                break;

            case RidleyPhase.CeresLungeSetup:
                CeresRidleyFlight.StartRidleyAnimation(state, slot, RidleyBodySegment.CeresLunge);
                state.Phase = RidleyPhase.CeresLungeMain;
                state.PhaseTimer = 64;
                CeresRidleyFlight.TickCeresRidleyLunge(slot, state, samus);
                break;

            case RidleyPhase.CeresLungeMain:
                CeresRidleyFlight.TickCeresRidleyLunge(slot, state, samus);
                break;

            case RidleyPhase.CeresSwoopSetup:
                state.Phase = RidleyPhase.CeresSwoopMoveToPosition;
                state.PhaseTimer = 10;
                state.SwoopAngleAccumulator = RotationAngle.Zero;
                state.IdleTailWhipEnabled = false;
                CeresRidleyFlight.TickCeresRidleySwoopMoveToPosition(slot, state);
                break;

            case RidleyPhase.CeresSwoopMoveToPosition:
                CeresRidleyFlight.TickCeresRidleySwoopMoveToPosition(slot, state);
                break;

            case RidleyPhase.CeresSwoopDescendingAimingDown:
                CeresRidleyFlight.TickCeresRidleySwoopPhase(
                    state,
                    angleDelta: -32,
                    targetAngle: RotationAngle.Zero - RotationAngle.FromSteps(4),
                    targetMagnitude: 768,
                    nextPhase: RidleyPhase.CeresSwoopDescendingAimingLeft,
                    nextTimer: 36);
                break;

            case RidleyPhase.CeresSwoopDescendingAimingLeft:
                CeresRidleyFlight.TickCeresRidleySwoopPhase(
                    state,
                    angleDelta: -512,
                    targetAngle: RotationAngle.ThreeQuarterTurn,
                    targetMagnitude: 768,
                    nextPhase: RidleyPhase.CeresSwoopAscendingAimingUp,
                    nextTimer: 28);
                if (state.Phase == RidleyPhase.CeresSwoopAscendingAimingUp)
                {
                    // The swoop requests one aimed tail fling at the exact bottom
                    // of the swoop. Neutral tail AI consumes and clears it later this same
                    // enemy-main pass; treating it as a persistent animation mode makes the
                    // tail lag an entire attack behind Ridley.
                    state.TailWhipRequest = 1;
                }
                break;

            case RidleyPhase.CeresSwoopAscendingAimingUp:
                CeresRidleyFlight.TickCeresRidleySwoopPhase(
                    state,
                    angleDelta: -512,
                    targetAngle: RotationAngle.Zero - SwoopClimbTarget,
                    targetMagnitude: 768,
                    nextPhase: RidleyPhase.CeresSwoopAscendingAimingUpFaster,
                    nextTimer: 1);
                break;

            case RidleyPhase.CeresSwoopAscendingAimingUpFaster:
                CeresRidleyFlight.UpdateRidleySwoopVelocity(
                    state, angleDelta: -768, targetAngle: RotationAngle.Zero - SwoopClimbTarget, targetMagnitude: 768);
                if (--state.PhaseTimer < 0)
                {
                    state.Phase = RidleyPhase.CeresHovering;
                    state.HoverCounter = 0;
                    state.IdleTailWhipEnabled = true;
                }
                break;

            case RidleyPhase.CeresFakeRetreatMoveToPosition:
                CeresRidleyFlight.TickCeresRidleyFakeRetreatMoveToPosition(slot, state);
                break;

            case RidleyPhase.CeresFakeRetreatRising:
                CeresRidleyFlight.TickCeresRidleyFakeRetreatRising(slot, state);
                break;

            case RidleyPhase.CeresWaitBeforeRetrievingBaby:
                if (--state.PhaseTimer < 0)
                {
                    CeresRidleyFlight.StartRidleyAnimation(state, slot, RidleyBodySegment.RetrieveBabyMetroid);
                    state.Phase = RidleyPhase.CeresRetrieveBaby;
                    CeresRidleyFlight.TickCeresRidleyRetrieveBaby(slot, state);
                }
                break;

            case RidleyPhase.CeresRetrieveBaby:
                CeresRidleyFlight.TickCeresRidleyRetrieveBaby(slot, state);
                break;

            case RidleyPhase.CeresRealRetreatRising:
                Flight.TickCeresRidleyRetreat(slot, state);
                break;

            case RidleyPhase.CeresRetreatDelay:
                Flight.TickCeresRidleyRetreatDelay(state);
                break;

            case RidleyPhase.CeresPublishEscapeHandoff:
                // The getaway handoff installs an empty step and publishes the room-main cutscene
                // state. The renderer stops emitting Ridley's ordinary composite at
                // this point because the rotating view owns his departure; hide the ordinary actor so
                // it cannot remain parked over that separately scoped presentation seam.
                state.Phase = RidleyPhase.CeresInactive;
                slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
                _ridleyEncounter.CeresStatus = CeresEscapeStatus.RidleyEscaped;
                CeresRidleyGetaway.SetupCeresRidleyGetawayFloor(state);
                break;

            case RidleyPhase.CeresInactive:
                // The inactive phase is empty; room main owns the getaway.
                return;

            case RidleyPhase.CeresActivateSelfDestruct:
                Getaway.TickCeresRidleySelfDestruct(state);
                return;

            case RidleyPhase.CeresSelfDestructAlarmOnly:
                CeresRidleyGetaway.UpdateCeresSelfDestructAlarm(state, slot.FrameCounter);
                return;

            case RidleyPhase.NorfairEnterArena:
            case RidleyPhase.NorfairSelectAttack:
            case RidleyPhase.NorfairHoverSetup:
            case RidleyPhase.NorfairHover:
            case RidleyPhase.NorfairSwoopSetup:
            case RidleyPhase.NorfairSwoopMoveToStart:
            case RidleyPhase.NorfairSwoopAimDown:
            case RidleyPhase.NorfairSwoopAimSideways:
            case RidleyPhase.NorfairSwoopAimUp:
            case RidleyPhase.NorfairSwoopClimb:
            case RidleyPhase.NorfairSwoopRecover:
            case RidleyPhase.NorfairPogoSetup:
            case RidleyPhase.NorfairPogoDescending:
            case RidleyPhase.NorfairPogoAscending:
            case RidleyPhase.NorfairFireballSetup:
            case RidleyPhase.NorfairFireballMoveToSide:
            case RidleyPhase.NorfairFireballMoveToHeight:
            case RidleyPhase.NorfairFireballAttack:
            case RidleyPhase.NorfairFireballRecover:
            case RidleyPhase.NorfairGrabApproach:
            case RidleyPhase.NorfairCarrySetup:
            case RidleyPhase.NorfairCarryMoveToAnchor:
            case RidleyPhase.NorfairCarryRise:
            case RidleyPhase.NorfairCarryRelease:
            case RidleyPhase.NorfairReturnToArena:
            case RidleyPhase.NorfairReleaseSamus:
            case RidleyPhase.NorfairDeathStart:
            case RidleyPhase.NorfairDeathExplosions:
            case RidleyPhase.NorfairDeathFall:
            case RidleyPhase.NorfairDeathImpact:
            case RidleyPhase.NorfairDeathWait:
            case RidleyPhase.NorfairDeathFinish:
            case RidleyPhase.NorfairDeathComplete:
            default:
                throw new InvalidDataException(
                    $"Ceres Ridley has no behaviour for phase {state.Phase}.");
        }

        // Once the eye fade enables composite animation, Ceres Ridley's main behaviour performs the
        // common Ridley 1/256-pixel movement pass after every update. Keeping that order
        // matters in the rise and swoop: velocity changes affect position in the very same frame.
        if (state.MovementAnimationEnabled && _ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
        {
            Flight.IntegrateRidleyMovement(slot, state, wallImpact: RidleyWallImpact.CeresWall);
            CeresRidleyFlight.TickRidleyWingAnimation(state);
            _ridleyTail.RandomlyUpdateCeresRidleyTailCurliness(state);
            _ridleyTail.TickRidleyTail(slot, state, samus);
            CeresRidleyHits.UpdateCeresRidleyHealthBand(state);
        }
        if (_ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            CeresRidleyBabyScene.TickCeresBaby(slot, state);
    }

    /// <summary>The fastest 1/256-pixel speed the acceleration clamps to toward a lower target: five pixels per update.</summary>
    internal static readonly SignedEightEight CeresRidleyMaximumSpeed = SignedEightEight.FromPixels(5);
    /// <summary>The fastest 1/256-pixel speed toward a higher target: five pixels per update the other way.</summary>
    internal static readonly SignedEightEight CeresRidleyMinimumSpeed = -CeresRidleyMaximumSpeed;

    /// <summary>The heading a swoop climbs toward, turned the swooping way: fifteen thirty-seconds of a turn.</summary>
    internal static readonly RotationAngle SwoopClimbTarget = RotationAngle.FromSteps(120);

    /// <summary>Runs Ceres Ridley's main step.</summary>
    internal void RunCeresRidleyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        RunCeresRidleyMain(slot, frame.Samus);
}
