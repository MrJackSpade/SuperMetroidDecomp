using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's flight: liftoff, hovering, fireballs, lunges, swoops, the fake retreat and taking the baby, and the movement integration they share.</summary>
[RoomLifetime]
internal sealed class CeresRidleyFlight
{
    /// <summary>Sets up the enemies Ridley's flight spawns and runs their initialization.</summary>
    private readonly IEnemyLifecycle _lifecycle;
    /// <summary>Provides room randomness and Ceres artwork state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Offsets the scene's own Baby and door drawing during a room quake.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues Ceres battle and getaway sound effects.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Finds the free place the turning-floor wall enemy takes when Ridley spawns it.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
    private readonly RidleyEncounter _ridleyEncounter;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyFlight(CeresRidleyFlightDependencies dependencies)
    {
        _lifecycle = dependencies.Lifecycle;
        _room = dependencies.Room;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _places = dependencies.Places;
        _ridleyEncounter = dependencies.RidleyEncounter;
    }

    /// <summary>
    /// Sets up the Ceres branch of Ridley's initializer; Norfair Ridley's branch is his own part's. The initializer's
    /// branch for a Ceres Ridley whose boss is already dead never runs: the room's state once its boss is dead places
    /// no Ridley, only its door.
    /// </summary>
    internal void InitializeCeresRidley(RoomEnemyPlace place)
    {
        _ridleyEncounter.Parts.AttachBody(place, "Ceres Ridley");

        // Ceres Ridley's initialization clears the boss's working state, which the fresh Ridley state below holds, and
        // disables the minimap, which the room loader does once the enemies are loaded; Ceres has no arena to mark explored.
        place.CollidesByFrameHitboxes = true;

        // The initializer sets both of Ridley's contact properties: he stops the Plasma Beam and ignores Samus.
        place.Contact = place.Contact.With(EnemyContact.BlocksPlasmaBeam);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        place.Position = place.Position.WithWhole(new(186, 169));
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
            BabyPosition = FixedPosition<RoomPixels>.FromWhole(place.Position.Whole + CeresRidleyBabyScene.CeresBabyArmsAnchor),
            WingBeatStep = 5,
            WingAnimationTimer = 0,
            WingAnimationTimerDelta = 0,
            TailAction = RidleyTailAction.None,
            TailAngleDelta = 1,
            TailMinimumClockwiseAngle = RidleyTailDefinitions.InitialMinimumClockwise,
            TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.InitialMaximumCounterClockwise,
            ClockwiseWhipTarget = null,
            CounterClockwiseWhipTarget = null,
            TailWhipRequested = false,
            TailExtensionSpeed = RidleyTailDefinitions.StandardExtensionSpeed,
            IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentSpacing,
            TailSegments = RidleyTail.CreateInitialRidleyTailSegments(),
        };
        StartRidleyAnimation(_ridleyEncounter.Ridley, place, RidleyBodySegment.Initial);
        // Ceres Ridley's initialization ends by queuing music zero (stop).
        _audio.RequestRoomEnemyStartMusic(MusicRequest.Stop);

