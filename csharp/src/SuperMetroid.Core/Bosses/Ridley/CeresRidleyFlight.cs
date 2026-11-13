using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's flight: liftoff, hovering, fireballs, lunges, swoops, the fake retreat and taking the baby, and the movement integration they share.</summary>
[RoomLifetime]
internal sealed class CeresRidleyFlight
{
    /// <summary>Runs Ridley's shared movement behaviours through the enemies' AI dispatch.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Provides room randomness and Ceres artwork state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Offsets the scene's own Baby and door drawing during a room quake.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues Ceres battle and getaway sound effects.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Finds the free place the turning-floor wall enemy takes when Ridley spawns it.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
    private readonly RidleyEncounter _ridleyEncounter;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyFlight(CeresRidleyFlightDependencies dependencies)
    {
        _dispatch = dependencies.Dispatch;
        _room = dependencies.Room;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _slotTable = dependencies.SlotTable;
        _ridleyEncounter = dependencies.RidleyEncounter;
    }

    /// <summary>
    /// Sets up the Ceres branch of Ridley's initializer; Norfair Ridley's branch is his own part's. The initializer's
    /// branch for a Ceres Ridley whose boss is already dead never runs: the room's state once its boss is dead places
    /// no Ridley, only its door.
    /// </summary>
    internal void InitializeCeresRidley(RoomEnemySlot slot)
    {
        _ridleyEncounter.Parts.RequireBody(slot, "Ceres Ridley");

        // Ceres Ridley's initialization clears the boss's working state, which the fresh Ridley state below holds, and
        // disables the minimap, which the room loader does once the enemies are loaded; Ceres has no arena to mark explored.
        slot.CollidesByFrameHitboxes = true;

        // The initializer sets both of Ridley's contact properties: he stops the Plasma Beam and ignores Samus.
        slot.Contact = slot.Contact.With(EnemyContact.BlocksPlasmaBeam);
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        slot.Position = slot.Position.WithWhole(new(186, 169));
        _ridleyEncounter.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;

        _ridleyEncounter.Ridley = new RidleyEnemyState
        {
            Phase = RidleyPhase.WaitForDoorTransition,
            FightMode = RidleyFightMode.NotFighting,
            HitCounter = 0,
            MovementAnimationEnabled = false,
            FacingDirection = RidleyFacing.Left,
            IdleTailWhipEnabled = true,
            TailDamage = 15,
            Velocity = EightEightVector<RoomPixels>.Zero,
            MovementMinimum = Position16<RoomPixels>.Wrapped(40, -32),
            MovementMaximum = new Position16<RoomPixels>(224, 176),
            BabyPlayback = new AnimationPlayback<CeresBabySegment>(CeresBabyAnimations.StartingWith(CeresBabySegment.Initial), AnimationPlace.StartOfAnimation),
            BabyFrameHold = 1,
            BabyAction = CeresBabyAction.CarriedInArms,
            BabyCurrentPose = null,
            BabyPosition = FixedPosition<RoomPixels>.FromWhole(slot.Position.Whole + CeresRidleyBabyScene.CeresBabyArmsAnchor),
            WingFrame = 5,
            WingAnimationTimer = 0,
            WingAnimationTimerDelta = 0,
            TailAction = RidleyTailAction.None,
            TailAngleDelta = 1,
            TailMinimumClockwiseAngle = RidleyTailDefinitions.InitialMinimumClockwise,
            TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.InitialMaximumCounterClockwise,
            ClockwiseWhipTarget = null,
            CounterClockwiseWhipTarget = null,
            TailWhipRequest = 0,
            TailExtensionSpeed = RidleyTailDefinitions.StandardExtensionSpeed,
            IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentSpacing,
            TailSegments = RidleyTail.CreateInitialRidleyTailSegments(),
        };
        StartRidleyAnimation(_ridleyEncounter.Ridley, slot, RidleyBodySegment.Initial);
        // Ceres Ridley's initialization ends by queuing music zero (stop).
        _audio.RequestRoomEnemyStartMusic(MusicRequest.Stop);

        // He arrives with the door's, container's and Baby's colours, his own hidden in black for his eye and body fades.
        _ridleyEncounter.Ridley.Colours.ShowCeresArrival();
    }

