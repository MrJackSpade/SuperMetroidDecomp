using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Norfair Ridley's arrival and fight: hovering, swoops, the wall sweep, the pogo attack and grabbing Samus.</summary>
[RoomLifetime]
internal sealed class NorfairRidleyCombat
{
    /// <summary>Room services, level state, and RNG for the Norfair fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Publishes tail-impact tremors to the room earthquake state.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues roars, tail impacts, and other enemy sounds.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Owns fight state shared with cutscenes and room transitions.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>Advances Ridley's independent tail segments and their hit checks.</summary>
    private readonly RidleyTail _ridleyTail;
    /// <summary>Coordinates Ridley's dedicated projectile effects.</summary>
    private readonly RidleyProjectiles _ridleyProjectiles;
    /// <summary>Raises the arena liquid during Ridley's reveal.</summary>
    private readonly EnemyRoomFx _roomFx;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal NorfairRidleyCombat(NorfairRidleyCombatDependencies dependencies)
    {
        _room = dependencies.Room;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _ridleyEncounter = dependencies.RidleyEncounter;
        _ridleyTail = dependencies.RidleyTail;
        _ridleyProjectiles = dependencies.RidleyProjectiles;
        _roomFx = dependencies.RoomFx;
    }

    /// <summary>
    /// Runs the Norfair branch of the shared initializer. A defeated Ridley
    /// is deleted before any shared workspace or colour side effects, matching the
    /// defeated-boss gate; a live fight begins with Ridley below the visible arena.
    /// </summary>
    internal void InitializeNorfairRidley(RoomEnemyPlace place)
    {
        _ridleyEncounter.Parts.AttachBody(place, "Lower Norfair Ridley");

        if (_room.Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
        {
            place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
            place.Presence = place.Presence.With(EnemyPresence.Deleted | EnemyPresence.Hidden);
            return;
        }
        place.CollidesByFrameHitboxes = true;

        // Ridley's body stops a Plasma Beam from passing through him.
        place.Contact = place.Contact.With(EnemyContact.BlocksPlasmaBeam);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        place.Position = place.Position.WithWhole(new(96, 394));

        _ridleyEncounter.Ridley = new RidleyEnemyState
        {
            Phase = RidleyPhase.WaitForDoorTransition,
            FightMode = RidleyFightMode.NotFighting,
            MovementAnimationEnabled = true,
            FacingDirection = RidleyFacing.Right,
            IdleTailWhipEnabled = false,
            TailDamage = 120,
            MovementMinimum = new Position16<RoomPixels>(64, 64),
            MovementMaximum = new Position16<RoomPixels>(224, 416),
            WingBeatStep = 0,
            WingAnimationTimer = 0,
            WingAnimationTimerDelta = 0,
            TailAction = RidleyTailAction.None,
            TailAngleDelta = 1,
            TailMinimumClockwiseAngle = RidleyTailDefinitions.InitialMinimumClockwise,
            TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.InitialMaximumCounterClockwise,
            ClockwiseWhipTarget = null,
            CounterClockwiseWhipTarget = null,
            TailExtensionSpeed = RidleyTailDefinitions.StandardExtensionSpeed,
            IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentSpacing,
            TailSegments = RidleyTail.CreateInitialRidleyTailSegments(),
        };
        CeresRidleyFlight.StartRidleyAnimation(_ridleyEncounter.Ridley, place, RidleyBodySegment.Initial);

        // He arrives in his own colours, his body and tail hidden in black, and his arena hidden for its reveal.
        _ridleyEncounter.Ridley.Colours.ShowNorfairArrival();
    }

    /// <summary>
    /// The arena reveal advances a Norfair-only fourteen-color fade every third update.
    /// The step after the last colour step activates the shared tail/wing composition.
    /// </summary>
    internal void TickNorfairRidleyArenaReveal(RoomEnemyPlace place, RidleyEnemyState state)
    {
        if (--state.PhaseTimer >= 0)
            return;

        state.PhaseTimer = 2;
        int step = state.FadeStep;
        bool showsRevealStep;
        if (step > NorfairRidleyRevealDefinitions.RevealStepCount)
            throw new InvalidDataException($"Norfair Ridley reveal step {step} is past the reveal's end.");
        showsRevealStep = step < NorfairRidleyRevealDefinitions.RevealStepCount;
        if (showsRevealStep)
            state.Colours.ShowArenaReveal(step);
        state.FadeStep++;
        if (showsRevealStep)
            return;

        RidleyLiquidMotion.Publish(_roomFx, RidleyLiquidDefinitions.BattleHeight,
            RidleyLiquidDefinitions.RiseVelocity, RidleyLiquidDefinitions.RiseDelay);
        state.FadeStep = 0;
        CeresRidleyFlight.StartRidleyAnimation(state, place, RidleyBodySegment.TransitionToFlying);
        state.WingAnimationTimer = 8;
        state.WingAnimationTimerDelta = 8;
        foreach (RidleyTailSegment segment in state.TailSegments)
            segment.Active = true;
        state.TailAction = RidleyTailAction.Neutral;
        state.Phase = RidleyPhase.ClearVelocity;
    }

    /// <summary>Steers Ridley to the facing-dependent hover point until timeout or arrival.</summary>
    internal static void TickNorfairRidleyHover(RoomEnemyPlace place, RidleyEnemyState state)
    {
        if (NorfairRidleyMovement.TickRidleyPhaseTimer(state))
        {
            state.Phase = RidleyPhase.NorfairSelectAttack;
            return;
        }

        Position16<RoomPixels> target = new(state.FacingDirection != RidleyFacing.Left ? 96 : 192, 256);
        NorfairRidleyMovement.MoveNorfairRidleyToward(
            place,
            state,
            target,
            ReadRidleyHealthMovementDivisor(state));
        if (CeresRidleyFlight.IsWithinRidleyRectangle(place, target, new(8, 8)))
            state.Phase = RidleyPhase.NorfairSelectAttack;
    }

    /// <summary>Moves to the high side point and initializes the swoop angle timer.</summary>
    internal static void TickNorfairRidleySwoopMoveToStart(RoomEnemyPlace place, RidleyEnemyState state)
    {
        Position16<RoomPixels> target = new(state.FacingDirection != RidleyFacing.Left ? 64 : 192, 128);
        NorfairRidleyMovement.MoveNorfairRidleyToward(place, state, target, divisor: 15);
        if (!CeresRidleyFlight.IsWithinRidleyRectangle(place, target, new(8, 8)))
            return;

        state.Phase = RidleyPhase.NorfairSwoopAimDown;
        state.SwoopPhaseTimer = 32;
        state.SwoopAngleAccumulator = RotationAngle.Zero;
    }

    /// <summary>Advances the swoop velocity arc and changes phase when its timer expires.</summary>
    internal static bool TickNorfairRidleySwoopPhase(
        RidleyEnemyState state,
        RidleySwoopTurn turn,
        RotationAngle targetAngle,
        SignedEightEight targetSpeed,
        RidleyPhase nextPhase,
        int nextTimer)
    {
        CeresRidleyFlight.UpdateRidleySwoopVelocity(state, turn, targetAngle, targetSpeed);
        if (state.SwoopPhaseTimer != 0)
        {
            state.SwoopPhaseTimer--;
            return false;
        }

        state.Phase = nextPhase;
        state.SwoopPhaseTimer = nextTimer;
        return true;
    }

    /// <summary>
    /// Runs the wall sweep: Ridley sweeps between the walls behind and ahead of him at Samus's height while she spin
    /// jumps; any other movement starts the pogo attack at once.
    /// </summary>
    internal void TickNorfairRidleyWallSweep(
        RoomEnemyPlace place,
        RidleyEnemyState state,
        SamusState samus,
        RidleyWallSweepTarget wall)
    {
        RidleyFacing facing = state.FacingDirection;
        NorfairRidleyMovement.MoveNorfairRidleyToward(
            place,
            state,
            new Position16<RoomPixels>(
                wall == RidleyWallSweepTarget.WallBehind ? RidleyMovementTargets.WallBehindX(facing) : RidleyMovementTargets.WallAheadX(facing),
                int.Min(samus.Position.Y.Whole, 352)),
            ReadRidleyHealthMovementDivisor(state));
        state.TailWhipRequested = true;

        if (SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.SpinJumping)
        {
            BeginNorfairRidleyPogoAttack(state);
            return;
        }

        // The fireball choice uses the current random number; it does not draw a new one.
        if (RidleyRandomDefinitions.WallSweepFireballs.IsTakenBy(_room.Random.Read()) && !state.Roaring && state.FacingDirection != RidleyFacing.Turning)
            CeresRidleyFlight.StartRidleyAnimation(state, place, RidleyBodySegment.Fireballing);

        if (!NorfairRidleyMovement.TickRidleyPhaseTimer(state))
            return;

        state.Phase = wall == RidleyWallSweepTarget.WallBehind
            ? RidleyPhase.NorfairSweepToWallAhead
            : RidleyPhase.NorfairSweepToWallBehind;
        state.PhaseTimer = 128;
        SelectNorfairRidleyFacingAnimation(place, state);
    }

    /// <summary>Resets tail extension and starts the pogo attack by moving Ridley to its side of the arena.</summary>
    internal static void BeginNorfairRidleyPogoAttack(RidleyEnemyState state)
    {
        state.TailExtensionSpeed = RidleyTailDefinitions.StandardExtensionSpeed;
        state.IdealInterSegmentTailAngle = 16;
        state.TailAction = RidleyTailAction.Neutral;
        state.Phase = RidleyPhase.NorfairPogoMoveToSide;
    }

    /// <summary>Moves Ridley toward the facing-dependent side before descending to the pogo height.</summary>
    internal void TickNorfairRidleyPogoMoveToSide(
        RoomEnemyPlace place,
        RidleyEnemyState state,
        SamusState samus)
    {
        if (CoordinateAxis.Difference(place.Position.Y.Whole, 288) < 0)
        {
            SelectNorfairRidleyFacingAnimation(place, state);
            state.Phase = RidleyPhase.NorfairPogoMoveToHeight;
            state.PhaseTimer = 32;
            TickNorfairRidleyPogoMoveToHeight(place, state, samus);
            return;
        }

        NorfairRidleyMovement.MoveNorfairRidleyToward(
            place, state, new Position16<RoomPixels>(RidleyMovementTargets.PogoSideX(state.FacingDirection), 288), divisor: 16);
    }

    /// <summary>Settles at Y=288, sets the pogo tail, and starts the pogo's random time limit.</summary>
    internal void TickNorfairRidleyPogoMoveToHeight(
        RoomEnemyPlace place,
        RidleyEnemyState state,
        SamusState samus)
    {
        NorfairRidleyMovement.MoveNorfairRidleyToward(place, state, place.Position.Whole.WithY(288), divisor: 16);
        if (!NorfairRidleyMovement.TickRidleyPhaseTimer(state))
            return;

        state.TailAction = RidleyTailAction.PogoSetup;
        _ridleyTail.TickRidleyPogoTail(place, state, samus);
        InitializeNorfairRidleyPogoVelocity(state);
        state.Phase = RidleyPhase.NorfairPogoFall;
        state.PhaseTimer = RidleyRandomDefinitions.PogoTimer.Of(_room.Random.Read()) + 128;
    }

    /// <summary>Checks claw grab, accelerates downward, and bounces the tail on terrain contact.</summary>
    internal void TickNorfairRidleyPogoFall(
        RoomEnemyPlace place,
        RidleyEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (SamusMovementUsesRidleyGrab(samus) &&
            RidleyClawOverlapsSamus(place, state, samus, new Extent16<RoomPixels>(4, 4)))
        {
            // Every acceleration clamps Ridley's speed to a few pixels an update, so it always has a magnitude.
            state.Velocity = state.Velocity.WithY(-SignedEightEight.Max(NorfairRidleyMotionDefinitions.GrabLiftMinimumSpeed, state.Velocity.Y.Magnitude()));
            state.TailAction = RidleyTailAction.Neutral;
            state.TailAngleDelta = 1;
            BeginNorfairRidleyGrab(place, state, samus);
            return;
        }

        // The fall is clamped to six pixels an update, so adding the acceleration stays far inside the speed's range.
        state.Velocity = state.Velocity.WithY(SignedEightEight.Min(state.Velocity.Y + state.PogoDownwardAcceleration, NorfairRidleyMotionDefinitions.PogoMaximumFallSpeed));

        if (!RidleyTail.RidleyTailTouchesTerrain(state, level))
            return;

        RidleyTailSegment tip = state.TailSegments[6];
        _ridleyProjectiles.SpawnRidleyDust(tip.Position + new Offset16<RoomPixels>(0, 12), MiscDustAnimation.SmallDustCloud);
        _audio.QueueEnemySound(EnemySound.Quake, priority: SoundRequestPriority.Normal);
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.ForegroundAndBackground), 4);
        SetRidleyPogoHorizontalDirection(place, state, samus);
        InitializeNorfairRidleyPogoVelocity(state);
        for (int index = 0; index < state.TailSegments.Length; index++)
        {
            state.TailSegments[index].Distance = RidleyTailReach.RestDistance(index);
            state.TailSegments[index].TargetDistance = RidleyTailDefinitions.BounceDistance;
        }
        state.TailAction = RidleyTailAction.Pogo;
        state.PogoBounceCount++;
        if (state.PogoBounceCount >= 2)
        {
            state.PogoBounceCount = 0;
            if (state.FacingDirection != RidleyFacing.Turning)
                CeresRidleyFlight.StartRidleyAnimation(state, place, RidleyBodySegment.Fireballing);
        }
        state.Phase = RidleyPhase.NorfairPogoRise;
    }

    /// <summary>Rises after a pogo bounce or returns to attack selection.</summary>
    internal static void TickNorfairRidleyPogoRise(
        RidleyEnemyState state,
        SamusState samus)
    {
        if (samus.Position.Y.Whole < 352 || NorfairRidleyMovement.TickRidleyPhaseTimer(state))
        {
            state.TailAction = RidleyTailAction.Neutral;
            state.TailAngleDelta = 1;
            state.Phase = RidleyPhase.NorfairSelectAttack;
            return;
        }

        // The rise starts at its launch speed and slows toward rest, so adding the acceleration never passes the speed's
        // range.
        SignedEightEight nextVertical = state.Velocity.Y + state.PogoUpwardAcceleration;
        if (!nextVertical.IsNegative)
        {
            state.Velocity = state.Velocity.WithY(SignedEightEight.Zero);
            state.Phase = RidleyPhase.NorfairPogoFall;
        }
        else
        {
            state.Velocity = state.Velocity.WithY(nextVertical);
        }
    }

    /// <summary>
    /// Launches a pogo bounce: one of the health band's four launches, chosen by the current random number. The sign of
    /// the previous horizontal velocity chooses the bounce's direction.
    /// </summary>
    internal void InitializeNorfairRidleyPogoVelocity(RidleyEnemyState state)
    {
        // The pogo choice samples the current random number; it does not advance shared RNG.
        int pattern = RidleyRandomDefinitions.PogoChoice.Of(_room.Random.Read());
        RidleyPogoDefinition definition = state.HealthBand.PogoLaunches[pattern];
        state.PogoUpwardAcceleration = definition.UpwardAcceleration;
        state.PogoDownwardAcceleration = definition.DownwardAcceleration;
        state.Velocity = state.Velocity.WithY(definition.Velocity.Y);
        SignedEightEight horizontal = definition.Velocity.X;
        state.Velocity = state.Velocity.WithX(state.Velocity.X.IsNegative
            ? horizontal.Negated()
            : horizontal);
    }

    /// <summary>Lunges toward a grabbable Samus, ending on passage, boundary, height, or claw contact.</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="state">Ridley's state.</param>
    /// <param name="samus">Samus, whom the lunge grabs.</param>
    /// <param name="powerBomb">The power bomb explosion of the pass's bombs, which a missed lunge dodges.</param>
    internal static void TickNorfairRidleyGrabApproach(
        RoomEnemyPlace place,
        RidleyEnemyState state,
        SamusState samus,
        SamusPowerBombExplosionState powerBomb)
    {
        if (!SamusMovementUsesRidleyGrab(samus))
        {
            HandleNorfairRidleyMissedLunge(place, state, powerBomb);
            return;
        }

        // The lunge is a passing attack, not indefinite pursuit. Test the
        // previous movement's boundary result before applying another acceleration.
        int deltaX = CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole);
        bool passedSamus = state.FacingDirection == RidleyFacing.Right ? deltaX >= 0 : deltaX < 0;
        if (state.HitRoomBoundary ||
            (passedSamus && CoordinateAxis.Difference(Math.Abs(deltaX), RidleyLungeDefinitions.PassDistance) >= 0) ||
            CoordinateAxis.AsSigned(place.Position.Y.Whole + RidleyLungeDefinitions.ClawHeight - samus.Position.Y.Whole) >= 0)
        {
            HandleNorfairRidleyMissedLunge(place, state, powerBomb);
            return;
        }

        int sideOffset = state.FacingDirection != RidleyFacing.Left ? 16 : -16;
        NorfairRidleyMovement.MoveNorfairRidleyToward(
            place,
            state,
            samus.Position.Whole + new Offset16<RoomPixels>(CoordinateAxis.AsSigned(sideOffset), -4),
            state.HealthBand.GrabDivisor);

        if (RidleyClawOverlapsSamus(place, state, samus, new Extent16<RoomPixels>(8, 12)))
        {
            // The missed lunge reverses before the carry setup accelerates.
            state.Velocity = state.Velocity.WithY(state.Velocity.Y.Negated());
            BeginNorfairRidleyGrab(place, state, samus);
        }
    }

    /// <summary>Locks Samus to the claw and enters carry or the zero-health death approach.</summary>
    private static void BeginNorfairRidleyGrab(
        RoomEnemyPlace place,
        RidleyEnemyState state,
        SamusState samus)
    {
        state.GrabOffset = samus.Position.Whole - new Position16<RoomPixels>(GetNorfairRidleyClawX(place, state), GetNorfairRidleyClawY(place, state));
        state.GrabState = RidleyGrabState.CarryingSamus;
        samus.Control.LockStationary();
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        if (place.Health == 0)
        {
            StartNorfairRidleyDeathSequence(place, state);
            // The grab tail-jumps to the move-to-death-spot phase on the same successful zero-health grab.
            TickNorfairRidleyMoveToDeathSpot(place, state);
            return;
        }
        // The grab falls through into the carry on the grabbing update.
        BeginNorfairRidleyCarry(place, state);
    }

    /// <summary>Chooses a facing-dependent carry anchor and starts Ridley's 32-frame move.</summary>
    internal static void BeginNorfairRidleyCarry(RoomEnemyPlace place, RidleyEnemyState state)
    {
        state.Target = new Position16<RoomPixels>(
            RidleyMovementTargets.CarryAnchorX(state.FacingDirection),
            CoordinateAxis.Difference(place.Position.Y.Whole, 320) < 0
                ? 256
                : CoordinateAxis.Wrap(place.Position.Y.Whole - 64));
        state.Phase = RidleyPhase.NorfairCarryMoveToAnchor;
        state.PhaseTimer = 32;
        NorfairRidleyMovement.MoveNorfairRidleyToward(place, state, state.Target, 16);
        NorfairRidleyMovement.TickRidleyPhaseTimer(state);
    }

    /// <summary>Decays Samus's grab offset and positions her at Ridley's moving claw.</summary>
    internal static void UpdateNorfairRidleyGrabbedSamus(
        RoomEnemyPlace place,
        RidleyEnemyState state,
        SamusState samus)
    {
        state.GrabOffset = new Offset16<RoomPixels>(
            DecayRidleyGrabOffset(state.GrabOffset.X),
            DecayRidleyGrabOffset(state.GrabOffset.Y));
        samus.Position = samus.Position.WithWhole(
            new Position16<RoomPixels>(GetNorfairRidleyClawX(place, state), GetNorfairRidleyClawY(place, state)) +
            state.GrabOffset);
    }

    /// <summary>Moves one signed grab-offset component four pixels toward zero.</summary>
    private static int DecayRidleyGrabOffset(int offset)
    {
        if (offset > 0)
            return CoordinateAxis.AsSigned(Math.Max(0, offset - 4));
        if (offset < 0)
            return CoordinateAxis.AsSigned(Math.Min(0, offset + 4));
        return 0;
    }

    /// <summary>Tests Samus's radius against the facing and foot-frame adjusted claw point.</summary>
    internal static bool RidleyClawOverlapsSamus(
        RoomEnemyPlace place,
        RidleyEnemyState state,
        SamusState samus,
        Extent16<RoomPixels> radius) =>
        CoordinateAxis.Distance(samus.Position.X.Whole, GetNorfairRidleyClawX(place, state)) <
            samus.Kinematics.Radius.X + radius.X &&
        CoordinateAxis.Distance(samus.Position.Y.Whole, GetNorfairRidleyClawY(place, state)) <
            samus.Kinematics.Radius.Y + radius.Y;

    /// <summary>Applies the facing-dependent claw X offset to Ridley's body position.</summary>
    private static int GetNorfairRidleyClawX(RoomEnemyPlace place, RidleyEnemyState state) =>
        CoordinateAxis.Wrap(place.Position.X.Whole + RidleyClawOffsets.ReadX(state.FacingDirection));

    /// <summary>Applies the current feet-frame claw Y offset to Ridley's body position.</summary>
    private static int GetNorfairRidleyClawY(RoomEnemyPlace place, RidleyEnemyState state) =>
        CoordinateAxis.Wrap(place.Position.Y.Whole + RidleyClawOffsets.ReadY(state.ClawHeight));

    /// <summary>Starts a turn only when Ridley faces away from the half of its 256-pixel screen his X lies in.</summary>
    internal static void SelectNorfairRidleyFacingAnimation(RoomEnemyPlace place, RidleyEnemyState state)
    {
        if (state.FacingDirection == RidleyFacing.Turning)
            return;
        // The facing test reads whether X lies in the right half of its 256-pixel screen,
        // not the sign of the full coordinate. Keep an inward
        // facing actor's current animation and timer untouched.
        bool rightHalf = RoomGridConversions.OffsetWithinScreen(place.Position.X.Whole) >= 128;
        if (state.FacingDirection == RidleyFacing.Left ? rightHalf : !rightHalf)
            return;
        CeresRidleyFlight.StartRidleyAnimation(
            state,
            place,
            state.FacingDirection == RidleyFacing.Left
                ? RidleyBodySegment.TurnFromLeftToRight
                : RidleyBodySegment.TurnFromRightToLeft);
        place.AnimationClock.HoldFor(2);
    }

    /// <summary>The hover acceleration divisor of Ridley's health band.</summary>
    internal static int ReadRidleyHealthMovementDivisor(RidleyEnemyState state) => state.HealthBand.HoverDivisor;

    /// <summary>Checks the validated pose movement type against Ridley's grabbable states.</summary>
    internal static bool SamusMovementUsesRidleyGrab(SamusState samus)
    {
        SamusMovementType movement = SamusPoseMetadata.ReadMovementType(samus);
        return RidleyGrabs.CanGrab(movement);
    }

    /// <summary>
    /// Lunges when a power bomb is active and Ridley is not holding Samus, run before the phase on
    /// every main and even hurt frame. It reads the power-bomb state, which is set as soon as a power bomb is
    /// laid, so Ridley lunges in the same frame Samus lays one.
    /// </summary>
    internal static void PrepareNorfairRidleyCombatFrame(
        RidleyEnemyState state,
        SamusBombProjectileSystem sharedProjectiles)
    {
        if (state.FightMode is RidleyFightMode.NotFighting or RidleyFightMode.Dying or RidleyFightMode.PowerBombDodge)
            return;
        if (!sharedProjectiles.PowerBombExplosion.IsArmed || state.GrabState != RidleyGrabState.Released)
            return;

        // The power-bomb lunge resets both tail controls before the lunge.
        state.TailAction = RidleyTailAction.Neutral;
        state.TailAngleDelta = 1;
        state.Phase = RidleyPhase.NorfairGrabApproach;
    }

    /// <summary>On zero health, makes Ridley intangible and enters the Samus-release death state.</summary>
    private static void StartNorfairRidleyDeathSequence(
        RoomEnemyPlace body,
        RidleyEnemyState state)
    {
        if (body.Health != 0 || state.FightMode == RidleyFightMode.Dying)
            return;

        state.FightMode = RidleyFightMode.Dying;
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.Phase = RidleyPhase.NorfairMoveToDeathSpot;
    }

    /// <summary>
    /// Keeps the already-grabbed Samus attached while accelerating Ridley toward
    /// the four-pixel rectangle centered at (128,328). Reaching it goes straight on to
    /// the roar setup instead of spending an extra transition frame.
    /// </summary>
    internal static void TickNorfairRidleyMoveToDeathSpot(
        RoomEnemyPlace body,
        RidleyEnemyState state)
    {
        NorfairRidleyMovement.MoveNorfairRidleyToward(body, state, new Position16<RoomPixels>(128, 328),
            RidleyInertiaDefinitions.DeathDivisor, RidleyInertiaDefinitions.DeathReversalBoost);
        if (CeresRidleyFlight.IsWithinRidleyRectangle(body, new(128, 328), new(4, 4)))
            BeginNorfairRidleyDeathRoar(body, state);
    }

    /// <summary>Selects the death-roar animation and arms its 32-frame move.</summary>
    internal static void BeginNorfairRidleyDeathRoar(
        RoomEnemyPlace body,
        RidleyEnemyState state)
    {
        CeresRidleyFlight.StartRidleyAnimation(state, body, RidleyBodySegment.DeathRoar);
        state.Phase = RidleyPhase.NorfairDeathRoar;
        state.PhaseTimer = 32;
    }

    /// <summary>Handles a missed lunge: death has priority over power-bomb dodge and ordinary retreat.</summary>
    private static void HandleNorfairRidleyMissedLunge(RoomEnemyPlace place, RidleyEnemyState state, SamusPowerBombExplosionState powerBomb)
    {
        if (state.ZeroHealthLungeCount >= RidleyLungeDefinitions.DeathLungeLimit)
        {
            StartNorfairRidleyDeathSequence(place, state);
            BeginNorfairRidleyDeathRoar(place, state);
            return;
        }
        if (powerBomb.IsArmed)
        {
            state.Phase = RidleyPhase.NorfairPowerBombDodge;
            TickNorfairRidleyPowerBombDodge(place, state, powerBomb);
            return;
        }
        state.Phase = RidleyPhase.NorfairHoverSetup;
        state.TailWhipRequested = true;
    }

    /// <summary>Runs the power-bomb branch of missed-lunge recovery.</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="state">Ridley's state.</param>
    /// <param name="bomb">The power bomb explosion of the pass's bombs, which Ridley dodges while it is armed.</param>
    internal static void TickNorfairRidleyPowerBombDodge(RoomEnemyPlace place, RidleyEnemyState state, SamusPowerBombExplosionState bomb)
    {
        if (!bomb.IsArmed)
        {
            state.FightMode = RidleyFightMode.Fighting;
            state.Phase = state.GrabState == RidleyGrabState.Released
                ? RidleyPhase.NorfairSelectAttack : RidleyPhase.NorfairCarrySetup;
            return;
        }
        state.FightMode = RidleyFightMode.PowerBombDodge;
        Position16<RoomPixels> target = new(
            CoordinateAxis.Difference(bomb.Position.X, RidleyLungeDefinitions.DodgeSplitX) >= 0
                ? RidleyLungeDefinitions.DodgeLeftX : RidleyLungeDefinitions.DodgeRightX,
            CoordinateAxis.Difference(bomb.Position.Y, RidleyLungeDefinitions.DodgeSplitY) >= 0
                ? RidleyLungeDefinitions.DodgeUpperY : RidleyLungeDefinitions.DodgeLowerY);
        NorfairRidleyMovement.MoveNorfairRidleyToward(place, state, target, ReadRidleyHealthMovementDivisor(state));
    }

    /// <summary>Chooses the next bounce's direction from position and the current seed.</summary>
    internal void SetRidleyPogoHorizontalDirection(RoomEnemyPlace place, RidleyEnemyState state, SamusState samus)
    {
        if (state.Velocity.X.IsZero)
            state.Velocity = state.Velocity.WithX(RoomGridConversions.OffsetWithinScreen(place.Position.X.Whole) < 128 ? NorfairRidleyMotionDefinitions.PogoRestartSpeed : -NorfairRidleyMotionDefinitions.PogoRestartSpeed);
        bool negative = state.Velocity.X.IsNegative;
        bool reverse;
        if (CoordinateAxis.Difference(place.Position.X.Whole, state.MovementMinimum.X) < 0) reverse = negative;
        else if (CoordinateAxis.Difference(place.Position.X.Whole, state.MovementMaximum.X) >= 0) reverse = !negative;
        else
        {
            bool headsTowardSamus = CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole) < 0 != state.Velocity.X.IsNegative;
            bool headsAway = RidleyRandomDefinitions.PogoBounceHeadsAwayFromSamus.IsTakenBy(_room.Random.Read());
            reverse = headsTowardSamus ? headsAway : !headsAway;
        }
        if (reverse) state.Velocity = state.Velocity.WithX(state.Velocity.X.Negated());
    }
}
