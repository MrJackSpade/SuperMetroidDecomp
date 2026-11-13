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

/// <summary>Norfair Ridley's arrival and fight: hovering, swoops, pogo and ground attacks, and grabbing Samus.</summary>
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
    /// <summary>The enemy pass's view of the frame, whose power bomb explosion Ridley dodges.</summary>
    private readonly EnemyFrameContext _frameContext;


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
        _frameContext = dependencies.FrameContext;
    }

    /// <summary>
    /// Ports the area-two branch of the shared initializer. A defeated Ridley
    /// is deleted before any shared workspace or palette side effects, matching the
    /// boss-bit gate; a live fight begins in layer five below the visible arena.
    /// </summary>
    internal void InitializeNorfairRidley(RoomEnemySlot slot)
    {
        _ridleyEncounter.Parts.RequireBody(slot, "Lower Norfair Ridley");

        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
            slot.Presence = slot.Presence.With(EnemyPresence.Deleted | EnemyPresence.Hidden);
            return;
        }
        slot.CollidesByFrameHitboxes = true;

        // One property bit blocks Plasma Beam penetration. Its meaning is established
        // only for this boss, so preserve the raw bit instead of widening the shared enum.
        slot.Contact = slot.Contact.With(EnemyContact.BlocksPlasmaBeam);
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        slot.Position = slot.Position.WithWhole(new(96, 394));

        _ridleyEncounter.Ridley = new RidleyEnemyState
        {
            Function = RidleyAiFunction.WaitForDoorTransition,
            FightMode = RidleyFightMode.NotFighting,
            MovementAnimationEnabled = true,
            FacingDirection = RidleyFacing.Right,
            IdleTailWhipEnabled = false,
            TailDamage = 120,
            MovementMinimum = new Position16<RoomPixels>(64, 64),
            MovementMaximum = new Position16<RoomPixels>(224, 416),
            WingFrame = 0,
            WingAnimationTimer = 0,
            WingAnimationTimerDelta = 0,
            TailFunctionIndex = RidleyTailFunction.None,
            TailAngleDelta = 1,
            TailMinimumClockwiseAngle = RidleyTailDefinitions.InitialMinimumClockwise,
            TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.InitialMaximumCounterClockwise,
            ClockwiseWhipTarget = null,
            CounterClockwiseWhipTarget = null,
            TailExtensionSpeed = RidleyTailDefinitions.StandardExtensionSpeed,
            IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentSpacing,
            TailSegments = RidleyTail.CreateInitialRidleyTailSegments(),
        };
        CeresRidleyFlight.StartRidleyProgram(_ridleyEncounter.Ridley, slot, RidleyBodyProgram.Initial);

        // He arrives in his own colours, his body and tail hidden in black, and his arena hidden for its reveal.
        _ridleyEncounter.Ridley.Colours.ShowNorfairArrival();
    }

    /// <summary>
    /// The arena reveal advances an area-two-only fourteen-color fade every third dispatcher call.
    /// The step after the last palette activates the shared tail/wing composition.
    /// </summary>
    internal void TickNorfairRidleyArenaReveal(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (--state.FunctionTimer >= 0)
            return;

        state.FunctionTimer = 2;
        int step = state.FadeStep;
        bool hasPalette;
        if (step > NorfairRidleyPaletteDefinitions.RevealStepCount)
            throw new InvalidDataException($"Norfair Ridley reveal step {step} is past the reveal's end.");
        hasPalette = step < NorfairRidleyPaletteDefinitions.RevealStepCount;
        if (hasPalette)
            state.Colours.ShowArenaReveal(step);
        state.FadeStep++;
        if (hasPalette)
            return;

        RidleyLiquidMotion.Publish(_roomFx, state, RidleyLiquidDefinitions.BattleHeight,
            RidleyLiquidDefinitions.RiseVelocity, RidleyLiquidDefinitions.RiseDelay);
        state.FadeStep = 0;
        CeresRidleyFlight.StartRidleyProgram(state, slot, RidleyBodyProgram.TransitionToFlying);
        state.WingAnimationTimer = 8;
        state.WingAnimationTimerDelta = 8;
        foreach (RidleyTailSegment segment in state.TailSegments)
            segment.Active = true;
        state.TailFunctionIndex = RidleyTailFunction.Neutral;
        state.Function = RidleyAiFunction.ClearVelocity;
    }

    /// <summary>Steers Ridley to the facing-dependent hover point until timeout or arrival.</summary>
    internal static void TickNorfairRidleyHover(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (NorfairRidleyMovement.TickRidleyFunctionTimer(state))
        {
            state.Function = RidleyAiFunction.NorfairSelectAttack;
            return;
        }

        Position16<RoomPixels> target = new(state.FacingDirection != RidleyFacing.Left ? 96 : 192, 256);
        NorfairRidleyMovement.MoveNorfairRidleyToward(
            slot,
            state,
            target,
            ReadRidleyHealthMovementDivisor(state));
        if (CeresRidleyFlight.IsWithinRidleyRectangle(slot, target, new(8, 8)))
            state.Function = RidleyAiFunction.NorfairSelectAttack;
    }

    /// <summary>Moves to the high side point and initializes the swoop angle timer.</summary>
    internal static void TickNorfairRidleySwoopMoveToStart(RoomEnemySlot slot, RidleyEnemyState state)
    {
        Position16<RoomPixels> target = new(state.FacingDirection != RidleyFacing.Left ? 64 : 192, 128);
        NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, target, divisor: 15);
        if (!CeresRidleyFlight.IsWithinRidleyRectangle(slot, target, new(8, 8)))
            return;

        state.Function = RidleyAiFunction.NorfairSwoopAimDown;
        state.SwoopPhaseTimer = 32;
        state.SwoopAngleAccumulator = RotationAngle.Zero;
    }

    /// <summary>Advances the swoop velocity arc and changes phase when its timer expires.</summary>
    internal static bool TickNorfairRidleySwoopPhase(
        RidleyEnemyState state,
        int angleDelta,
        RotationAngle targetAngle,
        int targetMagnitude,
        RidleyAiFunction nextFunction,
        int nextTimer)
    {
        CeresRidleyFlight.UpdateRidleySwoopVelocity(state, angleDelta, targetAngle, targetMagnitude);
        if (state.SwoopPhaseTimer != 0)
        {
            state.SwoopPhaseTimer--;
            return false;
        }

        state.Function = nextFunction;
        state.SwoopPhaseTimer = nextTimer;
        return true;
    }

    /// <summary>
    /// Runs the pogo's two side states. These two states alternate side targets while Samus is spin
    /// jumping; any other movement type immediately advances into the low arena attack.
    /// </summary>
    internal void TickNorfairRidleyPogo(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        VerticalDirection direction)
    {
        RidleyFacing facing = state.FacingDirection;
        NorfairRidleyMovement.MoveNorfairRidleyToward(
            slot,
            state,
            new Position16<RoomPixels>(
                direction == VerticalDirection.Down ? RidleyMovementTargets.DescendingPogoX(facing) : RidleyMovementTargets.AscendingPogoX(facing),
                samus is null ? 352 : int.Min(samus.Position.Y.Whole, 352)),
            ReadRidleyHealthMovementDivisor(state));
        state.TailWhipRequest = 1;

        if (samus is null || SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.SpinJumping)
        {
            BeginNorfairRidleyGroundAttack(state);
            return;
        }

        // The pogo choice uses the current random number; it does not draw a new one.
        if (RidleyRandomDefinitions.PogoFireballs.IsTakenBy(_room.Random.Read()) && !state.Roaring && state.FacingDirection != RidleyFacing.Turning)
            CeresRidleyFlight.StartRidleyProgram(state, slot, RidleyBodyProgram.Fireballing);

        if (!NorfairRidleyMovement.TickRidleyFunctionTimer(state))
            return;

        state.Function = direction == VerticalDirection.Down
            ? RidleyAiFunction.NorfairPogoAscending
            : RidleyAiFunction.NorfairPogoDescending;
        state.FunctionTimer = 128;
        SelectNorfairRidleyFacingInstruction(slot, state);
    }

    /// <summary>Resets tail extension and enters the ground fireball/pogo setup.</summary>
    internal static void BeginNorfairRidleyGroundAttack(RidleyEnemyState state)
    {
        state.TailExtensionSpeed = RidleyTailDefinitions.StandardExtensionSpeed;
        state.IdealInterSegmentTailAngle = 16;
        state.TailFunctionIndex = RidleyTailFunction.Neutral;
        state.Function = RidleyAiFunction.NorfairFireballMoveToSide;
    }

    /// <summary>Moves Ridley toward the facing-dependent side before descending to attack height.</summary>
    internal void TickNorfairRidleyGroundAttackMoveToSide(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (CoordinateAxis.Difference(slot.Position.Y.Whole, 288) < 0)
        {
            SelectNorfairRidleyFacingInstruction(slot, state);
            state.Function = RidleyAiFunction.NorfairFireballMoveToHeight;
            state.FunctionTimer = 32;
            TickNorfairRidleyGroundAttackMoveToHeight(slot, state, samus);
            return;
        }

        NorfairRidleyMovement.MoveNorfairRidleyToward(
            slot, state, new Position16<RoomPixels>(RidleyMovementTargets.GroundAttackX(state.FacingDirection), 288), divisor: 16);
    }

    /// <summary>Settles at Y=288, sets the pogo tail, and starts a randomized attack timer.</summary>
    internal void TickNorfairRidleyGroundAttackMoveToHeight(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, slot.Position.Whole.WithY(288), divisor: 16);
        if (!NorfairRidleyMovement.TickRidleyFunctionTimer(state))
            return;

        state.TailFunctionIndex = RidleyTailFunction.PogoSetup;
        _ridleyTail.TickRidleyPogoTail(slot, state, samus);
        InitializeNorfairRidleyPogoVelocity(state);
        state.Function = RidleyAiFunction.NorfairFireballAttack;
        state.FunctionTimer = RidleyRandomDefinitions.FireballAttackTimer.Of(_room.Random.Read()) + 128;
    }

    /// <summary>Checks claw grab, accelerates downward, and bounces the tail on terrain contact.</summary>
    internal void TickNorfairRidleyGroundAttack(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is not null && SamusMovementUsesRidleyGrab(samus) &&
            RidleyClawOverlapsSamus(slot, state, samus, new Extent16<RoomPixels>(4, 4)))
        {
            // Every acceleration clamps Ridley's speed to a few pixels an update, so it always has a magnitude.
            state.Velocity = state.Velocity.WithY(-SignedEightEight.Max(GrabLiftMinimumSpeed, state.Velocity.Y.Magnitude()));
            state.TailFunctionIndex = RidleyTailFunction.Neutral;
            state.TailAngleDelta = 1;
            BeginNorfairRidleyGrab(slot, state, samus);
            return;
        }

        // The fall is clamped to six pixels an update, so adding the acceleration stays far inside the speed's range.
        state.Velocity = state.Velocity.WithY(SignedEightEight.Min(state.Velocity.Y + state.PogoDownwardAcceleration, PogoMaximumFallSpeed));

        if (!RidleyTail.RidleyTailTouchesTerrain(state, level))
            return;

        RidleyTailSegment tip = state.TailSegments[6];
        _ridleyProjectiles.SpawnRidleyDust(tip.Position + new Offset16<RoomPixels>(0, 12), MiscDustAnimation.SmallDustCloud);
        _audio.QueueEnemySound(EnemySound.Quake, priority: SoundRequestPriority.Normal);
        _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Medium, EarthquakeDirection.Vertical, EarthquakeReach.ForegroundAndBackground), 4);
        SetRidleyPogoHorizontalDirection(slot, state, samus);
        InitializeNorfairRidleyPogoVelocity(state);
        for (int index = 0; index < state.TailSegments.Length; index++)
        {
            state.TailSegments[index].Distance = RidleyTailReach.RestDistance(index);
            state.TailSegments[index].TargetDistance = RidleyTailDefinitions.BounceDistance;
        }
        state.TailFunctionIndex = RidleyTailFunction.Pogo;
        state.PogoBounceCount++;
        if (state.PogoBounceCount >= 2)
        {
            state.PogoBounceCount = 0;
            if (state.FacingDirection != RidleyFacing.Turning)
                CeresRidleyFlight.StartRidleyProgram(state, slot, RidleyBodyProgram.Fireballing);
        }
        state.Function = RidleyAiFunction.NorfairFireballRecover;
    }

    /// <summary>Recovers upward after a pogo impact or returns to attack selection.</summary>
    internal static void TickNorfairRidleyGroundAttackRecovery(
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (samus is null || samus.Position.Y.Whole < 352 || NorfairRidleyMovement.TickRidleyFunctionTimer(state))
        {
            state.TailFunctionIndex = RidleyTailFunction.Neutral;
            state.TailAngleDelta = 1;
            state.Function = RidleyAiFunction.NorfairSelectAttack;
            return;
        }

        // The rise starts at its launch speed and slows toward rest, so adding the acceleration never passes the speed's
        // range.
        SignedEightEight nextVertical = state.Velocity.Y + state.PogoUpwardAcceleration;
        if (!nextVertical.IsNegative)
        {
            state.Velocity = state.Velocity.WithY(SignedEightEight.Zero);
            state.Function = RidleyAiFunction.NorfairFireballAttack;
        }
        else
        {
            state.Velocity = state.Velocity.WithY(nextVertical);
        }
    }

    /// <summary>
    /// Runs the four authored velocity tables the pogo setup consumes. The signedness
    /// of the previous horizontal velocity chooses direction; magnitudes and both vertical
    /// accelerations are compiled definitions indexed by Ridley's health stage.
    /// </summary>
    internal void InitializeNorfairRidleyPogoVelocity(RidleyEnemyState state)
    {
        // The routine samples the current word; it does not advance shared RNG.
        int randomIndex = RidleyRandomDefinitions.PogoChoice.Of(_room.Random.Read());
        var definition = RidleyPogo.Read(randomIndex, Math.Min(state.HealthStage, 3) + 2);
        state.PogoUpwardAcceleration = definition.UpwardAcceleration;
        state.PogoDownwardAcceleration = definition.DownwardAcceleration;
        state.Velocity = state.Velocity.WithY(definition.Velocity.Y);
        SignedEightEight horizontal = definition.Velocity.X;
        state.Velocity = state.Velocity.WithX(state.Velocity.X.IsNegative
            ? horizontal.Negated()
            : horizontal);
    }

    /// <summary>Lunges toward a grabbable Samus, ending on passage, boundary, height, or claw contact.</summary>
    internal void TickNorfairRidleyGrabApproach(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (samus is null || !SamusMovementUsesRidleyGrab(samus))
        {
            HandleNorfairRidleyMissedLunge(slot, state);
            return;
        }

        // The lunge is a passing attack, not indefinite pursuit. Test the
        // previous movement's boundary result before applying another acceleration.
        int deltaX = CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole);
        bool passedSamus = state.FacingDirection == RidleyFacing.Right ? deltaX >= 0 : deltaX < 0;
        if (state.HitRoomBoundary ||
            (passedSamus && CoordinateAxis.Difference(Math.Abs(deltaX), RidleyLungeDefinitions.PassDistance) >= 0) ||
            CoordinateAxis.AsSigned(slot.Position.Y.Whole + RidleyLungeDefinitions.ClawHeight - samus.Position.Y.Whole) >= 0)
        {
            HandleNorfairRidleyMissedLunge(slot, state);
            return;
        }

        int sideOffset = state.FacingDirection != RidleyFacing.Left ? 16 : -16;
        int divisor = RidleyMovementTargets.GrabDivisor(Math.Min(state.HealthStage, 3));
        NorfairRidleyMovement.MoveNorfairRidleyToward(
            slot,
            state,
            samus.Position.Whole + new Offset16<RoomPixels>(CoordinateAxis.AsSigned(sideOffset), -4),
            divisor);

        if (RidleyClawOverlapsSamus(slot, state, samus, new Extent16<RoomPixels>(8, 12)))
        {
            // The missed lunge reverses before the carry setup accelerates.
            state.Velocity = state.Velocity.WithY(state.Velocity.Y.Negated());
            BeginNorfairRidleyGrab(slot, state, samus);
        }
    }

    /// <summary>Locks Samus to the claw and enters carry or the zero-health death approach.</summary>
    private static void BeginNorfairRidleyGrab(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState samus)
    {
        state.GrabOffset = samus.Position.Whole - new Position16<RoomPixels>(GetNorfairRidleyClawX(slot, state), GetNorfairRidleyClawY(slot, state));
        state.GrabState = RidleyGrabState.CarryingSamus;
        samus.Control.LockStationary();
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        if (slot.Health == 0)
        {
            StartNorfairRidleyDeathSequence(slot, state);
            // The grab tail-jumps to the move-to-death-spot function on the same successful zero-health grab.
            TickNorfairRidleyMoveToDeathSpot(slot, state);
            return;
        }
        // The grab falls through into the carry on the grabbing update.
        BeginNorfairRidleyCarry(slot, state);
    }

    /// <summary>Chooses a facing-dependent carry anchor and starts Ridley's 32-frame move.</summary>
    internal static void BeginNorfairRidleyCarry(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.Target = new Position16<RoomPixels>(
            RidleyMovementTargets.CarryAnchorX(state.FacingDirection),
            CoordinateAxis.Difference(slot.Position.Y.Whole, 320) < 0
                ? 256
                : CoordinateAxis.Wrap(slot.Position.Y.Whole - 64));
        state.Function = RidleyAiFunction.NorfairCarryMoveToAnchor;
        state.FunctionTimer = 32;
        NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, state.Target, 16);
        NorfairRidleyMovement.TickRidleyFunctionTimer(state);
    }

    /// <summary>Decays Samus's grab offset and positions her at Ridley's moving claw.</summary>
    internal static void UpdateNorfairRidleyGrabbedSamus(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState samus)
    {
        state.GrabOffset = new Offset16<RoomPixels>(
            DecayRidleyGrabOffset(state.GrabOffset.X),
            DecayRidleyGrabOffset(state.GrabOffset.Y));
        samus.Position = samus.Position.WithWhole(
            new Position16<RoomPixels>(GetNorfairRidleyClawX(slot, state), GetNorfairRidleyClawY(slot, state)) +
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
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState samus,
        Extent16<RoomPixels> radius) =>
        CoordinateAxis.Distance(samus.Position.X.Whole, GetNorfairRidleyClawX(slot, state)) <
            samus.Kinematics.Radius.X + radius.X &&
        CoordinateAxis.Distance(samus.Position.Y.Whole, GetNorfairRidleyClawY(slot, state)) <
            samus.Kinematics.Radius.Y + radius.Y;

    /// <summary>Applies the facing-dependent claw X offset to Ridley's body position.</summary>
    private static int GetNorfairRidleyClawX(RoomEnemySlot slot, RidleyEnemyState state) =>
        CoordinateAxis.Wrap(slot.Position.X.Whole + RidleyClawOffsets.ReadX(state.FacingDirection));

    /// <summary>Applies the current feet-frame claw Y offset to Ridley's body position.</summary>
    private static int GetNorfairRidleyClawY(RoomEnemySlot slot, RidleyEnemyState state) =>
        CoordinateAxis.Wrap(slot.Position.Y.Whole + RidleyClawOffsets.ReadY(state.FeetDistanceIndex));

    /// <summary>Starts a turn only when Ridley faces away from the arena half identified by X bit seven.</summary>
    internal static void SelectNorfairRidleyFacingInstruction(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (state.FacingDirection == RidleyFacing.Turning)
            return;
        // The facing test reads whether X lies in the right half of its 256-pixel screen,
        // not the sign of the full coordinate. Keep an inward
        // facing actor's current animation and timer untouched.
        bool rightHalf = RoomGridConversions.OffsetWithinScreen(slot.Position.X.Whole) >= 128;
        if (state.FacingDirection == RidleyFacing.Left ? rightHalf : !rightHalf)
            return;
        CeresRidleyFlight.StartRidleyProgram(
            state,
            slot,
            state.FacingDirection == RidleyFacing.Left
                ? RidleyBodyProgram.TurnFromLeftToRight
                : RidleyBodyProgram.TurnFromRightToLeft);
        slot.AnimationClock.HoldFor(2);
    }

    /// <summary>Selects a hover acceleration divisor from Ridley's capped health stage.</summary>
    internal static int ReadRidleyHealthMovementDivisor(RidleyEnemyState state) =>
        RidleyMovementTargets.HoverDivisor(Math.Min(state.HealthStage, 3));

    /// <summary>Checks the validated pose movement type against Ridley's grabbable states.</summary>
    internal static bool SamusMovementUsesRidleyGrab(SamusState? samus)
    {
        if (samus is null)
            return false;
        SamusMovementType movement = SamusPoseMetadata.ReadMovementType(samus);
        return RidleyGrabs.CanGrab(movement);
    }

    /// <summary>
    /// Lunges when a power bomb is active and Ridley is not holding Samus, run before the function on
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

        // The power-bomb lunge function resets both tail control words before the lunge.
        state.TailFunctionIndex = RidleyTailFunction.Neutral;
        state.TailAngleDelta = 1;
        state.Function = RidleyAiFunction.NorfairGrabApproach;
    }

    /// <summary>On zero health, makes Ridley intangible and enters the Samus-release death state.</summary>
    private static void StartNorfairRidleyDeathSequence(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        if (body.Health != 0 || state.FightMode == RidleyFightMode.Dying)
            return;

        state.FightMode = RidleyFightMode.Dying;
        body.Contact = body.Contact.With(EnemyContact.IgnoresSamus);
        state.Function = RidleyAiFunction.NorfairReleaseSamus;
    }

    /// <summary>
    /// Keeps the already-grabbed Samus attached while accelerating Ridley toward
    /// the four-pixel rectangle centered at (128,328). Reaching it immediately tail-calls
    /// the roar setup instead of spending a host-only transition frame.
    /// </summary>
    internal static void TickNorfairRidleyMoveToDeathSpot(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        NorfairRidleyMovement.MoveNorfairRidleyToward(body, state, new Position16<RoomPixels>(128, 328),
            RidleyInertiaDefinitions.DeathDivisor, RidleyInertiaDefinitions.DeathReversalBoost);
        if (CeresRidleyFlight.IsWithinRidleyRectangle(body, new(128, 328), new(4, 4)))
            BeginNorfairRidleyDeathRoar(body, state);
    }

    /// <summary>Selects the death-roar list and arms its 32-frame move.</summary>
    internal static void BeginNorfairRidleyDeathRoar(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        CeresRidleyFlight.StartRidleyProgram(state, body, RidleyBodyProgram.DeathRoar);
        state.Function = RidleyAiFunction.NorfairDeathExplosions;
        state.FunctionTimer = 32;
    }

    /// <summary>Handles a missed lunge: death has priority over power-bomb dodge and ordinary retreat.</summary>
    private void HandleNorfairRidleyMissedLunge(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (state.ZeroHealthLungeCount >= RidleyLungeDefinitions.DeathLungeLimit)
        {
            StartNorfairRidleyDeathSequence(slot, state);
            BeginNorfairRidleyDeathRoar(slot, state);
            return;
        }
        if (_frameContext.PowerBombExplosion is { IsArmed: true })
        {
            state.Function = RidleyAiFunction.NorfairReturnToArena;
            TickNorfairRidleyPowerBombDodge(slot, state);
            return;
        }
        state.Function = RidleyAiFunction.NorfairHoverSetup;
        state.TailWhipRequest = 1;
    }

    /// <summary>Runs the power-bomb branch of missed-lunge recovery.</summary>
    internal void TickNorfairRidleyPowerBombDodge(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (_frameContext.PowerBombExplosion is not { IsArmed: true } bomb)
        {
            state.FightMode = RidleyFightMode.Fighting;
            state.Function = state.GrabState == RidleyGrabState.Released
                ? RidleyAiFunction.NorfairSelectAttack : RidleyAiFunction.NorfairCarrySetup;
            return;
        }
        state.FightMode = RidleyFightMode.PowerBombDodge;
        Position16<RoomPixels> target = new(
            CoordinateAxis.Difference(bomb.Position.X, RidleyLungeDefinitions.DodgeSplitX) >= 0
                ? RidleyLungeDefinitions.DodgeLeftX : RidleyLungeDefinitions.DodgeRightX,
            CoordinateAxis.Difference(bomb.Position.Y, RidleyLungeDefinitions.DodgeSplitY) >= 0
                ? RidleyLungeDefinitions.DodgeUpperY : RidleyLungeDefinitions.DodgeLowerY);
        NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, target, ReadRidleyHealthMovementDivisor(state));
    }

    /// <summary>The sideways speed a pogo bounce starts from when Ridley has none: three quarters of a pixel an update.</summary>
    private static readonly SignedEightEight PogoRestartSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(192);

    /// <summary>The least upward speed Ridley lifts a grabbed Samus with.</summary>
    private static readonly SignedEightEight GrabLiftMinimumSpeed = SignedEightEight.FromPixels(2);

    /// <summary>The fastest Ridley falls on a pogo bounce: six pixels an update.</summary>
    private static readonly SignedEightEight PogoMaximumFallSpeed = SignedEightEight.FromPixels(6);

    /// <summary>Chooses the next bounce's direction from position and the current seed.</summary>
    internal void SetRidleyPogoHorizontalDirection(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus)
    {
        if (state.Velocity.X.IsZero)
            state.Velocity = state.Velocity.WithX(RoomGridConversions.OffsetWithinScreen(slot.Position.X.Whole) < 128 ? PogoRestartSpeed : -PogoRestartSpeed);
        bool negative = state.Velocity.X.IsNegative;
        bool reverse;
        if (CoordinateAxis.Difference(slot.Position.X.Whole, state.MovementMinimum.X) < 0) reverse = negative;
        else if (CoordinateAxis.Difference(slot.Position.X.Whole, state.MovementMaximum.X) >= 0) reverse = !negative;
        else
        {
            bool headsTowardSamus = CoordinateAxis.Difference(slot.Position.X.Whole, samus?.Position.X.Whole ?? 0) < 0 != state.Velocity.X.IsNegative;
            bool headsAway = RidleyRandomDefinitions.PogoBounceHeadsAwayFromSamus.IsTakenBy(_room.Random.Read());
            reverse = headsTowardSamus ? headsAway : !headsAway;
        }
        if (reverse) state.Velocity = state.Velocity.WithX(state.Velocity.X.Negated());
    }
}
