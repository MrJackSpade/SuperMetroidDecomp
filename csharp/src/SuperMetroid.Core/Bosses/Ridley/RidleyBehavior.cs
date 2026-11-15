using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The Ridley species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class RidleyBehavior : IEnemySpecies
{
    /// <summary>Room services, level state, and RNG for the Norfair fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Owns fight state shared with cutscenes and room transitions.</summary>
    private readonly RidleyEncounter _ridleyEncounter;

    /// <summary>Advances Ridley's independent tail segments and their hit checks.</summary>
    private readonly RidleyTail _ridleyTail;

    /// <summary>Supplies shared Ceres and Norfair movement, fade, and animation behavior.</summary>
    private readonly CeresRidleyBehavior _ceresRidleyBehavior;
    /// <summary>Checks active door-transition state before the Norfair intro.</summary>
    private readonly EnemyRoomStatus _roomStatus;
    /// <summary>Runs Ridley's body and breakup animations and applies their cues.</summary>
    internal RidleyBodyAnimation Animation { get; }

    /// <summary>Norfair Ridley's arrival and fight: hovering, swoops, the wall sweep, the pogo attack and grabbing Samus.</summary>
    internal NorfairRidleyCombat Combat { get; }

    /// <summary>Norfair Ridley's death: the roar, breakup, explosions and the drop.</summary>
    internal NorfairRidleyDeath Death { get; }



    /// <summary>Wires Ridley's fight, movement, tail, projectile, audio, and room collaborators.</summary>
    internal RidleyBehavior(RidleyBehaviorDependencies dependencies)
    {
        _ceresRidleyBehavior = dependencies.CeresRidleyBehavior;
        _ridleyTail = dependencies.RidleyTail;
        _room = dependencies.Room;
        _ridleyEncounter = dependencies.RidleyEncounter;
        Animation = new RidleyBodyAnimation(dependencies.Audio, _ridleyEncounter, dependencies.RidleyProjectiles);
        Combat = new NorfairRidleyCombat(new NorfairRidleyCombatDependencies { Room = _room, Earthquake = dependencies.Earthquake, Audio = dependencies.Audio, RidleyEncounter = _ridleyEncounter, RidleyTail = _ridleyTail, RidleyProjectiles = dependencies.RidleyProjectiles, RoomFx = dependencies.RoomFx, FrameContext = dependencies.FrameContext });
        Death = new NorfairRidleyDeath(dependencies.Lifecycle, _room, dependencies.DeathsAndDrops, _ridleyEncounter, dependencies.RidleyProjectiles, dependencies.RoomFx, dependencies.Audio);
        _roomStatus = dependencies.RoomStatus;
    }

    /// <summary>
    /// Runs Norfair Ridley's per-frame behaviour, including the reveal phases he shares with
    /// Ceres Ridley. Composition is advanced after the selected phase, so a velocity change affects the same rendered
    /// frame.
    /// </summary>
    private void RunNorfairRidleyMain(
        RoomEnemySlot slot,
        SamusState? samus,
        ControllerButton controllerInput,
        RoomLevelData? level = null,
        SamusProjectileSystem? samusProjectiles = null,
        SamusBombProjectileSystem? sharedProjectiles = null,
        Position16<RoomPixels> camera = default)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireNorfairRidley(slot);
        state.HurtMovementClamp = Math.Max(0, state.HurtMovementClamp - 4);

        RidleyInteractions.UpdateNorfairRidleyIntangibility(slot, state, camera);
        NorfairRidleyCombat.PrepareNorfairRidleyCombatFrame(state, sharedProjectiles ?? throw new InvalidOperationException(
            "Ridley's power-bomb check requires the shared projectile owner."));

        RunNorfairRidleyPhase(slot, state, samus, controllerInput, level);

        if (state.MovementAnimationEnabled)
        {
            CeresRidleyHits.UpdateRidleyHurtFlash(slot, state, slot.FrameCounter);
            _ceresRidleyBehavior.Flight.IntegrateRidleyMovement(slot, state);
            CeresRidleyFlight.TickRidleyWingAnimation(state);
            _ridleyTail.TickRidleyTail(slot, state, samus);
            if (samusProjectiles is not null)
                _ridleyTail.ResolveRidleyTailProjectileHits(slot, state, samusProjectiles);
            // Main places carried Samus after body/tail movement.
            if (state.GrabState != RidleyGrabState.Released && samus is not null)
                NorfairRidleyCombat.UpdateNorfairRidleyGrabbedSamus(slot, state, samus);
        }

        RidleyInteractions.UpdateNorfairRidleyHealthBand(slot, state);
    }

    /// <summary>Runs the current Norfair reveal, flight, swoop, wall sweep, pogo, grab or death phase.</summary>
    internal void RunNorfairRidleyPhase(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        ControllerButton controllerInput,
        RoomLevelData? level)
    {
        switch (state.Phase)
        {
            case RidleyPhase.WaitForDoorTransition:
                if (_roomStatus.EnemyDoorTransitionActive)
                    return;
                state.Phase = RidleyPhase.InitialDelay;
                state.PhaseTimer = 170;
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                return;

            case RidleyPhase.InitialDelay:
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                return;

            case RidleyPhase.FadeInEyes:
                RidleyFadeIn.TickEyeFadeIn(state);
                return;

            case RidleyPhase.FadeInBody:
                _ceresRidleyBehavior.FadeIn.TickBodyFadeIn(slot, state);
                return;

            case RidleyPhase.WaitBeforeRoar:
                if (--state.PhaseTimer < 0)
                {
                    CeresRidleyFlight.StartRidleyAnimation(state, slot, RidleyBodySegment.OpeningRoar);
                    state.PhaseTimer = 0;
                    state.Phase = RidleyPhase.WaitBeforeLiftoff;
                }
                return;

            case RidleyPhase.WaitBeforeLiftoff:
                Combat.TickNorfairRidleyArenaReveal(slot, state);
                return;

            case RidleyPhase.ClearVelocity:
                state.Velocity = EightEightVector<RoomPixels>.Zero;
                return;

            case RidleyPhase.NorfairEnterArena:
                NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, new Position16<RoomPixels>(64, 256), divisor: 2);
                if (CeresRidleyFlight.IsWithinRidleyRectangle(slot, new(64, 256), new(8, 8)))
                {
                    state.FightMode = RidleyFightMode.Fighting;
                    state.Phase = RidleyPhase.NorfairSelectAttack;
                    SelectNorfairRidleyAttack(slot, state, samus, controllerInput, level);
                }
                return;

            case RidleyPhase.NorfairSelectAttack:
                SelectNorfairRidleyAttack(slot, state, samus, controllerInput, level);
                return;

            case RidleyPhase.NorfairHoverSetup:
                state.Phase = RidleyPhase.NorfairHover;
                state.PhaseTimer = 128;
                NorfairRidleyCombat.TickNorfairRidleyHover(slot, state);
                return;

            case RidleyPhase.NorfairHover:
                NorfairRidleyCombat.TickNorfairRidleyHover(slot, state);
                return;

            case RidleyPhase.NorfairSwoopSetup:
                state.Phase = RidleyPhase.NorfairSwoopMoveToStart;
                state.SwoopPhaseTimer = 10;
                state.SwoopAngleAccumulator = RotationAngle.Zero;
                NorfairRidleyCombat.TickNorfairRidleySwoopMoveToStart(slot, state);
                return;

            case RidleyPhase.NorfairSwoopMoveToStart:
                NorfairRidleyCombat.TickNorfairRidleySwoopMoveToStart(slot, state);
                return;

            case RidleyPhase.NorfairSwoopAimDown:
                NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                    state,
                    SwoopTurn(state, RotationAngle.FromSteps(0, 32)),
                    SwoopTarget(state, RotationAngle.FromSteps(2)),
                    SignedEightEight.FromParts(4, 128),
                    RidleyPhase.NorfairSwoopAimSideways,
                    20);
                return;

            case RidleyPhase.NorfairSwoopAimSideways:
                if (NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                        state,
                        SwoopTurn(state, RotationAngle.FromSteps(1, 64)),
                        SwoopTarget(state, RotationAngle.QuarterTurn),
                        SignedEightEight.FromPixels(5),
                        RidleyPhase.NorfairSwoopAimUp,
                        16))
                {
                    state.TailWhipRequested = true;
                }
                return;

            case RidleyPhase.NorfairSwoopAimUp:
                NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                    state,
                    SwoopTurn(state, RotationAngle.FromSteps(2)),
                    SwoopTarget(state, CeresRidleyBehavior.SwoopClimbTarget),
                    SignedEightEight.FromPixels(3),
                    RidleyPhase.NorfairSwoopClimb,
                    32);
                return;

            case RidleyPhase.NorfairSwoopClimb:
                if (NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                        state,
                        SwoopTurn(state, RotationAngle.FromSteps(4)),
                        SwoopTarget(state, CeresRidleyBehavior.SwoopClimbTarget),
                        SignedEightEight.FromPixels(3),
                        RidleyPhase.NorfairSwoopRecover,
                        32))
                {
                    NorfairRidleyCombat.SelectNorfairRidleyFacingAnimation(slot, state);
                }
                return;

            case RidleyPhase.NorfairSwoopRecover:
                CeresRidleyFlight.UpdateRidleySwoopVelocity(state, RidleySwoopTurn.None, RotationAngle.HalfTurn, SignedEightEight.FromParts(1, 192));
                if (state.SwoopPhaseTimer != 0)
                {
                    state.SwoopPhaseTimer--;
                    return;
                }
                state.Phase = NorfairRidleyCombat.SamusMovementUsesRidleyGrab(samus)
                    ? RidleyPhase.NorfairGrabApproach
                    : RidleyPhase.NorfairSelectAttack;
                return;

            case RidleyPhase.NorfairWallSweepSetup:
                state.IdealInterSegmentTailAngle = 11;
                state.TailExtensionSpeed = RidleyTailDefinitions.PogoExtensionSpeed;
                state.Phase = RidleyPhase.NorfairSweepToWallBehind;
                state.PhaseTimer = RidleyRandomDefinitions.WallSweepTimer.Of(_room.Random.Read()) + 32;
                Combat.TickNorfairRidleyWallSweep(slot, state, samus, RidleyWallSweepTarget.WallBehind);
                return;

            case RidleyPhase.NorfairSweepToWallBehind:
                Combat.TickNorfairRidleyWallSweep(slot, state, samus, RidleyWallSweepTarget.WallBehind);
                return;

            case RidleyPhase.NorfairSweepToWallAhead:
                Combat.TickNorfairRidleyWallSweep(slot, state, samus, RidleyWallSweepTarget.WallAhead);
                return;

            case RidleyPhase.NorfairPogoMoveToSide:
                Combat.TickNorfairRidleyPogoMoveToSide(slot, state, samus);
                return;

            case RidleyPhase.NorfairPogoMoveToHeight:
                Combat.TickNorfairRidleyPogoMoveToHeight(slot, state, samus);
                return;

            case RidleyPhase.NorfairPogoFall:
                Combat.TickNorfairRidleyPogoFall(slot, state, samus, level);
                return;

            case RidleyPhase.NorfairPogoRise:
                NorfairRidleyCombat.TickNorfairRidleyPogoRise(state, samus);
                return;

            case RidleyPhase.NorfairGrabApproach:
                Combat.TickNorfairRidleyGrabApproach(slot, state, samus);
                return;

            case RidleyPhase.NorfairPowerBombDodge:
                Combat.TickNorfairRidleyPowerBombDodge(slot, state);
                return;

            case RidleyPhase.NorfairCarrySetup:
                NorfairRidleyCombat.BeginNorfairRidleyCarry(slot, state);
                return;

            case RidleyPhase.NorfairCarryMoveToAnchor:
                NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, state.Target, 16);
                if (NorfairRidleyMovement.TickRidleyPhaseTimer(state))
                {
                    state.Phase = RidleyPhase.NorfairCarryRise;
                    state.PhaseTimer = 32;
                }
                return;

            case RidleyPhase.NorfairCarryRise:
                if (NorfairRidleyMovement.TickRidleyPhaseTimer(state))
                {
                    state.IdealInterSegmentTailAngle = RidleyTailDefinitions.CarryReleaseInterSegmentSpacing;
                    state.TailExtensionSpeed = RidleyTailDefinitions.StandardExtensionSpeed;
                    NorfairRidleyDeath.ReleaseNorfairRidleyGrab(state, samus);
                    state.Phase = RidleyPhase.NorfairCarryRelease;
                    state.PhaseTimer = 64;
                }
                else
                    NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, state.Target.WithY(256), 16);
                return;

            case RidleyPhase.NorfairCarryRelease:
                if (NorfairRidleyMovement.TickRidleyPhaseTimer(state))
                {
                    state.IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentSpacing;
                    state.TailExtensionSpeed = RidleyTailDefinitions.StandardExtensionSpeed;
                    state.Phase = RidleyPhase.NorfairSelectAttack;
                }
                else
                {
                    NorfairRidleyMovement.MoveNorfairRidleyToward(
                        slot, state, new Position16<RoomPixels>(RidleyMovementTargets.CarryReleaseX(state.FacingDirection), 224), 16);
                }
                return;

            case RidleyPhase.NorfairMoveToDeathSpot:
                NorfairRidleyCombat.TickNorfairRidleyMoveToDeathSpot(slot, state);
                return;

            case RidleyPhase.NorfairDeathRoar:
                Death.TickNorfairRidleyDeathRoar(slot, state);
                return;

            case RidleyPhase.NorfairDeathExplosions:
                Death.TickNorfairRidleyDeathExplosions(slot, state, samus);
                return;

            case RidleyPhase.NorfairDeathBreakup:
                Death.BeginNorfairRidleyBreakup(slot, state);
                return;

            case RidleyPhase.NorfairDeathWait:
                NorfairRidleyDeath.TickNorfairRidleyBreakupWait(state);
                return;

            case RidleyPhase.NorfairDeathFinish:
                Death.TickNorfairRidleyDeathFinish(slot, state);
                return;

            case RidleyPhase.NorfairDeathComplete:
                return;

            case RidleyPhase.CeresLiftoffAccelerating:
            case RidleyPhase.CeresLiftoffDecelerating:
            case RidleyPhase.CeresHovering:
            case RidleyPhase.CeresFireballMoveToPosition:
            case RidleyPhase.CeresFireballShooting:
            case RidleyPhase.CeresLungeSetup:
            case RidleyPhase.CeresLungeMain:
            case RidleyPhase.CeresSwoopSetup:
            case RidleyPhase.CeresSwoopMoveToPosition:
            case RidleyPhase.CeresSwoopDescendingAimingDown:
            case RidleyPhase.CeresSwoopDescendingAimingLeft:
            case RidleyPhase.CeresSwoopAscendingAimingUp:
            case RidleyPhase.CeresSwoopAscendingAimingUpFaster:
            case RidleyPhase.CeresRealRetreatRising:
            case RidleyPhase.CeresRetreatDelay:
            case RidleyPhase.CeresPublishEscapeHandoff:
            case RidleyPhase.CeresInactive:
            case RidleyPhase.CeresActivateSelfDestruct:
            case RidleyPhase.CeresSelfDestructAlarmOnly:
            case RidleyPhase.CeresFakeRetreatMoveToPosition:
            case RidleyPhase.CeresFakeRetreatRising:
            case RidleyPhase.CeresWaitBeforeRetrievingBaby:
            case RidleyPhase.CeresRetrieveBaby:
            default:
                throw new InvalidDataException(
                    $"Lower Norfair Ridley has no behaviour for phase {state.Phase}.");
        }
    }

    /// <summary>Chooses an attack situation by Samus's movement, Ridley's health and Samus's height in the arena.</summary>
    internal void SelectNorfairRidleyAttack(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        ControllerButton controllerInput,
        RoomLevelData? level)
    {
        RidleyAttackSituation situation;
        if (samus is not null && SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.SpinJumping)
        {
            situation = RidleyAttackSituation.SpinJumping;
        }
        else if (slot.Health == 0)
        {
            situation = RidleyAttackSituation.ZeroHealth;
            state.ZeroHealthLungeCount++;
        }
        else if (slot.Health < 14400)
        {
            situation = RidleyAttackSituation.Ordinary;
        }
        else if (samus is not null && samus.Position.Y.Whole >= 352)
        {
            situation = RidleyAttackSituation.PogoZone;
        }
        else if (NorfairRidleyCombat.SamusMovementUsesRidleyGrab(samus))
        {
            situation = RidleyAttackSituation.DamageBoosting;
        }
        else
        {
            situation = RidleyAttackSituation.Ordinary;
        }

        int choice = _room.Random.Next(RidleyRandomDefinitions.AttackChoice);
        state.Phase = RidleyAttackChoices.Resolve(situation, choice);

        // The choice runs the chosen phase at once. Retain that same-frame setup
        // so timers, animation changes, and velocity all begin on the selected frame.
        RunNorfairRidleyPhase(slot, state, samus, controllerInput, level);
    }

    /// <summary>Runs Lower Norfair Ridley's main AI.</summary>
    internal void RunRidleyMainAi(RoomEnemySlot slot, IEnemyAiFrame frame) =>
        RunNorfairRidleyMain(
            slot,
            frame.Samus,
            frame.ControllerInput,
            frame.Level,
            frame.SamusProjectiles,
            frame.SharedProjectiles,
            frame.Camera);

    /// <summary>
    /// Runs Ridley's hurt AI. Phase, movement, wing, and tail updates execute only on
    /// even actor frames; tail armor, flash/colour work, and the grabbed-Samus lag counter
    /// remain active on every hurt frame.
    /// </summary>
    internal void RunNorfairRidleyHurt(
        RoomEnemySlot body,
        SamusState? samus,
        ControllerButton controllerInput,
        RoomLevelData? level,
        SamusProjectileSystem? samusProjectiles,
        SamusBombProjectileSystem sharedProjectiles,
        Position16<RoomPixels> camera)
    {
        RidleyEnemyState state = _ridleyEncounter.RequireNorfairRidley(body);
        if (RidleyCadenceDefinitions.NorfairRidleyActsEverySecondUpdate.IsDue(body.FrameCounter))
        {
            RidleyInteractions.UpdateNorfairRidleyIntangibility(body, state, camera);
            NorfairRidleyCombat.PrepareNorfairRidleyCombatFrame(state, sharedProjectiles);
            RunNorfairRidleyPhase(body, state, samus, controllerInput, level);
            if (state.MovementAnimationEnabled)
            {
                _ceresRidleyBehavior.Flight.IntegrateRidleyMovement(body, state);
                CeresRidleyFlight.TickRidleyWingAnimation(state);
                _ridleyTail.TickRidleyTail(body, state, samus);
            }
        }

        if (samusProjectiles is not null && state.MovementAnimationEnabled)
            _ridleyTail.ResolveRidleyTailProjectileHits(body, state, samusProjectiles);
        CeresRidleyHits.UpdateRidleyHurtFlash(body, state, body.FrameCounter);
        RidleyInteractions.UpdateNorfairRidleyHealthBand(body, state);

        state.HurtMovementClamp = Math.Min(8, state.HurtMovementClamp + 1);
        if (state.GrabState != RidleyGrabState.Released && state.HurtMovementClamp >= 8 && samus is not null)
            NorfairRidleyCombat.UpdateNorfairRidleyGrabbedSamus(body, state, samus);
    }

    /// <summary>Adds Ridley's hurt behaviour for the post-hit AI window.</summary>
    /// <remarks>Ridley's hurt AI owns hurt frames instead of falling back to ordinary main AI. Movement and phase work runs only
    /// on even actor frames, while tail projectile armor and hurt flashes remain live on every frame.</remarks>
    internal void RunRidleyHurt(RoomEnemySlot slot, RoomEnemyFrame frame) =>
        RunNorfairRidleyHurt(
            slot,
            frame.Samus,
            frame.ControllerInput,
            frame.Level,
            frame.SamusProjectiles,
            frame.SharedProjectiles ?? throw new InvalidOperationException(
                "Ridley's power-bomb check requires the shared projectile owner."),
            frame.Camera);

    /// <summary>
    /// The heading a Norfair swoop turns toward: <paramref name="rightward"/> for a Ridley facing right or forward, and
    /// the same turn the other way round from zero for one facing left.
    /// </summary>
    private static RotationAngle SwoopTarget(RidleyEnemyState state, RotationAngle rightward) =>
        state.FacingDirection != RidleyFacing.Left ? rightward : RotationAngle.Zero - rightward;

    /// <summary>
    /// A Norfair swoop's turn of <paramref name="perUpdate"/> each update: counterclockwise, toward the right, for a Ridley
    /// facing right or forward, and clockwise for one facing left.
    /// </summary>
    private static RidleySwoopTurn SwoopTurn(RidleyEnemyState state, RotationAngle perUpdate) =>
        new(perUpdate, state.FacingDirection != RidleyFacing.Left ? RotationDirection.Counterclockwise : RotationDirection.Clockwise);
}
