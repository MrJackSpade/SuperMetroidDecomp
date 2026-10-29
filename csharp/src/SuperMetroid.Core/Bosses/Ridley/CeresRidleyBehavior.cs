using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The Ceres Ridley species owner: its per-room state, AI routines and the interactions it registers.</summary>
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

    /// <summary>Ceres Ridley's Mode 7 getaway and the station's self-destruct countdown that follows.</summary>
    internal CeresRidleyGetaway Getaway { get; }
    /// <summary>The session's installed area artwork.</summary>
    private readonly AreaArtworkBinding _areaArtwork;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;




    /// <summary>Connects Ceres Ridley's enemy AI, drawing, collision and room-main collaborators.</summary>
    internal CeresRidleyBehavior(CeresRidleyBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.Presentation.Artwork;
        _areaArtwork = dependencies.AreaArtwork;
        _ridleyTail = dependencies.RidleyTail;
        _ridleyEncounter = dependencies.RidleyEncounter;
        _roomStatus = dependencies.RoomStatus;
        Flight = new CeresRidleyFlight(new CeresRidleyFlightDependencies { Dispatch = dependencies.Dispatch, Room = dependencies.Room, Earthquake = dependencies.Earthquake, Audio = dependencies.Audio, SlotTable = dependencies.SlotTable, Presentation = dependencies.Presentation.Surfaces, RidleyEncounter = _ridleyEncounter, BossArtwork = _bossArtwork });
        BabyScene = new CeresRidleyBabyScene(dependencies.Room, dependencies.Earthquake, dependencies.Audio, dependencies.SlotTable, dependencies.Presentation.Surfaces, _ridleyEncounter, _bossArtwork);
        Fades = new CeresRidleyFades(dependencies.Presentation.Surfaces, _bossArtwork);
        Hits = new CeresRidleyHits(dependencies.Room, dependencies.SlotTable, dependencies.Presentation.Surfaces, dependencies.Collision, _ridleyEncounter, _bossArtwork);
        Getaway = new CeresRidleyGetaway(new CeresRidleyGetawayDependencies { FrameResults = dependencies.FrameResults, Room = dependencies.Room, Audio = dependencies.Audio, SlotTable = dependencies.SlotTable, Presentation = dependencies.Presentation.Surfaces, RidleyEncounter = _ridleyEncounter, RoomStatus = _roomStatus, AreaArtwork = _areaArtwork , BossArtwork = _bossArtwork });
    }

    /// <summary>
    /// Ports the Ceres dispatcher reached from $A6:A288 through the end of the battle. The
    /// reveal, liftoff, hover, two cartridge-authored termination conditions, upward retreat,
    /// palette handoff, and <c>ceres_status = 1</c> publication all retain their ROM addresses.
    /// Ridley's later Mode-7 getaway presentation remains a room-main concern, not enemy AI.
    /// </summary>
    internal void RunCeresRidleyMain(
        RoomEnemySlot slot,
        SamusState? samus)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireCeresRidley(slot);
        slot.Health = short.MaxValue;

        // $A6:A29F runs after the boss dispatcher and before movement. None of the
        // translated dispatcher branches mutate the flash timer, so selecting the two
        // draw palettes here is equivalent while keeping their decision ahead of the
        // shared movement/composition continuation below.
        if (state.MovementAnimationEnabled && _ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            CeresRidleyHits.UpdateRidleyHurtFlashPalettes(slot, state, slot.FrameCounter);

        switch (state.Function)
        {
            case RidleyAiFunction.WaitForDoorTransition:
                // A35B waits even though enemy visual instructions run during fade.
                if (_roomStatus.EnemyDoorTransitionActive)
                    break;
                state.Function = RidleyAiFunction.InitialDelay;
                state.FunctionTimer = WrappingCountdown.Start(512);
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                break;

            case RidleyAiFunction.InitialDelay:
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                break;

            case RidleyAiFunction.FadeInEyes:
                Fades.TickCeresRidleyEyeFade(state);
                break;

            case RidleyAiFunction.FadeInBody:
                Fades.TickCeresRidleyBodyFade(slot, state);
                break;

            case RidleyAiFunction.WaitBeforeRoar:
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsNegative)
                {
                    CeresRidleyFlight.StartRidleyProgram(state, slot, RidleyBodyProgram.OpeningRoar);
                    state.FunctionTimer = WrappingCountdown.Start(252);
                    state.Function = RidleyAiFunction.WaitBeforeLiftoff;
                }
                break;

            case RidleyAiFunction.WaitBeforeLiftoff:
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsNegative)
                {
                    state.FadeStep = 0;
                    CeresRidleyFlight.StartRidleyProgram(state, slot, RidleyBodyProgram.TransitionToFlying);
                    // Function_Ridley_RoarBeforeFly at $A6:A4B4-A4CB primes the wing
                    // timer, enables every tail segment, and selects neutral tail AI in
                    // the same transition that installs instruction list $E91D.
                    state.WingAnimationTimer = WrappingCountdown.Start(8);
                    state.WingAnimationTimerDelta = 8;
                    foreach (RidleyTailSegment segment in state.TailSegments)
                        segment.Active = true;
                    state.TailFunctionIndex = RidleyTailFunction.Neutral;
                    state.Function = RidleyAiFunction.ClearVelocity;
                }
                break;

            case RidleyAiFunction.ClearVelocity:
                state.Velocity = new EightEightVector<RoomPixels>(0, 0);
                break;

            case RidleyAiFunction.CeresLiftoffAccelerating:
                // $A6:A6AF adds -16 to the 8.8 Y velocity before the common movement pass.
                state.Velocity = state.Velocity.WithY(new SignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue - 16))));
                if (unchecked((short)(slot.Position.Y.Whole - 112)) < 0)
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
                CeresRidleyFlight.StartRidleyProgram(state, slot, RidleyBodyProgram.CeresLunge);
                state.Function = RidleyAiFunction.CeresLungeMain;
                state.FunctionTimer = WrappingCountdown.Start(64);
                CeresRidleyFlight.TickCeresRidleyLunge(slot, state, samus);
                break;

            case RidleyAiFunction.CeresLungeMain:
                CeresRidleyFlight.TickCeresRidleyLunge(slot, state, samus);
                break;

            case RidleyAiFunction.CeresSwoopSetup:
                state.Function = RidleyAiFunction.CeresSwoopMoveToPosition;
                state.FunctionTimer = WrappingCountdown.Start(10);
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
                    targetAngle: -1024,
                    targetMagnitude: 768,
                    nextFunction: RidleyAiFunction.CeresSwoopDescendingAimingLeft,
                    nextTimer: 36);
                break;

            case RidleyAiFunction.CeresSwoopDescendingAimingLeft:
                CeresRidleyFlight.TickCeresRidleySwoopPhase(
                    state,
                    angleDelta: -512,
                    targetAngle: -16384,
                    targetMagnitude: 768,
                    nextFunction: RidleyAiFunction.CeresSwoopAscendingAimingUp,
                    nextTimer: 28);
                if (state.Function == RidleyAiFunction.CeresSwoopAscendingAimingUp)
                {
                    // `$A6:A91B-$A91E` requests one aimed tail fling at the exact bottom
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
                    targetAngle: -30720,
                    targetMagnitude: 768,
                    nextFunction: RidleyAiFunction.CeresSwoopAscendingAimingUpFaster,
                    nextTimer: 1);
                break;

            case RidleyAiFunction.CeresSwoopAscendingAimingUpFaster:
                CeresRidleyFlight.UpdateRidleySwoopVelocity(
                    state, angleDelta: -768, targetAngle: -30720, targetMagnitude: 768);
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsNegative)
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
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsNegative)
                {
                    CeresRidleyFlight.StartRidleyProgram(state, slot, RidleyBodyProgram.RetrieveBabyMetroid);
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
                // $A6:AA11 installs a null dispatcher and publishes the room-main cutscene
                // state. The native renderer stops emitting Ridley's ordinary composite at
                // this point because Mode 7 owns his departure; hide the ordinary actor so
                // it cannot remain parked over that separately scoped presentation seam.
                state.Function = RidleyAiFunction.CeresInactive;
                slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
                _ridleyEncounter.CeresStatus = CeresEscapeStatus.RidleyEscaped;
                CeresRidleyGetaway.SetupCeresRidleyGetawayFloor(state);
                break;

            case RidleyAiFunction.CeresInactive:
                // $A6:AA4F is an RTS; room main owns the getaway.
                return;

            case RidleyAiFunction.CeresActivateSelfDestruct:
                Getaway.TickCeresRidleySelfDestruct(state);
                return;

            case RidleyAiFunction.CeresSelfDestructPaletteOnly:
                Getaway.UpdateCeresSelfDestructPalette(state, slot.FrameCounter);
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

        // Once the eye fade enables composite animation, CeresRidley_Main performs the
        // common Ridley 8.8 movement pass after every dispatcher call. Keeping that order
        // matters at A6AF/A6C8: velocity changes affect position in the very same frame.
        if (state.MovementAnimationEnabled && _ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
        {
            Flight.IntegrateRidleyMovement(slot, state, wallImpact: RidleyWallImpact.CeresWall);
            CeresRidleyFlight.TickRidleyWingAnimation(state);
            _ridleyTail.RandomlyUpdateCeresRidleyTailCurliness(state);
            _ridleyTail.TickRidleyTail(slot, state, samus);
            Hits.UpdateCeresRidleyHealthPalette(state);
        }
        if (_ridleyEncounter.CeresStatus == CeresEscapeStatus.RidleyNotEscaped)
            CeresRidleyBabyScene.TickCeresBaby(slot, state);
    }

    /// <summary>The fastest 8.8 speed the acceleration clamps to toward a lower target: five pixels per update.</summary>
    internal const ushort CeresRidleyMaximumSpeed = 1280;
    /// <summary>The fastest 8.8 speed toward a higher target, as the native two's-complement word.</summary>
    internal const ushort CeresRidleyMinimumSpeed = unchecked((ushort)-1280);

    /// <summary>Ports Ceres Ridley's main dispatcher.</summary>
    internal void RunCeresRidleyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        RunCeresRidleyMain(slot, frame.Samus);

    /// <summary>Native wing frame deltas selected from Ridley's movement speed.</summary>
    internal static readonly ushort[] CeresRidleyWingAnimationDeltas =
        [12, 14, 16, 18, 28, 32, 40, 48];

    /// <summary>A signed 8.8 coefficient of one, unit scale, as the getaway's setup installs it.</summary>
    internal static readonly SignedEightEight FloorUnitCoefficient = SignedEightEight.FromSignedRaw(256);
}
