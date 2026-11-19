using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Lower Norfair Ridley's encounter (#2229): his reveal's Lower Norfair timings and arena reveal, his fight, his death and
/// his breakup pieces. His body runs the parts both Ridleys share (<see cref="RidleyBody"/>); none runs through Ceres
/// Ridley's encounter.
/// </summary>
[RoomLifetime]
internal sealed class NorfairRidleyBehavior : IEnemySpecies, IRidleyRevealEncounter<NorfairRidleyState>
{
    /// <summary>Room services, level state, and RNG for the Norfair fight.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>The parts of his body both Ridleys run.</summary>
    internal RidleyBody Body { get; }

    /// <summary>Norfair Ridley's arrival and fight: hovering, swoops, the wall sweep, the pogo attack and grabbing Samus.</summary>
    internal NorfairRidleyCombat Combat { get; }

    /// <summary>Norfair Ridley's death: the roar, breakup, explosions and the drop.</summary>
    internal NorfairRidleyDeath Death { get; }

    /// <summary>Wires Ridley's fight, movement, tail, projectile, audio, and room collaborators.</summary>
    internal NorfairRidleyBehavior(NorfairRidleyBehaviorDependencies dependencies)
    {
        Body = dependencies.RidleyBody;
        _room = dependencies.Room;
        Combat = new NorfairRidleyCombat(new NorfairRidleyCombatDependencies { Room = _room, Earthquake = dependencies.Earthquake, Audio = dependencies.Audio, RidleyEncounter = dependencies.RidleyEncounter, RidleyTail = Body.Tail, RidleyProjectiles = dependencies.RidleyProjectiles, RoomFx = dependencies.RoomFx, Presentation = dependencies.Presentation });
        Death = new NorfairRidleyDeath(dependencies.Lifecycle, _room, dependencies.DeathsAndDrops, dependencies.RidleyProjectiles, dependencies.RoomFx, dependencies.Audio);
    }

    /// <summary>Lower Norfair Ridley's wait once Samus's door has finished, its first update the one the door finishes on.</summary>
    public int InitialDelayUpdates => NorfairRidleyRevealDefinitions.RevealDelayUpdates;

    /// <summary>Begins Lower Norfair Ridley's hold after his roar: his arena's reveal, which takes its first step on its first update.</summary>
    /// <param name="ridley">His encounter.</param>
    public void BeginRoarHold(NorfairRidleyState ridley) => Ensure.NotNull(ridley).BeginArenaReveal();

    /// <summary>Runs one update of his arena's reveal, which he holds his roar through; he takes off once it ends.</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="ridley">His encounter.</param>
    public bool TakesOff(RoomEnemyPlace place, NorfairRidleyState ridley) => Combat.TickNorfairRidleyArenaReveal(ridley);

    /// <summary>
    /// Runs Norfair Ridley for one update, including the reveal phases he shares with
    /// Ceres Ridley. Composition is advanced after the selected phase, so a velocity change affects the same rendered
    /// frame.
    /// </summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="state">His encounter, which his body holds.</param>
    /// <param name="frame">The AI pass.</param>
    internal void RunRidleyMainAi(RoomEnemyPlace place, NorfairRidleyState state, IEnemyAiFrame frame)
    {
        SamusState samus = frame.Samus;
        SamusProjectileSystem samusProjectiles = frame.SamusProjectiles;
        SamusBombProjectileSystem sharedProjectiles = frame.SharedProjectiles;
        state.HurtCarryLag.RecoverOnOrdinaryUpdate();

        RidleyInteractions.UpdateNorfairRidleyIntangibility(place, state, frame.Camera);
        NorfairRidleyCombat.PrepareNorfairRidleyCombatFrame(state, sharedProjectiles);

        RunNorfairRidleyPhase(place, state, samus, frame.ControllerInput, frame.Level, sharedProjectiles.PowerBombExplosion);

        if (state.MovementAnimationEnabled)
        {
            RidleyBodyContact.UpdateHurtFlash(place, state, place.FrameCounter);
            Body.Flight.IntegrateMovement(place, state.Flight);
            RidleyFlight.TickWingAnimation(state.Flight);
            Body.Tail.TickRidleyTail(place, state, samus, samusProjectiles);
            Body.Tail.ResolveRidleyTailProjectileHits(place, state.Tail, samusProjectiles);
            // Main places carried Samus after body/tail movement.
            if (state.GrabState != RidleyGrabState.Released)
                NorfairRidleyCombat.UpdateNorfairRidleyGrabbedSamus(place, state, samus);
        }

        RidleyInteractions.UpdateNorfairRidleyHealthBand(place, state);
    }