        // He arrives with the door's, container's and Baby's colours, his own hidden in black for his eye and body fades.
        _ridleyEncounter.Ridley.Colours.ShowCeresArrival();
    }

    /// <summary>Applies the crossing-frame -16 then +20 velocity changes before shared Ridley motion.</summary>
    internal static void TickCeresRidleyLiftoffDecelerating(
        RoomEnemyPlace place,
        RidleyEnemyState state)
    {
        // The fallthrough from the rise deliberately applies both -16 and +20 on the crossing
        // frame. Do not combine these into a single acceleration outside this state.
        // The rise carries Ridley past line 80, 32 pixels above where the slowing starts, within a few updates.
        state.Velocity = state.Velocity.WithY(state.Velocity.Y + SignedEightEight.FromTwoHundredFiftySixthsOfPixel(20));
        if (CoordinateAxis.Difference(place.Position.Y.Whole, 80) < 0)
        {
            state.Phase = RidleyPhase.CeresHovering;
            state.FightMode = RidleyFightMode.Fighting;
        }
    }

    /// <summary>Tests the scripted shot-count and Samus-energy exits, then admits attacks only inside the hover box.</summary>
    internal void TickCeresRidleyHover(
        RoomEnemyPlace place,
        RidleyEnemyState state,
        SamusState samus)
    {
        // Ceres Ridley never loses the ordinary enemy health. His shot AI increments
        // this separate counter and the fight treats 100 contacts as the scripted win condition.
        if (state.HitCounter >= 100)
        {
            state.FightMode = RidleyFightMode.NotFighting;
            state.Phase = RidleyPhase.CeresFakeRetreatMoveToPosition;
            TickCeresRidleyFakeRetreatMoveToPosition(place, state);
            return;
        }

        // The other original termination route is Samus energy below 30. A synthetic room
        // fixture may omit Samus, in which case only the shot-counter route is meaningful.
        // The low-energy check jumps into the real retreat, so its first acceleration runs this call.
        if (samus.Health < 30)
        {
            state.FightMode = RidleyFightMode.NotFighting;
            BeginCeresRidleyRetreat(state);
            TickCeresRidleyRetreat(place, state);
            return;
        }

        // The shared cooldown/return-to-hover movement: accelerate toward (192, 100)
        // with the hover's divisor of 16.
        AccelerateRidleyToward(place, state, target: new(192, 100), divisor: 16);
        if (!IsWithinRidleyRectangle(place, new(192, 100), new(8, 8)))
        {
            // The hover waits only while Ridley is still outside the hover box, for at most 124 updates.
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
    internal void TickCeresRidleyFireballMove(RoomEnemyPlace place, RidleyEnemyState state)
    {
        // Every acceleration clamps Ridley to five pixels an update, so his speed always has a magnitude.
        SignedEightEight verticalVelocity = state.Velocity.Y;
        state.Velocity = state.Velocity.WithY(SignedEightEight.Max(FireballMoveMinimumSpeed, verticalVelocity.Magnitude())
            .AlongDirectionOf(verticalVelocity));
        AccelerateRidleyToward(place, state, new(place.Position.X.Whole, 88), 16);

        if (CoordinateAxis.Difference(place.Position.Y.Whole, 80) < 0)
        {
            state.HoverCounter++;
            if (state.HoverCounter >= 48)
                state.Phase = RidleyPhase.CeresSwoopSetup;
            return;
        }
        if (CoordinateAxis.Difference(place.Position.Y.Whole, 128) >= 0)
            return;

        state.FireballBasePosition = place.Position.Whole;
        // The fireball phase starts the complete retail mouth/fireball animation.
        // Its fireball cues calculate and spawn actors; leaving this
        // assignment out produces correct boss motion with a conspicuously silent attack.
        StartRidleyAnimation(state, place, RidleyBodySegment.Fireballing);
        state.Phase = RidleyPhase.CeresFireballShooting;
        state.PhaseTimer = 224;
        TickCeresRidleyFireballShooting(place, state);
    }

    /// <summary>Selects the next fireball target from the base position and a jitter drawn from the current random number.</summary>
    internal void TickCeresRidleyFireballShooting(RoomEnemyPlace place, RidleyEnemyState state)
    {
        // The attack choice reads RandomNumberSeed without generating a number.
        int random = _room.Random.Read();
        int jitter = BossRandomRolls.InLowerHalf(random)
            ? RidleyRandomDefinitions.FireballJitter.Of(random)
            : -RidleyRandomDefinitions.FireballJitter.Of(random);
        Position16<RoomPixels> target = CeresRidleyFireballTarget.Around(state.FireballBasePosition, jitter);
        AccelerateRidleyToward(place, state, target, 16);
        if (--state.PhaseTimer < 0)
        {
            state.HoverCounter = 0;
            state.Phase = RidleyPhase.CeresHovering;
        }
    }

    /// <summary>Advances the attack lunge until its position threshold hands control to the swoop.</summary>
    internal static void TickCeresRidleyLunge(
        RoomEnemyPlace place,
        RidleyEnemyState state,
        SamusState samus)
    {
        Position16<RoomPixels> target = CoordinateAxis.Difference(samus.Position.Y.Whole, 132) < 0
            ? samus.Position.Whole.WithY(64)
            : samus.Position.Whole - new Offset16<RoomPixels>(0, 68);
        AccelerateRidleyToward(place, state, target, 3);
        bool reached = IsWithinRidleyRectangle(place, target, new(2, 2));
        state.PhaseTimer--;
        if (reached || state.PhaseTimer < 0)
        {
            state.HoverCounter = 0;
            state.Phase = RidleyPhase.CeresHovering;
        }
    }

    /// <summary>Steers Ridley toward the swoop start point with the shared acceleration.</summary>
    internal static void TickCeresRidleySwoopMoveToPosition(
        RoomEnemyPlace place,
        RidleyEnemyState state)
    {
        AccelerateRidleyToward(place, state, new(192, 80), 15);
        if (CoordinateAxis.Difference(place.Position.Y.Whole, 96) < 0)
        {
            state.Phase = RidleyPhase.CeresSwoopDescendingAimingDown;
            state.PhaseTimer = 10;
            state.SwoopAngleAccumulator = RotationAngle.Zero;
        }
    }

    /// <summary>Runs the swoop arc and publishes the one-frame tail-fling request at its bottom.</summary>
    internal static void TickCeresRidleySwoopPhase(
        RidleyEnemyState state,
        RidleySwoopTurn turn,
        RotationAngle targetAngle,
        SignedEightEight targetSpeed,
        RidleyPhase nextPhase,
        int nextTimer)
    {
        UpdateRidleySwoopVelocity(state, turn, targetAngle, targetSpeed);
        if (--state.PhaseTimer < 0)
        {
            state.Phase = nextPhase;
            state.PhaseTimer = nextTimer;
        }
    }

    /// <summary>
    /// Moves Ridley's swoop speed toward <paramref name="targetSpeed"/>, turns his heading by <paramref name="turn"/>
    /// toward <paramref name="targetAngle"/>, stopping at it by whether their wrapped difference is negative, and derives
    /// his velocity from the two.
    /// </summary>
    internal static void UpdateRidleySwoopVelocity(
        RidleyEnemyState state,
        RidleySwoopTurn turn,
        RotationAngle targetAngle,
        SignedEightEight targetSpeed)
    {
        SignedEightEight speed = state.SwoopSpeed;
        if (speed < targetSpeed)
            speed = SignedEightEight.Min(targetSpeed, speed + SwoopSpeedStep);
        else if (speed > targetSpeed)
            speed = SignedEightEight.Max(targetSpeed, speed - SwoopSpeedStep);
        state.SwoopSpeed = speed;

        RotationAngle angle = turn.Turn(state.SwoopAngleAccumulator);
        // The comparison checks whether the difference is negative, not signed less-than. The heading wraps around the
        // turn, particularly onto the half-turn recovery target after a rightward swoop.
        bool negativeDifference = angle.TurnFrom(targetAngle) < 0;
        if (turn.Direction == RotationDirection.Clockwise ? negativeDifference : !negativeDifference)
            angle = targetAngle;
        state.SwoopAngleAccumulator = angle;
        TrigonometryAngle phase = angle.ToTrigonometryAngle();
        state.Velocity = state.Velocity.WithX(Trigonometry.MultiplySignedSine(speed, phase));
        state.Velocity = state.Velocity.WithY(Trigonometry.MultiplySignedSine(speed, phase.TurnedBy(TrigonometryAngle.QuarterTurn)));
    }

    /// <summary>How far the swoop speed moves toward its target each update: an eighth of a pixel an update.</summary>
    private static readonly SignedEightEight SwoopSpeedStep = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(32);

    /// <summary>Switches the encounter into the initial scripted retreat.</summary>
    private static void BeginCeresRidleyRetreat(RidleyEnemyState state)
    {
        state.Phase = RidleyPhase.CeresRealRetreatRising;
        state.MovementMinimum = state.MovementMinimum.WithY(CoordinateAxis.Wrap(-192));
    }

    /// <summary>Moves the 100-shot retreat toward the Baby retrieval staging point.</summary>
    internal static void TickCeresRidleyFakeRetreatMoveToPosition(
        RoomEnemyPlace place,
        RidleyEnemyState state)
    {
        // The shared Ridley phase steers toward (192,128). Ceres uses this only for
        // the 100-shot "fake retreat"; the low-energy branch bypasses the Baby retrieval.
        AccelerateRidleyToward(place, state, new(192, 128), 15);
        if (CoordinateAxis.Difference(place.Position.X.Whole, 192) >= 0)
            state.Phase = RidleyPhase.CeresFakeRetreatRising;
    }

    /// <summary>Raises Ridley above Y=32 before releasing the Baby into its independent fall.</summary>
    internal static void TickCeresRidleyFakeRetreatRising(
        RoomEnemyPlace place,
        RidleyEnemyState state)
    {
        // Ridley rises past Y=32 while the Baby still follows his original hand
        // anchor. Crossing that line starts the Baby's independent fall and a
        // wait of 21 counted past zero before Ridley changes to the retrieval pose.
        state.MovementMinimum = state.MovementMinimum.WithY(CoordinateAxis.Wrap(-192));
        AccelerateRidleyToward(place, state, Position16<RoomPixels>.Wrapped(192, -128), 15);
        if (CoordinateAxis.Difference(place.Position.Y.Whole, 32) < 0)
        {
            state.BabyAction = CeresBabyAction.Drop;
            state.Phase = RidleyPhase.CeresWaitBeforeRetrievingBaby;
            state.PhaseTimer = 21;
        }
    }

    /// <summary>Steers Ridley's hand toward the falling Baby and accepts overlapping radius-four grasp rectangles.</summary>
    internal static void TickCeresRidleyRetrieveBaby(RoomEnemyPlace place, RidleyEnemyState state)
    {
        // The Baby grab flies Ridley's hand toward the falling Baby. The collision compares
        // two radius-four rectangles centered on the hand and Baby points.
        AccelerateRidleyToward(place, state, state.BabyPosition.Whole - CeresRidleyHandToBaby, 4);

        Offset16<RoomPixels> fromHand = state.BabyPosition.Whole - (place.Position.Whole + CeresRidleyBabyScene.CeresBabyFeetAnchor);
        if (Math.Abs(fromHand.X) >= 8 || Math.Abs(fromHand.Y) >= 8)
        {
            return;
        }

        state.BabyAction = CeresBabyAction.CarriedInFeet;
        state.Velocity = state.Velocity.WithY(SignedEightEight.FromPixels(-2));
        BeginCeresRidleyRetreat(state);
    }

    /// <summary>Accelerates Ridley toward the offscreen retreat point and begins the 64-frame getaway delay.</summary>
    internal void TickCeresRidleyRetreat(RoomEnemyPlace place, RidleyEnemyState state)
    {
        // The retreat accelerates toward the off-screen point. The above-the-line check
        // is performed before the common movement pass; once the origin is above Y=-128
        // the 64-frame delay is installed and the delay falls through into its first tick.
        AccelerateRidleyToward(place, state, Position16<RoomPixels>.Wrapped(192, -128), 15);
        if (CoordinateAxis.AsSigned(place.Position.Y.Whole + 128) < 0)
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

        // The retreat recolours the chamber: its walls and the scenery that shares the retreat colours. The game also copied those
        // colours into Ridley's own sprite colours; his sprites here keep their own colours (#1544).
        state.Colours.ShowRetreat();
    }

    /// <summary>The retrieval steering target relative to the falling Baby.</summary>
    private static Offset16<RoomPixels> CeresRidleyHandToBaby => new(10, 56);

    /// <summary>Steers both 1/256-pixel velocity axes toward a target, raising a step that divides to zero to one.</summary>
    internal static void AccelerateRidleyToward(
        RoomEnemyPlace place,
        RidleyEnemyState state,
        Position16<RoomPixels> target,
        int divisor)
    {
        // Division truncates like the game's quotient and a zero quotient is promoted
        // to one, ensuring that even a one-pixel error continues to change velocity.
        divisor = RidleyInertia.Divisor(divisor);

        state.Velocity = new EightEightVector<RoomPixels>(
            AccelerateAxis(state.Velocity.X, place.Position.X.Whole, target.X, divisor),
            AccelerateAxis(state.Velocity.Y, place.Position.Y.Whole, target.Y, divisor));
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
        // The distance divided by the inertia, at most 32768; a step that divides to zero is raised to one.
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

    /// <summary>Applies Ridley's 1/256-pixel motion and arena clamps, invoking impact only at the strict left bound.</summary>
    internal void IntegrateRidleyMovement(
        RoomEnemyPlace place,
        RidleyEnemyState state,
        RidleyWallImpact wallImpact = RidleyWallImpact.None)
    {
        // The bounds clamp runs the impact only when proposed X is strictly below the
        // left bound, before zeroing X velocity. Vertical speed can meet its threshold
        // even when horizontal motion is slow; the right and vertical clamps do not call it.
        FixedCoordinate proposedX = IntegrateUnclampedAxis(place.Position.X, state.Velocity.X);
        FixedCoordinate proposedY = IntegrateUnclampedAxis(place.Position.Y, state.Velocity.Y);
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
            place.Position.X,
            state.Velocity.X,
            state.MovementMinimum.X,
            state.MovementMaximum.X);
        state.Velocity = state.Velocity.WithX(xVelocity);
        place.Position = place.Position.WithX(x);
        (FixedCoordinate y, SignedEightEight yVelocity) = IntegrateAxis(
            place.Position.Y,
            state.Velocity.Y,
            state.MovementMinimum.Y,
            state.MovementMaximum.Y);
        state.Velocity = state.Velocity.WithY(yVelocity);
        place.Position = place.Position.WithY(y);
    }

    /// <summary>Integrates one 1/256-pixel velocity into a position's fraction, in 256ths of a pixel, and clips to an arena bound.</summary>
    private static CeresRidleyAxisMotion IntegrateAxis(
        FixedCoordinate position,
        SignedEightEight velocity,
        int minimum,
        int maximum)
    {
        // Ridley's velocities are signed 1/256-pixel values that the game adds, in 256ths of a pixel, to the
        // ordinary 1/65536-pixel position's fraction.
        FixedCoordinate moved = position.MovedBy(velocity);

        if (CoordinateAxis.Difference(moved.Whole, minimum) < 0)
            return new CeresRidleyAxisMotion(moved.WithWhole(minimum), SignedEightEight.Zero);
        if (CoordinateAxis.Difference(moved.Whole, maximum) >= 0)
            return new CeresRidleyAxisMotion(moved.WithWhole(maximum), SignedEightEight.Zero);
        return new CeresRidleyAxisMotion(moved, velocity);
    }

    /// <summary>Adds a 1/256-pixel velocity to a position before boundary checks.</summary>
    private static FixedCoordinate IntegrateUnclampedAxis(FixedCoordinate position, SignedEightEight velocity) =>
        position.MovedBy(velocity);

    /// <summary>Tests an inclusive actor rectangle for hover, lunge or retrieval handoffs.</summary>
    internal static bool IsWithinRidleyRectangle(
        RoomEnemyPlace place,
        Position16<RoomPixels> center,
        Extent16<RoomPixels> radius) =>
        // The hover check tests overlap with the actor's rectangle, including its boundary.
        // A center-only test delays flight and attack handoffs until Ridley gets too close.
        CoordinateAxis.Distance(place.Position.X.Whole, center.X) < CoordinateAxis.Wrap(place.Radius.X + radius.X + 1) &&
            CoordinateAxis.Distance(place.Position.Y.Whole, center.Y) < CoordinateAxis.Wrap(place.Radius.Y + radius.Y + 1);

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
    internal static void StartRidleyAnimation(RidleyEnemyState state, RoomEnemyPlace place, RidleyBodySegment segment)
    {
        state.BodyPlayback = EnemyAnimationPlayback.Start(place, RidleyAnimations.StartingWith(segment));
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
            // The wing beat quickens with each quarter pixel of speed up to seven quarters (#1935).
            int wingStep = Math.Min(CeresRidleyWingDefinitions.AnimationTimerSteps.Length - 1, largestAxisVelocity.WholeQuarterPixels);
            int delta = CeresRidleyWingDefinitions.AnimationTimerSteps[wingStep];
            if (!state.Velocity.Y.IsNegative)
                delta >>= 1;
            state.WingAnimationTimerDelta = delta;
        }

        state.WingAnimationTimer -= state.WingAnimationTimerDelta;
        if (state.WingAnimationTimer >= 0)
            return;

        state.WingAnimationTimer = 32;
        state.WingBeatStep = (state.WingBeatStep + 1) % RidleySupplementalVisualDefinitions.WingBeatSteps;
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
        SpawnCeresRidleyGetawayWall(CeresGetawayChamberWallDefinitions.LeftWall, HorizontalDirection.Left);
        SpawnCeresRidleyGetawayWall(CeresGetawayChamberWallDefinitions.RightWall, HorizontalDirection.Right);
    }

    /// <summary>Initializes one wall actor at its place, on its side of the chamber, drawing nothing.</summary>
    private void SpawnCeresRidleyGetawayWall(Position16<RoomPixels> position, HorizontalDirection side)
    {
        RoomEnemyPlace wall = _places.FindFreePlace() ??
            throw new InvalidOperationException("Ceres Ridley getaway has no free enemy place for a turning-floor wall.");

        RoomEnemyDefinition definition = _room.Definitions.ReadEnemyDefinition(EnemyDefinitionId.CeresGetawayChamberWall);
        RoomEnemyPlacement population = new(
            EnemyDefinitionId.CeresGetawayChamberWall,
            Position: position,
            Contact: EnemyContact.None,
            Processing: EnemyProcessing.Animates | EnemyProcessing.OffScreen,
            Presence: EnemyPresence.Hidden,
            CollidesByFrameHitboxes: false,
            SpawnSettings: new CeresGetawayChamberWallSpawnSettings(side));
        _lifecycle.InitializePlaceFromDefinition(wall, population, definition);
        _lifecycle.RunInitializationAi(wall, EnemySpawnSurroundings.None);
        _places.CoverSpawn(wall);
    }
}
