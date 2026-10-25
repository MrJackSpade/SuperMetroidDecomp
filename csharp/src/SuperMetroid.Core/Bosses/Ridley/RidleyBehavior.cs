using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The Ridley species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class RidleyBehavior : IEnemySpecies
{
    /// <summary>Room services, level state, and native RNG for the Norfair fight.</summary>
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

    /// <summary>How Norfair Ridley accelerates and moves toward a target, and the function timer its routines count down.</summary>
    internal NorfairRidleyMovement Movement { get; }

    /// <summary>Norfair Ridley's death: the roar, breakup, explosions and the drop.</summary>
    internal NorfairRidleyDeath Death { get; }
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;



    /// <summary>Wires Ridley's fight, movement, tail, projectile, audio, and room collaborators.</summary>
    internal RidleyBehavior(RidleyBehaviorDependencies dependencies)
    {
        _bossArtwork = dependencies.Presentation.Artwork;
        _ceresRidleyBehavior = dependencies.CeresRidleyBehavior;
        _ridleyTail = dependencies.RidleyTail;
        _room = dependencies.Room;
        _ridleyEncounter = dependencies.RidleyEncounter;
        Animation = new RidleyBodyAnimation(dependencies.Audio, _ridleyEncounter, dependencies.RidleyProjectiles);
        Interactions = new RidleyInteractions(dependencies.Presentation.Surfaces, _bossArtwork);
        Combat = new NorfairRidleyCombat(new NorfairRidleyCombatDependencies { Room = _room, Earthquake = dependencies.Earthquake, Audio = dependencies.Audio, Presentation = dependencies.Presentation.Surfaces, RidleyEncounter = _ridleyEncounter, RidleyTail = _ridleyTail, RidleyProjectiles = dependencies.RidleyProjectiles, RoomFx = dependencies.RoomFx , BossArtwork = _bossArtwork });
        Movement = new NorfairRidleyMovement();
        Death = new NorfairRidleyDeath(dependencies.Dispatch, _room, dependencies.DeathsAndDrops, _ridleyEncounter, dependencies.RidleyProjectiles, dependencies.RoomFx);
        _roomStatus = dependencies.RoomStatus;
    }

    /// <summary>
    /// Ports <c>Ridley_Main</c> at $A6:B227 and the area-two half of the shared reveal
    /// dispatcher. Composition is advanced after the selected function, exactly like the
    /// cartridge, so a velocity change affects the same rendered frame.
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
            "Ridley's power-bomb check ($A6:BD2C) requires the shared projectile owner."));

        RunNorfairRidleyFunction(slot, state, samus, controllerInput, level);

        if (state.MovementAnimationEnabled)
        {
            CeresRidleyHits.UpdateRidleyHurtFlashPalettes(slot, state, slot.FrameCounter);
            _ceresRidleyBehavior.Flight.IntegrateRidleyMovement(slot, state);
            CeresRidleyFlight.TickRidleyWingAnimation(state);
            _ridleyTail.TickRidleyTail(slot, state, samus);
            if (samusProjectiles is not null)
                _ridleyTail.ResolveRidleyTailProjectileHits(slot, state, samusProjectiles);
            // Native Main places carried Samus after body/tail movement.
            if (state.GrabState != RidleyGrabState.Released && samus is not null)
                NorfairRidleyCombat.UpdateNorfairRidleyGrabbedSamus(slot, state, samus);
        }

        Interactions.UpdateNorfairRidleyHealthPalette(slot, state);
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
                state.FunctionTimer = WrappingCountdown.Start(170);
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                return;

            case RidleyAiFunction.InitialDelay:
                CeresRidleyFlight.TickCeresRidleyInitialDelay(state);
                return;

            case RidleyAiFunction.FadeInEyes:
                _ceresRidleyBehavior.Fades.TickCeresRidleyEyeFade(state);
                return;

            case RidleyAiFunction.FadeInBody:
                _ceresRidleyBehavior.Fades.TickCeresRidleyBodyFade(slot, state);
                if (state.Function == RidleyAiFunction.WaitBeforeRoar)
                    slot.Layer = 2;
                return;

            case RidleyAiFunction.WaitBeforeRoar:
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsNegative)
                {
                    CeresRidleyFlight.StartRidleyProgram(state, slot, RidleyBodyProgram.OpeningRoar);
                    state.FunctionTimer = WrappingCountdown.Start(0);
                    state.Function = RidleyAiFunction.WaitBeforeLiftoff;
                }
                return;

            case RidleyAiFunction.WaitBeforeLiftoff:
                Combat.TickNorfairRidleyArenaReveal(slot, state);
                return;

            case RidleyAiFunction.ClearVelocity:
                state.Velocity = new EightEightVector<RoomPixels>(0, 0);
                return;

            case RidleyAiFunction.NorfairEnterArena:
                NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, new Position16<RoomPixels>(64, 256), divisorIndex: 14);
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
                state.FunctionTimer = WrappingCountdown.Start(128);
                NorfairRidleyCombat.TickNorfairRidleyHover(slot, state);
                return;

            case RidleyAiFunction.NorfairHover:
                NorfairRidleyCombat.TickNorfairRidleyHover(slot, state);
                return;

            case RidleyAiFunction.NorfairSwoopSetup:
                state.Function = RidleyAiFunction.NorfairSwoopMoveToStart;
                state.SwoopPhaseTimer = WrappingCountdown.Start(10);
                state.SwoopAngleAccumulator = SnesAngle.Zero;
                NorfairRidleyCombat.TickNorfairRidleySwoopMoveToStart(slot, state);
                return;

            case RidleyAiFunction.NorfairSwoopMoveToStart:
                NorfairRidleyCombat.TickNorfairRidleySwoopMoveToStart(slot, state);
                return;

            case RidleyAiFunction.NorfairSwoopAimDown:
                NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                    state,
                    state.FacingDirection != RidleyFacing.Left ? 32 : -32,
                    state.FacingDirection != RidleyFacing.Left ? 512 : -512,
                    1152,
                    RidleyAiFunction.NorfairSwoopAimSideways,
                    20);
                return;

            case RidleyAiFunction.NorfairSwoopAimSideways:
                if (NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                        state,
                        state.FacingDirection != RidleyFacing.Left ? 320 : -320,
                        state.FacingDirection != RidleyFacing.Left ? 16384 : -16384,
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
                    state.FacingDirection != RidleyFacing.Left ? 30720 : -30720,
                    768,
                    RidleyAiFunction.NorfairSwoopClimb,
                    32);
                return;

            case RidleyAiFunction.NorfairSwoopClimb:
                if (NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                        state,
                        state.FacingDirection != RidleyFacing.Left ? 1024 : -1024,
                        state.FacingDirection != RidleyFacing.Left ? 30720 : -30720,
                        768,
                        RidleyAiFunction.NorfairSwoopRecover,
                        32))
                {
                    NorfairRidleyCombat.SelectNorfairRidleyFacingInstruction(slot, state);
                }
                return;

            case RidleyAiFunction.NorfairSwoopRecover:
                CeresRidleyFlight.UpdateRidleySwoopVelocity(state, 0, short.MinValue, 448);
                if (!state.SwoopPhaseTimer.IsZero)
                {
                    state.SwoopPhaseTimer = state.SwoopPhaseTimer.Decrement().Remaining;
                    return;
                }
                state.Function = NorfairRidleyCombat.SamusMovementUsesRidleyGrab(samus)
                    ? RidleyAiFunction.NorfairGrabApproach
                    : RidleyAiFunction.NorfairSelectAttack;
                return;

            case RidleyAiFunction.NorfairPogoSetup:
                state.IdealInterSegmentTailAngle = 11;
                state.TailExtensionSpeed = 384;
                state.Function = RidleyAiFunction.NorfairPogoDescending;
                state.FunctionTimer = WrappingCountdown.FromRaw(unchecked((ushort)(_room.Random.Read() % 32 + 32)));
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
                NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, state.Target, 0);
                if (NorfairRidleyMovement.TickRidleyFunctionTimer(state))
                {
                    state.Function = RidleyAiFunction.NorfairCarryRise;
                    state.FunctionTimer = WrappingCountdown.Start(32);
                }
                return;

            case RidleyAiFunction.NorfairCarryRise:
                if (NorfairRidleyMovement.TickRidleyFunctionTimer(state))
                {
                    state.IdealInterSegmentTailAngle = RidleyTailDefinitions.CarryReleaseInterSegmentSpacing;
                    state.TailExtensionSpeed = RidleyTailDefinitions.CarryReleaseExtensionSpeed;
                    NorfairRidleyDeath.ReleaseNorfairRidleyGrab(state, samus);
                    state.Function = RidleyAiFunction.NorfairCarryRelease;
                    state.FunctionTimer = WrappingCountdown.Start(64);
                }
                else
                    NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, state.Target.WithY(256), 0);
                return;

            case RidleyAiFunction.NorfairCarryRelease:
                if (NorfairRidleyMovement.TickRidleyFunctionTimer(state))
                {
                    state.IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentSpacing;
                    state.TailExtensionSpeed = RidleyTailDefinitions.CarryReleaseExtensionSpeed;
                    state.Function = RidleyAiFunction.NorfairSelectAttack;
                }
                else
                {
                    ushort releaseX = RidleyMovementTargets.CarryReleaseX(state.FacingDirection);
                    NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, new Position16<RoomPixels>(releaseX, 224), 0);
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

    /// <summary>Chooses a native attack table by Samus movement, Ridley health, and arena position.</summary>
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

        int choice = _room.Random.Generate() & 7;
        state.Function = RidleyAttackChoices.Resolve(situation, choice);

        // The native selector tail-calls the chosen routine. Retain that same-frame setup
        // so timers, instruction changes, and velocity all begin on the selected frame.
        RunNorfairRidleyFunction(slot, state, samus, controllerInput, level);
    }

    /// <summary>Ports Lower Norfair Ridley's main AI.</summary>
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
    /// Ports hurt AI $A6:B297. Function, movement, wing, and tail updates execute only on
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
        if ((body.FrameCounter & 1) == 0)
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
        Interactions.UpdateNorfairRidleyHealthPalette(body, state);

        state.HurtMovementClamp = Math.Min(8, state.HurtMovementClamp + 1);
        if (state.GrabState != RidleyGrabState.Released && state.HurtMovementClamp >= 8 && samus is not null)
            NorfairRidleyCombat.UpdateNorfairRidleyGrabbedSamus(body, state, samus);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        // These are one-shot publications made by Ridley's current AI call, matching
        // the global QueueMusic/QueueSfx calls in bank $A6. Clear them at the same
        // enemy-frame boundary as every other Last* request above.
        if (_ridleyEncounter.Ridley is not null)
        {
            _ridleyEncounter.Ridley.LastDeathSoundEffect = null;
            _ridleyEncounter.Ridley.MusicRequest = null;
        }
    }

    /// <summary>Adds Ridley's hurt callback for the post-hit AI window.</summary>
    /// <remarks>$A6:B297 owns hurt frames instead of falling back to ordinary main AI. Movement/function work runs only
    /// on even actor frames, while tail projectile armor and hurt palettes remain live on every frame.</remarks>
    internal void AddRidleyHurtAi(EnemySharedReactions table) =>
        table.AddHurt(slot => slot.DefinitionId == EnemyDefinitionId.Ridley, (slot, frame) => RunNorfairRidleyHurt(
            slot,
            frame.Samus,
            frame.ControllerInput,
            frame.Level,
            frame.SamusProjectiles,
            frame.SharedProjectiles ?? throw new InvalidOperationException(
                "Ridley's power-bomb check ($A6:BD2C) requires the shared projectile owner."),
            frame.Camera));
}