    /// <summary>Runs the current Norfair reveal, flight, swoop, wall sweep, pogo, grab or death phase.</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="state">Ridley's state.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="controllerInput">The actions held this update.</param>
    /// <param name="level">The room's terrain.</param>
    /// <param name="powerBomb">The power bomb explosion of the pass's bombs, which a missed lunge dodges.</param>
    private void RunNorfairRidleyPhase(
        RoomEnemyPlace place,
        NorfairRidleyState state,
        SamusState samus,
        GameplayAction controllerInput,
        RoomLevelData level,
        SamusPowerBombExplosionState powerBomb)
    {
        if (!state.LiftedOff)
            Body.Reveal.Run(place, state, this);
        else
            RunNorfairRidleyFightPhase(place, state, samus, controllerInput, level, powerBomb);
    }

    /// <summary>Runs the Lower Norfair flight, swoop, wall sweep, pogo, grab or death phase Ridley is in once he has lifted off.</summary>
    private void RunNorfairRidleyFightPhase(
        RoomEnemyPlace place,
        NorfairRidleyState state,
        SamusState samus,
        GameplayAction controllerInput,
        RoomLevelData level,
        SamusPowerBombExplosionState powerBomb)
    {
        switch (state.Phase)
        {
            case NorfairRidleyPhase.EnterArena:
                NorfairRidleyMovement.MoveNorfairRidleyToward(place, state.Flight, NorfairRidleyArenaDefinitions.ArenaEntry, divisor: NorfairRidleyMotionDefinitions.ArenaEntryDivisor);
                if (RidleyFlight.IsWithinRectangle(place, NorfairRidleyArenaDefinitions.ArenaEntry, NorfairRidleyArenaDefinitions.FlightTargetReach))
                {
                    state.FightMode = RidleyFightMode.Fighting;
                    state.Phase = NorfairRidleyPhase.SelectAttack;
                    SelectNorfairRidleyAttack(place, state, samus, controllerInput, level, powerBomb);
                }
                return;

            case NorfairRidleyPhase.SelectAttack:
                SelectNorfairRidleyAttack(place, state, samus, controllerInput, level, powerBomb);
                return;

            case NorfairRidleyPhase.HoverSetup:
                state.Phase = NorfairRidleyPhase.Hover;
                state.PhaseTimer.LastFor(NorfairRidleyMotionDefinitions.HoverUpdates);
                NorfairRidleyCombat.TickNorfairRidleyHover(place, state);
                return;

            case NorfairRidleyPhase.Hover:
                NorfairRidleyCombat.TickNorfairRidleyHover(place, state);
                return;

            case NorfairRidleyPhase.SwoopSetup:
                state.Phase = NorfairRidleyPhase.SwoopMoveToStart;
                state.Flight.SwoopAngleAccumulator = RotationAngle.Zero;
                NorfairRidleyCombat.TickNorfairRidleySwoopMoveToStart(place, state);
                return;

            case NorfairRidleyPhase.SwoopMoveToStart:
                NorfairRidleyCombat.TickNorfairRidleySwoopMoveToStart(place, state);
                return;

            case NorfairRidleyPhase.SwoopAimDown:
                NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                    state,
                    SwoopTurn(state, NorfairRidleyMotionDefinitions.SwoopDiveTurn),
                    SwoopTarget(state, NorfairRidleyMotionDefinitions.SwoopDiveHeading),
                    NorfairRidleyMotionDefinitions.SwoopDiveSpeed,
                    NorfairRidleyPhase.SwoopAimSideways,
                    NorfairRidleyMotionDefinitions.SwoopSweepUpdates);
                return;

            case NorfairRidleyPhase.SwoopAimSideways:
                if (NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                        state,
                        SwoopTurn(state, NorfairRidleyMotionDefinitions.SwoopSweepTurn),
                        SwoopTarget(state, RotationAngle.QuarterTurn),
                        NorfairRidleyMotionDefinitions.SwoopSweepSpeed,
                        NorfairRidleyPhase.SwoopAimUp,
                        NorfairRidleyMotionDefinitions.SwoopTurnUpdates))
                {
                    state.Tail.WhipRequested = true;
                }
                return;

