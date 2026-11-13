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

    /// <summary>Ceres Ridley's eye and body palette fades.</summary>
    internal CeresRidleyFades Fades { get; }

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
        Flight = new CeresRidleyFlight(new CeresRidleyFlightDependencies { Dispatch = dependencies.Dispatch, Room = dependencies.Room, Earthquake = dependencies.Earthquake, Audio = dependencies.Audio, SlotTable = dependencies.SlotTable, RidleyEncounter = _ridleyEncounter });
        BabyScene = new CeresRidleyBabyScene(dependencies.Room, dependencies.Earthquake, dependencies.Audio, dependencies.Presentation.Surfaces, _ridleyEncounter);
        Fades = new CeresRidleyFades(dependencies.Audio);
        Hits = new CeresRidleyHits(dependencies.Room, dependencies.Collision, _ridleyEncounter);
        Getaway = new CeresRidleyGetaway(new CeresRidleyGetawayDependencies { FrameResults = dependencies.FrameResults, Room = dependencies.Room, Audio = dependencies.Audio, Presentation = dependencies.Presentation.Surfaces, RidleyEncounter = _ridleyEncounter, RoomStatus = _roomStatus, BossArtwork = _bossArtwork, UnauthoredBoostCadence = dependencies.UnauthoredBoostCadence });
    }

    /// <summary>
    /// The health Ceres Ridley's main AI restores every update, kept exact (#1627): a hit later in the update takes its
    /// damage from this count, and what remains is the health the rest of that update reads.
    /// </summary>
    private const int RestoredHealth = 32767;

    /// <summary>
    /// Runs the Ceres dispatcher reached through the end of the battle. The
    /// reveal, liftoff, hover, two authored termination conditions, upward retreat,
    /// palette handoff, and <c>RidleyEscaped</c> publication all keep the game's order.
    /// Ridley's later rotating getaway presentation remains a room-main concern, not enemy AI.
    /// </summary>
    internal void RunCeresRidleyMain(
        RoomEnemySlot slot,
        SamusState? samus)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireCeresRidley(slot);
        slot.Health = RestoredHealth;

        // The palette selection runs after the boss dispatcher and before movement. None of the
        // dispatcher branches mutate the flash timer, so selecting the two
        // draw palettes here is equivalent while keeping their decision ahead of the
        // shared movement/composition continuation below.
        if (state.MovementAnimationEnabled && _ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            CeresRidleyHits.UpdateRidleyHurtFlashPalettes(slot, state, slot.FrameCounter);

        switch (state.Function)
        {
            case RidleyAiFunction.WaitForDoorTransition:
                // The wait holds even though enemy animations run during the fade.
                if (_roomStatus.EnemyDoorTransitionActive)
                    break;
                state.Function = RidleyAiFunction.InitialDelay;
                state.FunctionTimer = 512;
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                break;

            case RidleyAiFunction.InitialDelay:
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                break;

            case RidleyAiFunction.FadeInEyes:
                CeresRidleyFades.TickCeresRidleyEyeFade(state);
                break;

            case RidleyAiFunction.FadeInBody:
                Fades.TickCeresRidleyBodyFade(slot, state);
                break;

            case RidleyAiFunction.WaitBeforeRoar:
                if (--state.FunctionTimer < 0)
                {
                    CeresRidleyFlight.StartRidleyAnimation(state, slot, RidleyBodySegment.OpeningRoar);
                    state.FunctionTimer = 252;
                    state.Function = RidleyAiFunction.WaitBeforeLiftoff;
                }
                break;

            case RidleyAiFunction.WaitBeforeLiftoff:
                if (--state.FunctionTimer < 0)
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
                    state.TailFunctionIndex = RidleyTailFunction.Neutral;
                    state.Function = RidleyAiFunction.ClearVelocity;
                }
                break;

            case RidleyAiFunction.ClearVelocity:
                state.Velocity = EightEightVector<RoomPixels>.Zero;
                break;

            case RidleyAiFunction.CeresLiftoffAccelerating:
                // The rising step adds -16 to the 8.8 Y velocity before the common movement pass. Ridley rises from rest
                // until he passes line 112 of his chamber, so the speed stays a few pixels an update.
                state.Velocity = state.Velocity.WithY(state.Velocity.Y - SignedEightEight.FromTwoHundredFiftySixthsOfPixel(16));
                if (CoordinateAxis.Difference(slot.Position.Y.Whole, 112) < 0)
                {
                    state.Function = RidleyAiFunction.CeresLiftoffDecelerating;
                    CeresRidleyFlight.TickCeresRidleyLiftoffDecelerating(slot, state);
                }
                break;

            case RidleyAiFunction.CeresLiftoffDecelerating:
                CeresRidleyFlight.TickCeresRidleyLiftoffDecelerating(slot, state);
                break;

            case RidleyAiFunction.CeresHovering:
                Flight.TickCeresRidleyHover(slot, state, samus);
                break;

            case RidleyAiFunction.CeresFireballMoveToPosition:
                Flight.TickCeresRidleyFireballMove(slot, state);
                break;

            case RidleyAiFunction.CeresFireballShooting:
                Flight.TickCeresRidleyFireballShooting(slot, state);
                break;

            case RidleyAiFunction.CeresLungeSetup:
                CeresRidleyFlight.StartRidleyAnimation(state, slot, RidleyBodySegment.CeresLunge);
                state.Function = RidleyAiFunction.CeresLungeMain;
                state.FunctionTimer = 64;
                CeresRidleyFlight.TickCeresRidleyLunge(slot, state, samus);
                break;

            case RidleyAiFunction.CeresLungeMain:
                CeresRidleyFlight.TickCeresRidleyLunge(slot, state, samus);
                break;

            case RidleyAiFunction.CeresSwoopSetup:
                state.Function = RidleyAiFunction.CeresSwoopMoveToPosition;
                state.FunctionTimer = 10;
                state.SwoopAngleAccumulator = RotationAngle.Zero;
                state.IdleTailWhipEnabled = false;
                CeresRidleyFlight.TickCeresRidleySwoopMoveToPosition(slot, state);
                break;

            case RidleyAiFunction.CeresSwoopMoveToPosition:
                CeresRidleyFlight.TickCeresRidleySwoopMoveToPosition(slot, state);
                break;

            case RidleyAiFunction.CeresSwoopDescendingAimingDown:
                CeresRidleyFlight.TickCeresRidleySwoopPhase(
                    state,
                    angleDelta: -32,
                    targetAngle: RotationAngle.Zero - RotationAngle.FromSteps(4),
                    targetMagnitude: 768,
                    nextFunction: RidleyAiFunction.CeresSwoopDescendingAimingLeft,
                    nextTimer: 36);
                break;

            case RidleyAiFunction.CeresSwoopDescendingAimingLeft:
                CeresRidleyFlight.TickCeresRidleySwoopPhase(
                    state,
                    angleDelta: -512,
                    targetAngle: RotationAngle.ThreeQuarterTurn,
                    targetMagnitude: 768,
                    nextFunction: RidleyAiFunction.CeresSwoopAscendingAimingUp,
                    nextTimer: 28);
                if (state.Function == RidleyAiFunction.CeresSwoopAscendingAimingUp)
                {
                    // The swoop requests one aimed tail fling at the exact bottom
                    // of the swoop. Neutral tail AI consumes and clears it later this same
                    // enemy-main pass; treating it as a persistent animation mode makes the
                    // tail lag an entire attack behind Ridley.
                    state.TailWhipRequest = 1;
                }
                break;

            case RidleyAiFunction.CeresSwoopAscendingAimingUp:
                CeresRidleyFlight.TickCeresRidleySwoopPhase(
                    state,
                    angleDelta: -512,
                    targetAngle: RotationAngle.Zero - SwoopClimbTarget,
                    targetMagnitude: 768,
                    nextFunction: RidleyAiFunction.CeresSwoopAscendingAimingUpFaster,
                    nextTimer: 1);
                break;

            case RidleyAiFunction.CeresSwoopAscendingAimingUpFaster:
                CeresRidleyFlight.UpdateRidleySwoopVelocity(
                    state, angleDelta: -768, targetAngle: RotationAngle.Zero - SwoopClimbTarget, targetMagnitude: 768);
                if (--state.FunctionTimer < 0)
                {
                    state.Function = RidleyAiFunction.CeresHovering;
                    state.HoverCounter = 0;
                    state.IdleTailWhipEnabled = true;
                }
                break;

            case RidleyAiFunction.CeresFakeRetreatMoveToPosition:
                CeresRidleyFlight.TickCeresRidleyFakeRetreatMoveToPosition(slot, state);
                break;

            case RidleyAiFunction.CeresFakeRetreatRising:
                CeresRidleyFlight.TickCeresRidleyFakeRetreatRising(slot, state);
                break;

            case RidleyAiFunction.CeresWaitBeforeRetrievingBaby:
                if (--state.FunctionTimer < 0)
                {
                    CeresRidleyFlight.StartRidleyAnimation(state, slot, RidleyBodySegment.RetrieveBabyMetroid);
                    state.Function = RidleyAiFunction.CeresRetrieveBaby;
                    CeresRidleyFlight.TickCeresRidleyRetrieveBaby(slot, state);
                }
                break;

            case RidleyAiFunction.CeresRetrieveBaby:
                CeresRidleyFlight.TickCeresRidleyRetrieveBaby(slot, state);
                break;

            case RidleyAiFunction.CeresRealRetreatRising:
                Flight.TickCeresRidleyRetreat(slot, state);
                break;

            case RidleyAiFunction.CeresRetreatDelay:
                Flight.TickCeresRidleyRetreatDelay(state);
                break;

            case RidleyAiFunction.CeresPublishEscapeHandoff:
                // The getaway handoff installs a null dispatcher and publishes the room-main cutscene
                // state. The renderer stops emitting Ridley's ordinary composite at
                // this point because the rotating view owns his departure; hide the ordinary actor so
                // it cannot remain parked over that separately scoped presentation seam.
                state.Function = RidleyAiFunction.CeresInactive;
                slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
                _ridleyEncounter.CeresStatus = CeresEscapeStatus.RidleyEscaped;
                CeresRidleyGetaway.SetupCeresRidleyGetawayFloor(state);
                break;

            case RidleyAiFunction.CeresInactive:
                // The inactive function is empty; room main owns the getaway.
                return;

            case RidleyAiFunction.CeresActivateSelfDestruct:
                Getaway.TickCeresRidleySelfDestruct(state);
                return;

            case RidleyAiFunction.CeresSelfDestructPaletteOnly:
                CeresRidleyGetaway.UpdateCeresSelfDestructAlarm(state, slot.FrameCounter);
                return;

            case RidleyAiFunction.NorfairEnterArena:
            case RidleyAiFunction.NorfairSelectAttack:
            case RidleyAiFunction.NorfairHoverSetup:
            case RidleyAiFunction.NorfairHover:
            case RidleyAiFunction.NorfairSwoopSetup:
            case RidleyAiFunction.NorfairSwoopMoveToStart:
            case RidleyAiFunction.NorfairSwoopAimDown:
            case RidleyAiFunction.NorfairSwoopAimSideways:
            case RidleyAiFunction.NorfairSwoopAimUp:
            case RidleyAiFunction.NorfairSwoopClimb:
            case RidleyAiFunction.NorfairSwoopRecover:
            case RidleyAiFunction.NorfairPogoSetup:
            case RidleyAiFunction.NorfairPogoDescending:
            case RidleyAiFunction.NorfairPogoAscending:
            case RidleyAiFunction.NorfairFireballSetup:
            case RidleyAiFunction.NorfairFireballMoveToSide:
            case RidleyAiFunction.NorfairFireballMoveToHeight:
            case RidleyAiFunction.NorfairFireballAttack:
            case RidleyAiFunction.NorfairFireballRecover:
            case RidleyAiFunction.NorfairGrabApproach:
            case RidleyAiFunction.NorfairCarrySetup:
            case RidleyAiFunction.NorfairCarryMoveToAnchor:
            case RidleyAiFunction.NorfairCarryRise:
            case RidleyAiFunction.NorfairCarryRelease:
            case RidleyAiFunction.NorfairReturnToArena:
            case RidleyAiFunction.NorfairReleaseSamus:
            case RidleyAiFunction.NorfairDeathStart:
            case RidleyAiFunction.NorfairDeathExplosions:
            case RidleyAiFunction.NorfairDeathFall:
            case RidleyAiFunction.NorfairDeathImpact:
            case RidleyAiFunction.NorfairDeathWait:
            case RidleyAiFunction.NorfairDeathFinish:
            case RidleyAiFunction.NorfairDeathComplete:
            default:
                throw new InvalidDataException(
                    $"Ceres Ridley function {state.Function} is not translated.");
        }

        // Once the eye fade enables composite animation, Ceres Ridley's main behaviour performs the
        // common Ridley 8.8 movement pass after every dispatcher call. Keeping that order
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

    /// <summary>The fastest 8.8 speed the acceleration clamps to toward a lower target: five pixels per update.</summary>
    internal static readonly SignedEightEight CeresRidleyMaximumSpeed = SignedEightEight.FromPixels(5);
    /// <summary>The fastest 8.8 speed toward a higher target: five pixels per update the other way.</summary>
    internal static readonly SignedEightEight CeresRidleyMinimumSpeed = -CeresRidleyMaximumSpeed;

    /// <summary>The heading a swoop climbs toward, turned the swooping way: fifteen thirty-seconds of a turn.</summary>
    internal static readonly RotationAngle SwoopClimbTarget = RotationAngle.FromSteps(120);

    /// <summary>Runs Ceres Ridley's main dispatcher.</summary>
    internal void RunCeresRidleyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        RunCeresRidleyMain(slot, frame.Samus);
}