    /// <summary>Applies the crossing-frame -16 then +20 velocity changes before shared Ridley motion.</summary>
    internal static void TickCeresRidleyLiftoffDecelerating(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        // The fallthrough from the rise deliberately applies both -16 and +20 on the crossing
        // frame. Do not combine these into a single acceleration outside this state.
        // The rise carries Ridley past line 80, 32 pixels above where the slowing starts, within a few updates.
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + SignedEightEight.FromTwoHundredFiftySixthsOfPixel(20));
        if (CoordinateAxis.Difference(slot.Position.Y.Whole, 80) < 0)
        {
            state.Phase = RidleyPhase.CeresHovering;
            state.FightMode = RidleyFightMode.Fighting;
        }
    }

    /// <summary>Tests the scripted shot-count and Samus-energy exits, then admits attacks only inside the hover box.</summary>
    internal void TickCeresRidleyHover(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        // Ceres Ridley never loses the ordinary enemy health. His shot AI increments
        // this separate counter and the fight treats 100 contacts as the scripted win condition.
        if (state.HitCounter >= 100)
        {
            state.FightMode = RidleyFightMode.NotFighting;
            state.Phase = RidleyPhase.CeresFakeRetreatMoveToPosition;
            TickCeresRidleyFakeRetreatMoveToPosition(slot, state);
            return;
        }

        // The other original termination route is Samus energy below 30. A synthetic room
        // fixture may omit Samus, in which case only the shot-counter route is meaningful.
        // The low-energy check jumps into the real retreat, so its first acceleration runs this call.
        if (samus is not null && samus.Health < 30)
        {
            state.FightMode = RidleyFightMode.NotFighting;
            BeginCeresRidleyRetreat(state);
            TickCeresRidleyRetreat(slot, state);
            return;
        }

        // The shared cooldown/return-to-hover movement: accelerate toward (192, 100)
        //  using divisor table entry zero.
        AccelerateRidleyToward(slot, state, target: new(192, 100), divisor: 16);
        if (!IsWithinRidleyRectangle(slot, new(192, 100), new(8, 8)))
        {
            // The hover-box overlap test returns zero on rectangle overlap. The hover wait therefore waits
            // only while Ridley is still outside the hover box, with 124 as the authentic
            // timeout. An earlier version inverted this branch and selected attacks
            // while he was still above the arena, making every fireball route hit its
            // intentional 48-frame bailout before the mouth animation could begin.
            state.HoverCounter++;
            if (state.HoverCounter < 124)
                return;
        }

        state.Phase = CeresRidleyAttackDefinitions.SelectAttack(_room.Random.Read());
        state.HoverCounter = 0;
    }

    /// <summary>The least vertical speed Ridley keeps, either way, while he moves into his fireball position.</summary>
    private static readonly SignedEightEight FireballMoveMinimumSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(128);

    /// <summary>Steers Ridley into his fireball position and installs the mouth/fireball animation.</summary>
    internal void TickCeresRidleyFireballMove(RoomEnemySlot slot, RidleyEnemyState state)
    {
        // Every acceleration clamps Ridley to five pixels an update, so his speed always has a magnitude.
        SignedEightEight verticalVelocity = state.Velocity.Y;
        state.Velocity = state.Velocity.WithY(SignedEightEight.Max(FireballMoveMinimumSpeed, verticalVelocity.Magnitude())
            .AlongDirectionOf(verticalVelocity));
        AccelerateRidleyToward(slot, state, new(slot.Position.X.Whole, 88), 16);

        if (CoordinateAxis.Difference(slot.Position.Y.Whole, 80) < 0)
        {
            state.HoverCounter++;
            if (state.HoverCounter >= 48)
                state.Phase = RidleyPhase.CeresSwoopSetup;
            return;
        }
        if (CoordinateAxis.Difference(slot.Position.Y.Whole, 128) >= 0)
            return;

        state.FireballBasePosition = slot.Position.Whole;
        // The fireball phase starts the complete retail mouth/fireball animation.
        // Its fireball cues calculate and spawn actors; leaving this
        // assignment out produces correct boss motion with a conspicuously silent attack.
        StartRidleyAnimation(state, slot, RidleyBodySegment.Fireballing);
        state.Phase = RidleyPhase.CeresFireballShooting;
        state.PhaseTimer = 224;
        TickCeresRidleyFireballShooting(slot, state);
    }

    /// <summary>Selects the next fireball target from the base position and a jitter drawn from the current random number.</summary>
    internal void TickCeresRidleyFireballShooting(RoomEnemySlot slot, RidleyEnemyState state)
    {
        // The attack choice reads RandomNumberSeed without generating a number.
        int random = _room.Random.Read();
        int jitter = BossRandomRolls.InLowerHalf(random)
            ? RidleyRandomDefinitions.FireballJitter.Of(random)
            : -RidleyRandomDefinitions.FireballJitter.Of(random);
        Position16<RoomPixels> target = CeresRidleyFireballTarget.Around(state.FireballBasePosition, jitter);
        AccelerateRidleyToward(slot, state, target, 16);
        if (--state.PhaseTimer < 0)
        {
            state.HoverCounter = 0;
            state.Phase = RidleyPhase.CeresHovering;
        }
    }

    /// <summary>Advances the attack lunge until its position threshold hands control to the swoop.</summary>
    internal static void TickCeresRidleyLunge(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Ceres Ridley lunge requires the active Samus actor.");

        Position16<RoomPixels> target = CoordinateAxis.Difference(samus.Position.Y.Whole, 132) < 0
            ? samus.Position.Whole.WithY(64)
            : samus.Position.Whole - new Offset16<RoomPixels>(0, 68);
        AccelerateRidleyToward(slot, state, target, 3);
        bool reached = IsWithinRidleyRectangle(slot, target, new(2, 2));
        state.PhaseTimer--;
        if (reached || state.PhaseTimer < 0)
        {
            state.HoverCounter = 0;
            state.Phase = RidleyPhase.CeresHovering;
        }
    }

    /// <summary>Steers Ridley toward the swoop start point with the shared acceleration.</summary>
    internal static void TickCeresRidleySwoopMoveToPosition(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        AccelerateRidleyToward(slot, state, new(192, 80), 15);
        if (CoordinateAxis.Difference(slot.Position.Y.Whole, 96) < 0)
        {
            state.Phase = RidleyPhase.CeresSwoopDescendingAimingDown;
            state.PhaseTimer = 10;
            state.SwoopAngleAccumulator = RotationAngle.Zero;
        }
    }

    /// <summary>Runs the swoop arc and publishes the one-frame tail-fling request at its bottom.</summary>
    internal static void TickCeresRidleySwoopPhase(
        RidleyEnemyState state,
        int angleDelta,
        RotationAngle targetAngle,
        int targetMagnitude,
        RidleyPhase nextPhase,
        int nextTimer)
    {
        UpdateRidleySwoopVelocity(state, angleDelta, targetAngle, targetMagnitude);
        if (--state.PhaseTimer < 0)
        {
            state.Phase = nextPhase;
            state.PhaseTimer = nextTimer;
        }
    }

    /// <summary>
    /// Turns Ridley's swoop heading by <paramref name="angleDelta"/> 256ths of a step toward <paramref name="targetAngle"/>,
    /// testing the sign of the difference as the heading wraps, and derives his velocity from it.
    /// </summary>
    internal static void UpdateRidleySwoopVelocity(
        RidleyEnemyState state,
        int angleDelta,
        RotationAngle targetAngle,
        int targetMagnitude)
    {
        int magnitude = state.SwoopSpeedMagnitude;
        if (magnitude < targetMagnitude)
            magnitude = Math.Min(targetMagnitude, magnitude + 32);
        else if (magnitude > targetMagnitude)
            magnitude = Math.Max(targetMagnitude, magnitude - 32);
        state.SwoopSpeedMagnitude = magnitude;

        RotationAngle angle = state.SwoopAngleAccumulator.TurnedByFractionsOfStep(angleDelta);
        // The comparison tests the sign of the difference, not signed less-than. The heading wraps around the
        // turn, particularly onto the half-turn recovery target after a rightward swoop.
        bool negativeDifference = angle.TurnFrom(targetAngle) < 0;
        if (angleDelta < 0 ? negativeDifference : !negativeDifference)
            angle = targetAngle;
        state.SwoopAngleAccumulator = angle;
        TrigonometryAngle phase = angle.ToTrigonometryAngle();
        SignedEightEight speed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(magnitude);
        state.Velocity = state.Velocity.WithX(Trigonometry.MultiplySignedSine(speed, phase));
        state.Velocity = state.Velocity.WithY(Trigonometry.MultiplySignedSine(speed, phase.TurnedBy(TrigonometryAngle.QuarterTurn)));
    }

    /// <summary>Switches the encounter into the initial scripted retreat.</summary>
    private static void BeginCeresRidleyRetreat(RidleyEnemyState state)
    {
        state.Phase = RidleyPhase.CeresRealRetreatRising;
        state.MovementMinimum = state.MovementMinimum.WithY(CoordinateAxis.Wrap(-192));
    }

    /// <summary>Moves the 100-shot retreat toward the Baby retrieval staging point.</summary>
    internal static void TickCeresRidleyFakeRetreatMoveToPosition(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        // The shared Ridley phase steers toward (192,128). Ceres uses this only for
        // the 100-shot "fake retreat"; the low-energy branch bypasses the Baby retrieval.
        AccelerateRidleyToward(slot, state, new(192, 128), 15);
        if (CoordinateAxis.Difference(slot.Position.X.Whole, 192) >= 0)
            state.Phase = RidleyPhase.CeresFakeRetreatRising;
    }

    /// <summary>Raises Ridley above Y=32 before releasing the Baby into its independent fall.</summary>
    internal static void TickCeresRidleyFakeRetreatRising(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        // Ridley rises past Y=32 while the Baby still follows his original hand
        // anchor. Crossing that line starts the Baby's independent fall and a
        // 21-underflow wait before Ridley changes to the retrieval pose.
        state.MovementMinimum = state.MovementMinimum.WithY(CoordinateAxis.Wrap(-192));
        AccelerateRidleyToward(slot, state, Position16<RoomPixels>.Wrapped(192, -128), 15);
        if (CoordinateAxis.Difference(slot.Position.Y.Whole, 32) < 0)
        {
            state.BabyAction = CeresBabyAction.Drop;
            state.Phase = RidleyPhase.CeresWaitBeforeRetrievingBaby;
            state.PhaseTimer = 21;
        }
    }

    /// <summary>Steers Ridley's hand toward the falling Baby and accepts overlapping radius-four grasp rectangles.</summary>
    internal static void TickCeresRidleyRetrieveBaby(RoomEnemySlot slot, RidleyEnemyState state)
    {
        // The Baby grab flies Ridley's hand toward the falling Baby. The collision compares
        // two radius-four rectangles centered on the hand and Baby points.
        AccelerateRidleyToward(slot, state, state.BabyPosition.Whole - CeresRidleyHandToBaby, 4);

        Offset16<RoomPixels> fromHand = state.BabyPosition.Whole - (slot.Position.Whole + CeresRidleyBabyScene.CeresBabyFeetAnchor);
        if (Math.Abs(fromHand.X) >= 8 || Math.Abs(fromHand.Y) >= 8)
        {
            return;
        }

        state.BabyAction = CeresBabyAction.CarriedInFeet;
        state.Velocity = state.Velocity.WithY(SignedEightEight.FromPixels(-2));
        BeginCeresRidleyRetreat(state);
    }

    /// <summary>Accelerates Ridley toward the offscreen retreat point and begins the 64-frame getaway delay.</summary>
    internal void TickCeresRidleyRetreat(RoomEnemySlot slot, RidleyEnemyState state)
    {
        // The retreat accelerates toward the off-screen point. The signed test
        // is performed before the common movement pass; once the origin is above Y=-128
        // the 64-frame delay is installed and the delay falls through into its first tick.
        AccelerateRidleyToward(slot, state, Position16<RoomPixels>.Wrapped(192, -128), 15);
        if (CoordinateAxis.AsSigned(slot.Position.Y.Whole + 128) < 0)
        {
            state.Phase = RidleyPhase.CeresRetreatDelay;
            state.PhaseTimer = 64;
            TickCeresRidleyRetreatDelay(state);
        }
    }

    /// <summary>Ends the retreat's delay: places the two wall actors, stops Ridley and recolours the chamber.</summary>
    internal void TickCeresRidleyRetreatDelay(RidleyEnemyState state)
    {
        if (--state.PhaseTimer >= 0)
            return;

        // The retreat places the two wall actors before the getaway begins. The getaway scene shows the chamber's walls
        // itself (#1545); the actors draw nothing, but they take two enemy places (SpawnCeresRidleyGetawayWalls).
        SpawnCeresRidleyGetawayWalls();
        state.Velocity = EightEightVector<RoomPixels>.Zero;
        state.TailAction = RidleyTailAction.None;
        state.Phase = RidleyPhase.CeresPublishEscapeHandoff;

        // The retreat recolours the chamber: its walls and its first tiles. The game also copied the first tiles'
        // colours into Ridley's own sprite colours; his sprites here keep their own colours (#1544).
        state.Colours.ShowRetreat();
    }

    /// <summary>The retrieval steering target relative to the falling Baby.</summary>
    private static Offset16<RoomPixels> CeresRidleyHandToBaby => new(10, 56);

    /// <summary>Steers both 8.8 velocity axes toward a target, promoting zero hardware quotients to one.</summary>
    internal static void AccelerateRidleyToward(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        Position16<RoomPixels> target,
        int divisor)
    {
        // Division truncates like the game's quotient and a zero quotient is promoted
        // to one, ensuring that even a one-pixel error continues to change velocity.
        divisor = RidleyInertia.Divisor(divisor);

        state.Velocity = new EightEightVector<RoomPixels>(
            AccelerateAxis(state.Velocity.X, slot.Position.X.Whole, target.X, divisor),
            AccelerateAxis(state.Velocity.Y, slot.Position.Y.Whole, target.Y, divisor));
    }

    /// <summary>
    /// One axis of Ridley's seek (Y and X): one run of steps toward the target, three while reversing
    /// and one otherwise, in which a step that crosses zero makes the next one a unit larger
    /// (<see cref="SeekingVelocityRun"/>). The velocity stays within a few thousand units of rest (each call clamps it
    /// to five pixels and a step is at most the arena's width), so the clamps compare signed velocities.
    /// </summary>
    /// <param name="velocity">The axis velocity.</param>
    /// <param name="position">Ridley's whole coordinate on the axis, as his position holds it.</param>
    /// <param name="target">The target's whole coordinate on the axis, as its position holds it.</param>
    /// <param name="divisor">The inertia divisor, at least one.</param>
    private static SignedEightEight AccelerateAxis(SignedEightEight velocity, int position, int target, int divisor)
    {
        int difference = CoordinateAxis.Difference(position, target);
        if (difference == 0)
            return velocity;
        // Hardware quotient of the distance, at most 32768; a zero quotient is promoted to one.
        int step = Math.Max(1, Math.Abs(difference) / divisor);
        var run = new SeekingVelocityRun(velocity, position, target);

        if (difference < 0)
        {
            if (velocity.IsNegative)
            {
                run.Raise(step);
                run.Raise(step);
            }
            run.Raise(step);
            return SignedEightEight.Min(run.Velocity, CeresRidleyBehavior.CeresRidleyMaximumSpeed);
        }
        if (!velocity.IsNegative)
        {
            run.Lower(step);
            run.Lower(step);
        }
        run.Lower(step);
        return SignedEightEight.Max(run.Velocity, CeresRidleyBehavior.CeresRidleyMinimumSpeed);
    }

    /// <summary>Applies Ridley's 8.8 motion and arena clamps, invoking impact only at the strict left bound.</summary>
    internal void IntegrateRidleyMovement(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        RidleyWallImpact wallImpact = RidleyWallImpact.None)
    {
        // The bounds clamp runs the impact only when proposed X is strictly below the
        // left bound, before zeroing X velocity. Vertical speed can meet its threshold
        // even when horizontal motion is slow; the right and vertical clamps do not call it.
        FixedCoordinate proposedX = IntegrateUnclampedAxis(slot.Position.X, state.Velocity.X);
        FixedCoordinate proposedY = IntegrateUnclampedAxis(slot.Position.Y, state.Velocity.Y);
        state.HitRoomBoundary = CoordinateAxis.Difference(proposedX.Whole, state.MovementMinimum.X) < 0 ||
            CoordinateAxis.Difference(proposedX.Whole, state.MovementMaximum.X) >= 0 ||
            CoordinateAxis.Difference(proposedY.Whole, state.MovementMinimum.Y) < 0 ||
            CoordinateAxis.Difference(proposedY.Whole, state.MovementMaximum.Y) >= 0;
        if (wallImpact == RidleyWallImpact.CeresWall && CoordinateAxis.Difference(proposedX.Whole, state.MovementMinimum.X) < 0 &&
            SignedEightEight.Max(state.Velocity.X.Magnitude(), state.Velocity.Y.Magnitude()) >= RidleyWallImpactDefinitions.MinimumSpeed)
        {
            _earthquake.Start(RidleyWallImpactDefinitions.Earthquake, RidleyWallImpactDefinitions.Duration);
        }
        (FixedCoordinate x, SignedEightEight xVelocity) = IntegrateAxis(
            slot.Position.X,
            state.Velocity.X,
            state.MovementMinimum.X,
            state.MovementMaximum.X);
        state.Velocity = state.Velocity.WithX(xVelocity);
        slot.Position = slot.Position.WithX(x);
        (FixedCoordinate y, SignedEightEight yVelocity) = IntegrateAxis(
            slot.Position.Y,
            state.Velocity.Y,
            state.MovementMinimum.Y,
            state.MovementMaximum.Y);
        state.Velocity = state.Velocity.WithY(yVelocity);
        slot.Position = slot.Position.WithY(y);
    }

    /// <summary>Integrates one 8.8 velocity into a position's fraction, in 256ths of a pixel, and clips to an arena bound.</summary>
    private static CeresRidleyAxisMotion IntegrateAxis(
        FixedCoordinate position,
        SignedEightEight velocity,
        int minimum,
        int maximum)
    {
        // Ridley's velocities are signed 8.8 values that the game adds, in 256ths of a pixel, to the
        // ordinary 16.16 position's fraction.
        FixedCoordinate moved = position.MovedBy(velocity);

        if (CoordinateAxis.Difference(moved.Whole, minimum) < 0)
            return new CeresRidleyAxisMotion(moved.WithWhole(minimum), SignedEightEight.Zero);
        if (CoordinateAxis.Difference(moved.Whole, maximum) >= 0)
            return new CeresRidleyAxisMotion(moved.WithWhole(maximum), SignedEightEight.Zero);
        return new CeresRidleyAxisMotion(moved, velocity);
    }

    /// <summary>Adds an 8.8 velocity to a position before boundary checks.</summary>
    private static FixedCoordinate IntegrateUnclampedAxis(FixedCoordinate position, SignedEightEight velocity) =>
        position.MovedBy(velocity);

    /// <summary>Tests an inclusive actor rectangle for hover, lunge or retrieval handoffs.</summary>
    internal static bool IsWithinRidleyRectangle(
        RoomEnemySlot slot,
        Position16<RoomPixels> center,
        Extent16<RoomPixels> radius) =>
        // The hover check tests overlap with the actor's rectangle, including its boundary.
        // A center-only test delays flight and attack handoffs until Ridley gets too close.
        CoordinateAxis.Distance(slot.Position.X.Whole, center.X) < CoordinateAxis.Wrap(slot.Radius.X + radius.X + 1) &&
            CoordinateAxis.Distance(slot.Position.Y.Whole, center.Y) < CoordinateAxis.Wrap(slot.Radius.Y + radius.Y + 1);

    /// <summary>Advances the initial battle hold before Ridley's eye fade begins.</summary>
    internal static void TickCeresRidleyInitialDelay(RidleyEnemyState state)
    {
        if (--state.PhaseTimer >= 0)
            return;

        state.Phase = RidleyPhase.FadeInEyes;
        state.FadeStep = 0;
        state.PhaseTimer = 0;
    }

    /// <summary>Restarts Ridley's body animation and frame timer at a selected segment.</summary>
    internal static void StartRidleyAnimation(RidleyEnemyState state, RoomEnemySlot slot, RidleyBodySegment segment)
    {
        state.BodyPlayback = EnemyAnimationPlayback.Start(slot, RidleyAnimations.StartingWith(segment));
    }

    /// <summary>Animates Ridley's wings after the common Ridley movement pass.</summary>
    internal static void TickRidleyWingAnimation(RidleyEnemyState state)
    {
        // Every acceleration clamps Ridley to five pixels an update, so his speed on each axis has a magnitude.
        SignedEightEight largestAxisVelocity = SignedEightEight.Max(state.Velocity.X.Magnitude(), state.Velocity.Y.Magnitude());
        if (largestAxisVelocity.IsZero)
        {
            state.WingAnimationTimerDelta = 0;
        }
        else
        {
            // The wing animation reads the speed's quarter pixels within a cycle of sixteen as a table index and clamps
            // that index to seven.
            int tableIndex = Math.Min(7, largestAxisVelocity.WholeQuarterPixels % 16);
            int delta = CeresRidleyWingDefinitions.AnimationTimerSteps[tableIndex];
            if (!state.Velocity.Y.IsNegative)
                delta >>= 1;
            state.WingAnimationTimerDelta = delta;
        }

        state.WingAnimationTimer -= state.WingAnimationTimerDelta;
        if (state.WingAnimationTimer >= 0)
            return;

        state.WingAnimationTimer = 32;
        state.WingFrame = (state.WingFrame + 1) % 10;
    }

    /// <summary>
    /// Places the getaway's two wall actors at the chamber's walls (#1545). They draw nothing: the getaway scene shows the
    /// walls (<see cref="CeresRidleyGetaway.ChamberWalls"/>). They are kept because gameplay can observe them: each takes
    /// the first free enemy place and extends the room's filled places, and on the update after this one they are
    /// touchable, before their first animation step makes them ignore Samus and her weapons. Evidence: the original's
    /// retreat spawns the two door-family enemies, whose animations start with the intangibility step.
    /// </summary>
    private void SpawnCeresRidleyGetawayWalls()
    {
        SpawnCeresRidleyGetawayWall(CeresGetawayChamberWallDefinitions.LeftWall, CeresDoorVariant.RidleyEscapeFloorLeftWall);
        SpawnCeresRidleyGetawayWall(CeresGetawayChamberWallDefinitions.RightWall, CeresDoorVariant.RidleyEscapeFloorRightWall);
    }

    /// <summary>Initializes one wall actor at its place and variant, drawing nothing.</summary>
    private void SpawnCeresRidleyGetawayWall(Position16<RoomPixels> position, CeresDoorVariant variant)
    {
        RoomEnemySlot wall = _slotTable.FindFreeSlot() ??
            throw new InvalidOperationException("Ceres Ridley getaway has no free enemy slot for a turning-floor wall.");

        RoomEnemyDefinition definition = _room.LoadedDefinitions.ReadEnemyDefinition(EnemyDefinitionId.CeresDoor);
        RoomEnemyPopulationRecord population = new(
            EnemyDefinitionId.CeresDoor,
            Position: position,
            Contact: EnemyContact.None,
            Processing: EnemyProcessing.Animates | EnemyProcessing.OffScreen,
            Presence: EnemyPresence.Hidden,
            CollidesByFrameHitboxes: false,
            SpawnSettings: new CeresDoorSpawnSettings(variant));
        _dispatch.InitializeSlotFromDefinition(wall, population, definition);
        _dispatch.RunInitializationAi(wall, EnemySpawnSurroundings.None);
        _slotTable.CoverSpawn(wall);
    }
}