            case NorfairRidleyPhase.SwoopAimUp:
                NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                    state,
                    SwoopTurn(state, NorfairRidleyMotionDefinitions.SwoopTurnUpTurn),
                    SwoopTarget(state, RidleyFlight.SwoopClimbTarget),
                    NorfairRidleyMotionDefinitions.SwoopClimbSpeed,
                    NorfairRidleyPhase.SwoopClimb,
                    NorfairRidleyMotionDefinitions.SwoopClimbUpdates);
                return;

            case NorfairRidleyPhase.SwoopClimb:
                if (NorfairRidleyCombat.TickNorfairRidleySwoopPhase(
                        state,
                        SwoopTurn(state, NorfairRidleyMotionDefinitions.SwoopClimbTurn),
                        SwoopTarget(state, RidleyFlight.SwoopClimbTarget),
                        NorfairRidleyMotionDefinitions.SwoopClimbSpeed,
                        NorfairRidleyPhase.SwoopRecover,
                        NorfairRidleyMotionDefinitions.SwoopRecoveryUpdates))
                {
                    NorfairRidleyCombat.SelectNorfairRidleyFacingAnimation(place, state);
                }
                return;

            case NorfairRidleyPhase.SwoopRecover:
                RidleyFlight.UpdateSwoopVelocity(state.Flight, TurnStep.None, RotationAngle.HalfTurn, NorfairRidleyMotionDefinitions.SwoopRecoverySpeed);
                if (state.SwoopPhaseTimer.StepKeepsRunning())
                    return;
                state.Phase = NorfairRidleyCombat.SamusMovementUsesRidleyGrab(samus)
                    ? NorfairRidleyPhase.GrabApproach
                    : NorfairRidleyPhase.SelectAttack;
                return;

            case NorfairRidleyPhase.WallSweepSetup:
                state.Tail.IdealLinkSpacing = RidleyTailDefinitions.WallSweepLinkSpacing;
                state.Tail.ExtensionSpeed = RidleyTailDefinitions.PogoExtensionSpeed;
                state.Phase = NorfairRidleyPhase.SweepToWallBehind;
                state.PhaseTimer.LastFor(RidleyRandomDefinitions.WallSweepTimer.Of(_room.Random.Read()) + NorfairRidleyMotionDefinitions.WallSweepFirstLegLeastUpdates);
                Combat.TickNorfairRidleyWallSweep(place, state, samus, RidleyWallSweepTarget.WallBehind);
                return;

            case NorfairRidleyPhase.SweepToWallBehind:
                Combat.TickNorfairRidleyWallSweep(place, state, samus, RidleyWallSweepTarget.WallBehind);
                return;

            case NorfairRidleyPhase.SweepToWallAhead:
                Combat.TickNorfairRidleyWallSweep(place, state, samus, RidleyWallSweepTarget.WallAhead);
                return;

            case NorfairRidleyPhase.PogoMoveToSide:
                Combat.TickNorfairRidleyPogoMoveToSide(place, state, samus);
                return;

            case NorfairRidleyPhase.PogoMoveToHeight:
                Combat.TickNorfairRidleyPogoMoveToHeight(place, state, samus);
                return;

            case NorfairRidleyPhase.PogoFall:
                Combat.TickNorfairRidleyPogoFall(place, state, samus, level);
                return;

            case NorfairRidleyPhase.PogoRise:
                NorfairRidleyCombat.TickNorfairRidleyPogoRise(state, samus);
                return;

            case NorfairRidleyPhase.GrabApproach:
                NorfairRidleyCombat.TickNorfairRidleyGrabApproach(place, state, samus, powerBomb);
                return;

            case NorfairRidleyPhase.PowerBombDodge:
                NorfairRidleyCombat.TickNorfairRidleyPowerBombDodge(place, state, powerBomb);
                return;

            case NorfairRidleyPhase.CarrySetup:
                NorfairRidleyCombat.BeginNorfairRidleyCarry(place, state);
                return;

            case NorfairRidleyPhase.CarryMoveToAnchor:
                NorfairRidleyMovement.MoveNorfairRidleyToward(place, state.Flight, state.Target, NorfairRidleyMotionDefinitions.CarryDivisor);
                if (state.PhaseTimer.StepRunsOut())
                {
                    state.Phase = NorfairRidleyPhase.CarryRise;
                    state.PhaseTimer.LastFor(NorfairRidleyMotionDefinitions.CarryRiseUpdates);
                }
                return;

            case NorfairRidleyPhase.CarryRise:
                if (state.PhaseTimer.StepRunsOut())
                {
                    state.Tail.IdealLinkSpacing = RidleyTailDefinitions.CarryReleaseLinkSpacing;
                    state.Tail.ExtensionSpeed = RidleyTailDefinitions.StandardExtensionSpeed;
                    NorfairRidleyDeath.ReleaseNorfairRidleyGrab(state, samus);
                    state.Phase = NorfairRidleyPhase.CarryRelease;
                    state.PhaseTimer.LastFor(NorfairRidleyMotionDefinitions.CarryReleaseUpdates);
                }
                else
                    NorfairRidleyMovement.MoveNorfairRidleyToward(place, state.Flight, state.Target.WithY(NorfairRidleyArenaDefinitions.FlightLineY), NorfairRidleyMotionDefinitions.CarryDivisor);
                return;

            case NorfairRidleyPhase.CarryRelease:
                if (state.PhaseTimer.StepRunsOut())
                {
                    state.Tail.IdealLinkSpacing = RidleyTailDefinitions.StandardLinkSpacing;
                    state.Tail.ExtensionSpeed = RidleyTailDefinitions.StandardExtensionSpeed;
                    state.Phase = NorfairRidleyPhase.SelectAttack;
                }
                else
                {
                    NorfairRidleyMovement.MoveNorfairRidleyToward(
                        place, state.Flight, new Position16<RoomPixels>(RidleyMovementTargets.CarryRelease.For(state.Flight.FacingDirection), NorfairRidleyArenaDefinitions.CarryReleaseY), NorfairRidleyMotionDefinitions.CarryDivisor);
                }
                return;

            case NorfairRidleyPhase.MoveToDeathSpot:
                NorfairRidleyCombat.TickNorfairRidleyMoveToDeathSpot(place, state);
                return;

            case NorfairRidleyPhase.DeathRoar:
                Death.TickNorfairRidleyDeathRoar(place, state, samus);
                return;

            case NorfairRidleyPhase.DeathExplosions:
                Death.TickNorfairRidleyDeathExplosions(place, state, samus);
                return;

            case NorfairRidleyPhase.DeathBreakup:
                Death.BeginNorfairRidleyBreakup(place, state);
                return;

            case NorfairRidleyPhase.DeathWait:
                NorfairRidleyDeath.TickNorfairRidleyBreakupWait(state);
                return;

            case NorfairRidleyPhase.DeathFinish:
                Death.TickNorfairRidleyDeathFinish(place, state, samus);
                return;

            case NorfairRidleyPhase.DeathComplete:
                return;

            default:
                throw new InvalidDataException($"Undefined {nameof(NorfairRidleyPhase)} {state.Phase}.");
        }
    }

    /// <summary>Chooses an attack situation by Samus's movement, Ridley's health and Samus's height in the arena.</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="state">Ridley's state.</param>
    /// <param name="samus">Samus.</param>
    /// <param name="controllerInput">The actions held this update.</param>
    /// <param name="level">The room's terrain.</param>
    /// <param name="powerBomb">The power bomb explosion of the pass's bombs, which the chosen phase may read at once.</param>
    private void SelectNorfairRidleyAttack(
        RoomEnemyPlace place,
        NorfairRidleyState state,
        SamusState samus,
        GameplayAction controllerInput,
        RoomLevelData level,
        SamusPowerBombExplosionState powerBomb)
    {
        RidleyAttackSituation situation;
        if (SamusPoseMetadata.ReadMovementType(samus) == SamusMovementType.SpinJumping)
        {
            situation = RidleyAttackSituation.SpinJumping;
        }
        else if (place.Health == 0)
        {
            situation = RidleyAttackSituation.ZeroHealth;
            state.ZeroHealthLungeCount++;
        }
        else if (place.Health < NorfairRidleyMotionDefinitions.WoundedAttackHealth)
        {
            situation = RidleyAttackSituation.Ordinary;
        }
        else if (samus.Position.Y.Whole >= NorfairRidleyArenaDefinitions.PogoZoneTopY)
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

        state.Phase = RidleyAttackChoices.Resolve(situation, _room.Random.Generate());

        // The choice runs the chosen phase at once. Retain that same-frame setup
        // so timers, animation changes, and velocity all begin on the selected frame.
        RunNorfairRidleyPhase(place, state, samus, controllerInput, level, powerBomb);
    }

    /// <summary>
    /// Updates Ridley while he is hurt. Phase, movement, wing, and tail updates run only on
    /// even actor frames; tail armor, flash/colour work, and the grabbed-Samus lag counter
    /// remain active on every hurt frame.
    /// </summary>
    /// <param name="body">Ridley's body.</param>
    /// <param name="state">His encounter, which his body holds.</param>
    /// <param name="frame">The hurt pass.</param>
    internal void RunRidleyHurt(RoomEnemyPlace body, NorfairRidleyState state, RoomEnemyFrame frame)
    {
        SamusState samus = frame.Samus;
        SamusProjectileSystem samusProjectiles = frame.SamusProjectiles;
        SamusBombProjectileSystem sharedProjectiles = frame.SharedProjectiles;
        if (RidleyCadenceDefinitions.NorfairRidleyActsEverySecondUpdate.IsDue(body.FrameCounter))
        {
            RidleyInteractions.UpdateNorfairRidleyIntangibility(body, state, frame.Camera);
            NorfairRidleyCombat.PrepareNorfairRidleyCombatFrame(state, sharedProjectiles);
            RunNorfairRidleyPhase(body, state, samus, frame.ControllerInput, frame.Level, sharedProjectiles.PowerBombExplosion);
            if (state.MovementAnimationEnabled)
            {
                Body.Flight.IntegrateMovement(body, state.Flight);
                RidleyFlight.TickWingAnimation(state.Flight);
                Body.Tail.TickRidleyTail(body, state, samus, samusProjectiles);
            }
        }

        if (state.MovementAnimationEnabled)
            Body.Tail.ResolveRidleyTailProjectileHits(body, state.Tail, samusProjectiles);
        RidleyBodyContact.UpdateHurtFlash(body, state, body.FrameCounter);
        RidleyInteractions.UpdateNorfairRidleyHealthBand(body, state);

        state.HurtCarryLag.BuildOnHurtUpdate();
        if (state.GrabState != RidleyGrabState.Released && state.HurtCarryLag.IsFull)
            NorfairRidleyCombat.UpdateNorfairRidleyGrabbedSamus(body, state, samus);
    }

    /// <summary>
    /// The heading a Norfair swoop turns toward: <paramref name="rightward"/> for a Ridley facing right or forward, and
    /// the same turn the other way round from zero for one facing left.
    /// </summary>
    private static RotationAngle SwoopTarget(NorfairRidleyState state, RotationAngle rightward) =>
        state.Flight.FacingDirection != RidleyFacing.Left ? rightward : RotationAngle.Zero - rightward;

    /// <summary>
    /// A Norfair swoop's turn of <paramref name="perUpdate"/> each update: counterclockwise, toward the right, for a Ridley
    /// facing right or forward, and clockwise for one facing left.
    /// </summary>
    private static TurnStep SwoopTurn(NorfairRidleyState state, RotationAngle perUpdate) =>
        new(perUpdate, state.Flight.FacingDirection != RidleyFacing.Left ? RotationDirection.Counterclockwise : RotationDirection.Clockwise);
}
