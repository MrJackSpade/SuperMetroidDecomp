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
    /// <summary>Runs Ridley's body and breakup programs and applies their cues.</summary>
    internal RidleyBodyAnimation Animation { get; }

    /// <summary>Ridley's hitboxes, weapon reactions, intangibility and health palette.</summary>
    internal RidleyInteractions Interactions { get; }

    /// <summary>Norfair Ridley's arrival and fight: hovering, swoops, pogo and ground attacks, and grabbing Samus.</summary>
    internal NorfairRidleyCombat Combat { get; }

    /// <summary>How Norfair Ridley accelerates and moves toward a target, and the function timer its steps count down.</summary>
    internal NorfairRidleyMovement Movement { get; }

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
        Interactions = new RidleyInteractions();
        Combat = new NorfairRidleyCombat(new NorfairRidleyCombatDependencies { Room = _room, Earthquake = dependencies.Earthquake, Audio = dependencies.Audio, RidleyEncounter = _ridleyEncounter, RidleyTail = _ridleyTail, RidleyProjectiles = dependencies.RidleyProjectiles, RoomFx = dependencies.RoomFx, FrameContext = dependencies.FrameContext });
        Movement = new NorfairRidleyMovement();
        Death = new NorfairRidleyDeath(dependencies.Dispatch, _room, dependencies.DeathsAndDrops, _ridleyEncounter, dependencies.RidleyProjectiles, dependencies.RoomFx, dependencies.Audio);
        _roomStatus = dependencies.RoomStatus;
    }

    /// <summary>
    /// Runs Norfair Ridley's per-frame behaviour and the area-two half of the shared reveal
    /// dispatcher. Composition is advanced after the selected function, so a velocity change affects the same rendered
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

        RunNorfairRidleyFunction(slot, state, samus, controllerInput, level);

        if (state.MovementAnimationEnabled)
        {
            CeresRidleyHits.UpdateRidleyHurtFlashPalettes(slot, state, slot.FrameCounter);
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

    /// <summary>Dispatches the current Norfair intro, flight, swoop, pogo, grab, or death state.</summary>
    internal void RunNorfairRidleyFunction(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        ControllerButton controllerInput,
        RoomLevelData? level)
    {
        switch (state.Function)
        {
            case RidleyAiFunction.WaitForDoorTransition:
                if (_roomStatus.EnemyDoorTransitionActive)
                    return;
                state.Function = RidleyAiFunction.InitialDelay;
                state.FunctionTimer = 170;
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                return;

            case RidleyAiFunction.InitialDelay:
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                return;

            case RidleyAiFunction.FadeInEyes:
                CeresRidleyFades.TickCeresRidleyEyeFade(state);
                return;

            case RidleyAiFunction.FadeInBody:
                _ceresRidleyBehavior.Fades.TickCeresRidleyBodyFade(slot, state);
                return;

            case RidleyAiFunction.WaitBeforeRoar:
                if (--state.FunctionTimer < 0)
                {
                    CeresRidleyFlight.StartRidleyAnimation(state, slot, RidleyBodySegment.OpeningRoar);
                    state.FunctionTimer = 0;
                    state.Function = RidleyAiFunction.WaitBeforeLiftoff;
                }
                return;

            case RidleyAiFunction.WaitBeforeLiftoff:
                Combat.TickNorfairRidleyArenaReveal(slot, state);
                return;

            case RidleyAiFunction.ClearVelocity:
                state.Velocity = EightEightVector<RoomPixels>.Zero;
                return;

            case RidleyAiFunction.NorfairEnterArena:
                NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, new Position16<RoomPixels>(64, 256), divisor: 2);
                if (CeresRidleyFlight.IsWithinRidleyRectangle(slot, new(64, 256), new(8, 8)))
                {
                    state.FightMode = RidleyFightMode.Fighting;
                    state.Function = RidleyAiFunction.NorfairSelectAttack;
                    SelectNorfairRidleyAttack(slot, state, samus, controllerInput, level);
                }
                return;

            case RidleyAiFunction.NorfairSelectAttack:
                SelectNorfairRidleyAttack(slot, state, samus, controllerInput, level);
                return;

            case RidleyAiFunction.NorfairHoverSetup:
                state.Function = RidleyAiFunction.NorfairHover;
                state.FunctionTimer = 128;
                NorfairRidleyCombat.TickNorfairRidleyHover(slot, state);
                return;

            case RidleyAiFunction.NorfairHover:
                NorfairRidleyCombat.TickNorfairRidleyHover(slot, state);
                return;

            case RidleyAiFunction.NorfairSwoopSetup:
                state.Function = RidleyAiFunction.NorfairSwoopMoveToStart;
                state.SwoopPhaseTimer = 10;
                state.SwoopAngleAccumulator = RotationAngle.Zero;
                NorfairRidleyCombat.TickNorfairRidleySwoopMoveToStart(slot, state);
                return;

            case RidleyAiFunction.NorfairSwoopMoveToStart:
                NorfairRidleyCombat.TickNorfairRidleySwoopMoveToStart(slot, state);
                return;

            case RidleyAiFunction.NorfairSwoopAimDown:
                NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                    state,
                    state.FacingDirection != RidleyFacing.Left ? 32 : -32,
                    SwoopTarget(state, RotationAngle.FromSteps(2)),
                    1152,
                    RidleyAiFunction.NorfairSwoopAimSideways,
                    20);
                return;

            case RidleyAiFunction.NorfairSwoopAimSideways:
                if (NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                        state,
                        state.FacingDirection != RidleyFacing.Left ? 320 : -320,
                        SwoopTarget(state, RotationAngle.QuarterTurn),
                        1280,
                        RidleyAiFunction.NorfairSwoopAimUp,
                        16))
                {
                    state.TailWhipRequest = 1;
                }
                return;

            case RidleyAiFunction.NorfairSwoopAimUp:
                NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                    state,
                    state.FacingDirection != RidleyFacing.Left ? 512 : -512,
                    SwoopTarget(state, CeresRidleyBehavior.SwoopClimbTarget),
                    768,
                    RidleyAiFunction.NorfairSwoopClimb,
                    32);
                return;

            case RidleyAiFunction.NorfairSwoopClimb:
                if (NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                        state,
                        state.FacingDirection != RidleyFacing.Left ? 1024 : -1024,
                        SwoopTarget(state, CeresRidleyBehavior.SwoopClimbTarget),
                        768,
                        RidleyAiFunction.NorfairSwoopRecover,
                        32))
                {
                    NorfairRidleyCombat.SelectNorfairRidleyFacingAnimation(slot, state);
                }
                return;

            case RidleyAiFunction.NorfairSwoopRecover:
                CeresRidleyFlight.UpdateRidleySwoopVelocity(state, 0, RotationAngle.HalfTurn, 448);
                if (state.SwoopPhaseTimer != 0)
                {
                    state.SwoopPhaseTimer--;
                    return;
                }
                state.Function = NorfairRidleyCombat.SamusMovementUsesRidleyGrab(samus)
                    ? RidleyAiFunction.NorfairGrabApproach
                    : RidleyAiFunction.NorfairSelectAttack;
                return;

            case RidleyAiFunction.NorfairPogoSetup:
                state.IdealInterSegmentTailAngle = 11;
                state.TailExtensionSpeed = RidleyTailDefinitions.PogoExtensionSpeed;
                state.Function = RidleyAiFunction.NorfairPogoDescending;
                state.FunctionTimer = RidleyRandomDefinitions.PogoDescentTimer.Of(_room.Random.Read()) + 32;
                Combat.TickNorfairRidleyPogo(slot, state, samus, direction: VerticalDirection.Down);
                return;

            case RidleyAiFunction.NorfairPogoDescending:
                Combat.TickNorfairRidleyPogo(slot, state, samus, direction: VerticalDirection.Down);
                return;

            case RidleyAiFunction.NorfairPogoAscending:
                Combat.TickNorfairRidleyPogo(slot, state, samus, direction: VerticalDirection.Up);
                return;

            case RidleyAiFunction.NorfairFireballSetup:
                NorfairRidleyCombat.BeginNorfairRidleyGroundAttack(state);
                return;

            case RidleyAiFunction.NorfairFireballMoveToSide:
                Combat.TickNorfairRidleyGroundAttackMoveToSide(slot, state, samus);
                return;

            case RidleyAiFunction.NorfairFireballMoveToHeight:
                Combat.TickNorfairRidleyGroundAttackMoveToHeight(slot, state, samus);
                return;

            case RidleyAiFunction.NorfairFireballAttack:
                Combat.TickNorfairRidleyGroundAttack(slot, state, samus, level);
                return;

            case RidleyAiFunction.NorfairFireballRecover:
                NorfairRidleyCombat.TickNorfairRidleyGroundAttackRecovery(state, samus);
                return;

            case RidleyAiFunction.NorfairGrabApproach:
                Combat.TickNorfairRidleyGrabApproach(slot, state, samus);
                return;

            case RidleyAiFunction.NorfairReturnToArena:
                Combat.TickNorfairRidleyPowerBombDodge(slot, state);
                return;

            case RidleyAiFunction.NorfairCarrySetup:
                NorfairRidleyCombat.BeginNorfairRidleyCarry(slot, state);
                return;

            case RidleyAiFunction.NorfairCarryMoveToAnchor:
                NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, state.Target, 16);
                if (NorfairRidleyMovement.TickRidleyFunctionTimer(state))
                {
                    state.Function = RidleyAiFunction.NorfairCarryRise;
                    state.FunctionTimer = 32;
                }
                return;

            case RidleyAiFunction.NorfairCarryRise:
                if (NorfairRidleyMovement.TickRidleyFunctionTimer(state))
                {
                    state.IdealInterSegmentTailAngle = RidleyTailDefinitions.CarryReleaseInterSegmentSpacing;
                    state.TailExtensionSpeed = RidleyTailDefinitions.StandardExtensionSpeed;
                    NorfairRidleyDeath.ReleaseNorfairRidleyGrab(state, samus);
                    state.Function = RidleyAiFunction.NorfairCarryRelease;
                    state.FunctionTimer = 64;
                }
                else
                    NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, state.Target.WithY(256), 16);
                return;

            case RidleyAiFunction.NorfairCarryRelease:
                if (NorfairRidleyMovement.TickRidleyFunctionTimer(state))
                {
                    state.IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentSpacing;
                    state.TailExtensionSpeed = RidleyTailDefinitions.StandardExtensionSpeed;
                    state.Function = RidleyAiFunction.NorfairSelectAttack;
                }
                else
                {
                    NorfairRidleyMovement.MoveNorfairRidleyToward(
                        slot, state, new Position16<RoomPixels>(RidleyMovementTargets.CarryReleaseX(state.FacingDirection), 224), 16);
                }
                return;

            case RidleyAiFunction.NorfairReleaseSamus:
                NorfairRidleyCombat.TickNorfairRidleyMoveToDeathSpot(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathStart:
                NorfairRidleyCombat.BeginNorfairRidleyDeathRoar(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathExplosions:
                Death.TickNorfairRidleyDeathRoar(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathFall:
                Death.TickNorfairRidleyDeathExplosions(slot, state, samus);
                return;

            case RidleyAiFunction.NorfairDeathImpact:
                Death.BeginNorfairRidleyBreakup(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathWait:
                NorfairRidleyDeath.TickNorfairRidleyBreakupWait(state);
                return;

            case RidleyAiFunction.NorfairDeathFinish:
                Death.TickNorfairRidleyDeathFinish(slot, state);
                return;

            case RidleyAiFunction.NorfairDeathComplete:
                return;

            case RidleyAiFunction.CeresLiftoffAccelerating:
            case RidleyAiFunction.CeresLiftoffDecelerating:
            case RidleyAiFunction.CeresHovering:
            case RidleyAiFunction.CeresFireballMoveToPosition:
            case RidleyAiFunction.CeresFireballShooting:
            case RidleyAiFunction.CeresLungeSetup:
            case RidleyAiFunction.CeresLungeMain:
            case RidleyAiFunction.CeresSwoopSetup:
            case RidleyAiFunction.CeresSwoopMoveToPosition:
            case RidleyAiFunction.CeresSwoopDescendingAimingDown:
            case RidleyAiFunction.CeresSwoopDescendingAimingLeft:
            case RidleyAiFunction.CeresSwoopAscendingAimingUp:
            case RidleyAiFunction.CeresSwoopAscendingAimingUpFaster:
            case RidleyAiFunction.CeresRealRetreatRising:
            case RidleyAiFunction.CeresRetreatDelay:
            case RidleyAiFunction.CeresPublishEscapeHandoff:
            case RidleyAiFunction.CeresInactive:
            case RidleyAiFunction.CeresActivateSelfDestruct:
            case RidleyAiFunction.CeresSelfDestructPaletteOnly:
            case RidleyAiFunction.CeresFakeRetreatMoveToPosition:
            case RidleyAiFunction.CeresFakeRetreatRising:
            case RidleyAiFunction.CeresWaitBeforeRetrievingBaby:
            case RidleyAiFunction.CeresRetrieveBaby:
            default:
                throw new InvalidDataException(
                    $"Lower Norfair Ridley function {state.Function} is not translated.");
        }
    }

    /// <summary>Chooses an attack table by Samus movement, Ridley health, and arena position.</summary>
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
            situation = RidleyAttackSituation.BelowHalfHealth;
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
            situation = slot.Health < 9000 ? RidleyAttackSituation.AboveHalfHealth : RidleyAttackSituation.BelowHalfHealth;
        }

        int choice = _room.Random.Next(RidleyRandomDefinitions.AttackChoice);
        state.Function = RidleyAttackChoices.Resolve(situation, choice);

        // The selector runs the chosen function at once. Retain that same-frame setup
        // so timers, program changes, and velocity all begin on the selected frame.
        RunNorfairRidleyFunction(slot, state, samus, controllerInput, level);
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
    /// Runs Ridley's hurt AI. Function, movement, wing, and tail updates execute only on
    /// even actor frames; tail armor, flash/palette work, and the grabbed-Samus lag counter
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
            RunNorfairRidleyFunction(body, state, samus, controllerInput, level);
            if (state.MovementAnimationEnabled)
            {
                _ceresRidleyBehavior.Flight.IntegrateRidleyMovement(body, state);
                CeresRidleyFlight.TickRidleyWingAnimation(state);
                _ridleyTail.TickRidleyTail(body, state, samus);
            }
        }

        if (samusProjectiles is not null && state.MovementAnimationEnabled)
            _ridleyTail.ResolveRidleyTailProjectileHits(body, state, samusProjectiles);
        CeresRidleyHits.UpdateRidleyHurtFlashPalettes(body, state, body.FrameCounter);
        RidleyInteractions.UpdateNorfairRidleyHealthBand(body, state);

        state.HurtMovementClamp = Math.Min(8, state.HurtMovementClamp + 1);
        if (state.GrabState != RidleyGrabState.Released && state.HurtMovementClamp >= 8 && samus is not null)
            NorfairRidleyCombat.UpdateNorfairRidleyGrabbedSamus(body, state, samus);
    }

    /// <summary>Adds Ridley's hurt callback for the post-hit AI window.</summary>
    /// <remarks>Ridley's hurt AI owns hurt frames instead of falling back to ordinary main AI. Movement/function work runs only
    /// on even actor frames, while tail projectile armor and hurt palettes remain live on every frame.</remarks>
    internal void AddRidleyHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.DefinitionId == EnemyDefinitionId.Ridley, (slot, frame) => RunNorfairRidleyHurt(
            slot,
            frame.Samus,
            frame.ControllerInput,
            frame.Level,
            frame.SamusProjectiles,
            frame.SharedProjectiles ?? throw new InvalidOperationException(
                "Ridley's power-bomb check requires the shared projectile owner."),
            frame.Camera));

    /// <summary>
    /// The heading a Norfair swoop turns toward: <paramref name="rightward"/> for a Ridley facing right or forward, and
    /// the same turn the other way round from zero for one facing left.
    /// </summary>
    private static RotationAngle SwoopTarget(RidleyEnemyState state, RotationAngle rightward) =>
        state.FacingDirection != RidleyFacing.Left ? rightward : RotationAngle.Zero - rightward;
}
