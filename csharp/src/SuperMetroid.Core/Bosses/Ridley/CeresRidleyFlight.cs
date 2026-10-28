using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Rooms;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's flight: liftoff, hovering, fireballs, lunges, swoops, the fake retreat and taking the baby, and the movement integration they share.</summary>
[RoomLifetime]
internal sealed class CeresRidleyFlight
{
    /// <summary>Invokes the enemy AI table for Ridley's shared movement callbacks.</summary>
    private readonly IEnemyDispatch _dispatch;
    /// <summary>Provides room randomness and Ceres artwork state.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Offsets the private Baby/door drawing hook during a room quake.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues Ceres battle and getaway sound effects.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Resolves the door and Mode-7 wall enemy records beside Ridley.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Writes Ceres target colors and supplemental composite drawing.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Owns the shared Ridley encounter and its battle state.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CeresRidleyFlight(CeresRidleyFlightDependencies dependencies)
    {
        _dispatch = dependencies.Dispatch;
        _room = dependencies.Room;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _slotTable = dependencies.SlotTable;
        _presentation = dependencies.Presentation;
        _ridleyEncounter = dependencies.RidleyEncounter;
        _bossArtwork = dependencies.BossArtwork;
    }

    /// <summary>
    /// Ports the Ceres branch of <c>CeresRidley_Init</c> at $A6:A0F5. The shared routine also
    /// initializes Norfair Ridley ($E17F), but selecting that branch here would be fiction:
    /// this loader currently has no saved boss-bit input and the playable path is fresh Ceres.
    /// </summary>
    internal void InitializeCeresRidley(RoomEnemySlot slot)
    {
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Ceres Ridley requires native enemy slot zero, not slot {slot.SlotIndex}.");
        }

        // $A6:A10B-A12E clears the shared boss tilemap workspace, disables the minimap, and
        // marks the room explored. Those consumers are not yet represented by RoomEnemySystem;
        // every actor-owned write that follows is retained below.
        slot.PaletteRow = 7;
        slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.UsesExtendedSpritemap);

        // Native bits $1000 and $0400 are proven by the initializer, but $1000 has not yet
        // been given a cross-enemy semantic name. Preserve it raw; $0400 already has the
        // verified IgnoreSamusCollision identity used by the scheduler.
        slot.Properties = slot.Properties.With(EnemyProperties.BlocksPlasmaBeam);
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        slot.Position = slot.Position.WithWhole(new(186, 169));
        _ridleyEncounter.CeresStatus = CeresEscapeStatus.RidleyNotEscaped;

        _ridleyEncounter.Ridley = new RidleyEnemyState
        {
            Function = RidleyAiFunction.WaitForDoorTransition,
            FightMode = RidleyFightMode.NotFighting,
            HitCounter = 0,
            SpritemapPalette = 7,
            CommonDrawPalette = 7,
            MovementAnimationEnabled = false,
            FacingDirection = RidleyFacing.Left,
            IdleTailWhipEnabled = true,
            TailDamage = 15,
            Velocity = new EightEightVector<RoomPixels>(0, 0),
            MovementMinimum = new Position16<RoomPixels>(40, unchecked((ushort)-32)),
            MovementMaximum = new Position16<RoomPixels>(224, 176),
            BabyProgram = AnimationProgramCursor<CeresBabyProgram>.Start(CeresBabyProgram.Initial),
            BabyInstructionTimer = 1,
            BabyFunction = CeresBabyFunction.CarriedInArms,
            BabyCurrentPose = null,
            BabyPosition = FixedPosition<RoomPixels>.FromWhole(slot.Position.Whole + CeresRidleyBabyScene.CeresBabyArmsAnchor),
            WingFrame = 5,
            WingAnimationTimer = WrappingCountdown.Start(0),
            WingAnimationTimerDelta = 0,
            TailFunctionIndex = RidleyTailFunction.None,
            TailAngleDelta = 1,
            TailMinimumClockwiseAngle = RidleyTailDefinitions.InitialMinimumClockwise,
            TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.InitialMaximumCounterClockwise,
            ClockwiseWhipTarget = null,
            CounterClockwiseWhipTarget = null,
            TailWhipRequest = 0,
            TailExtensionSpeed = 240,
            IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentSpacing,
            TailSegments = RidleyTail.CreateInitialRidleyTailSegments(),
        };
        StartRidleyProgram(_ridleyEncounter.Ridley, slot, RidleyBodyProgram.Initial);
        // `$A6:A280` ends Ceres Ridley's initialization by queuing music zero (stop).
        _audio.QueueInitializationMusicDelayed8(MusicRequest.Stop);

        // WriteColorsToTargetPalette($140, $A6:E16F, $20) installs the Ceres door and Baby
        // Metroid container palettes. This runtime exposes the final target directly in
        // CGRAM, matching the established Ceres-door palette seam.
        (_bossArtwork.CeresRidleyColors ?? throw new InvalidOperationException(
            "Ceres Ridley requires installed start colors.")).ApplyStart(_presentation.Colours);

        // The native loop clears target-palette byte offsets $1E2..$1FE: OBJ palette seven,
        // colors one through fifteen. Color zero belongs to the shared transparent backdrop
        // and is intentionally left untouched.
        _presentation.BossColours().CeresRidleyRevealedObjects.Fill(RgbColor.Black);
    }

    /// <summary>Applies the crossing-frame -16 then +20 velocity changes before shared Ridley motion.</summary>
    internal static void TickCeresRidleyLiftoffDecelerating(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        // The fallthrough from A6AF deliberately applies both -16 and +20 on the crossing
        // frame. Do not combine these into a single acceleration outside this state.
        state.Velocity = state.Velocity.WithY(new SignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue + 20))));
        if (unchecked((short)(slot.Position.Y.Whole - 80)) < 0)
        {
            state.Function = RidleyAiFunction.CeresHovering;
            state.FightMode = RidleyFightMode.Fighting;
        }
    }

    /// <summary>Tests the scripted shot-count and Samus-energy exits, then admits attacks only inside the hover box.</summary>
    internal void TickCeresRidleyHover(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        // Ceres Ridley never loses the ordinary enemy-health word. His shot AI increments
        // this separate counter and A6E8 treats 100 contacts as the scripted win condition.
        if (state.HitCounter >= 100)
        {
            state.FightMode = RidleyFightMode.NotFighting;
            state.Function = RidleyAiFunction.CeresFakeRetreatMoveToPosition;
            TickCeresRidleyFakeRetreatMoveToPosition(slot, state);
            return;
        }

        // The other original termination route is Samus energy below 30. A synthetic room
        // fixture may omit Samus, in which case only the shot-counter route is meaningful.
        // $A6:A706 jumps into the real retreat, so its first acceleration runs this call.
        if (samus is not null && samus.Health < 30)
        {
            state.FightMode = RidleyFightMode.NotFighting;
            BeginCeresRidleyRetreat(state);
            TickCeresRidleyRetreat(slot, state);
            return;
        }

        // A6:A763 is the shared cooldown/return-to-hover movement: accelerate toward
        // ($00C0,$0064) using divisor table entry zero.
        AccelerateRidleyToward(slot, state, target: new(192, 100), divisorIndex: 0);
        if (!IsWithinRidleyRectangle(slot, new(192, 100), new(8, 8)))
        {
            // Shitroid_Func_2 returns zero on rectangle overlap. $A6:A6E8 therefore waits
            // only while Ridley is still outside the hover box, with $7C as the authentic
            // timeout. The previous translation inverted this branch and selected attacks
            // while he was still above the arena, making every fireball route hit its
            // intentional 48-frame bailout before the mouth animation could begin.
            state.HoverCounter++;
            if (state.HoverCounter < 124)
                return;
        }

        state.Function = CeresRidleyAttackDefinitions.SelectAttack(_room.Random.Read());
        state.HoverCounter = 0;
    }

    /// <summary>Steers Ridley into his fireball position and installs the mouth/fireball instruction stream.</summary>
    internal void TickCeresRidleyFireballMove(RoomEnemySlot slot, RidleyEnemyState state)
    {
        int verticalVelocity = state.Velocity.Y.SignedRawValue;
        int magnitude = Math.Max(128, Math.Abs(verticalVelocity));
        state.Velocity = state.Velocity.WithY(new SignedEightEight(unchecked((ushort)(verticalVelocity < 0 ? -magnitude : magnitude))));
        AccelerateRidleyToward(slot, state, new(slot.Position.X.Whole, 88), 0);

        if (unchecked((short)(slot.Position.Y.Whole - 80)) < 0)
        {
            state.HoverCounter++;
            if (state.HoverCounter >= 48)
                state.Function = RidleyAiFunction.CeresSwoopSetup;
            return;
        }
        if (unchecked((short)(slot.Position.Y.Whole - 128)) >= 0)
            return;

        state.FireballBasePosition = slot.Position.Whole;
        // Function $A782 installs the complete retail mouth/fireball instruction stream.
        // Its $E84D/$E904/$E909 commands calculate and spawn bank-$86 actors; leaving this
        // assignment out produces correct boss motion with a conspicuously silent attack.
        StartRidleyProgram(state, slot, RidleyBodyProgram.Fireballing);
        state.Function = RidleyAiFunction.CeresFireballShooting;
        state.FunctionTimer = WrappingCountdown.Start(224);
        TickCeresRidleyFireballShooting(slot, state);
    }

    /// <summary>Uses the current RNG word and native carry arithmetic to select the next fireball target.</summary>
    internal void TickCeresRidleyFireballShooting(RoomEnemySlot slot, RidleyEnemyState state)
    {
        // $A6:A7F9 reads RandomNumberSeed without generating a number.
        int random = _room.Random.Read();
        int jitter = unchecked((short)random) < 0 ? -(random & 7) : random & 7;
        // Both targets use ADC without CLC. Carry is clear on entry (the enemy AI dispatcher's
        // CLC/ADC at $A0:9083 and $A6:A7BA's in-range compare), so the X sum carries into Y.
        bool carry = false;
        Position16<RoomPixels> target = new(
            WordCarry.AddWithCarry(state.FireballBasePosition.X, unchecked((ushort)jitter), ref carry),
            WordCarry.AddWithCarry(state.FireballBasePosition.Y, unchecked((ushort)jitter), ref carry));
        AccelerateRidleyToward(slot, state, target, 0);
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (state.FunctionTimer.IsNegative)
        {
            state.HoverCounter = 0;
            state.Function = RidleyAiFunction.CeresHovering;
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

        Position16<RoomPixels> target = unchecked((short)(samus.Position.Y.Whole - 132)) < 0
            ? samus.Position.Whole.WithY(64)
            : samus.Position.Whole - new Offset16<RoomPixels>(0, 68);
        AccelerateRidleyToward(slot, state, target, 13);
        bool reached = IsWithinRidleyRectangle(slot, target, new(2, 2));
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (reached || state.FunctionTimer.IsNegative)
        {
            state.HoverCounter = 0;
            state.Function = RidleyAiFunction.CeresHovering;
        }
    }

    /// <summary>Steers Ridley toward the swoop start point with the shared acceleration routine.</summary>
    internal static void TickCeresRidleySwoopMoveToPosition(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        AccelerateRidleyToward(slot, state, new(192, 80), 1);
        if (unchecked((short)(slot.Position.Y.Whole - 96)) < 0)
        {
            state.Function = RidleyAiFunction.CeresSwoopDescendingAimingDown;
            state.FunctionTimer = WrappingCountdown.Start(10);
            state.SwoopAngleAccumulator = RotationAngle.Zero;
        }
    }

    /// <summary>Runs the swoop arc and publishes the one-frame tail-fling request at its bottom.</summary>
    internal static void TickCeresRidleySwoopPhase(
        RidleyEnemyState state,
        int angleDelta,
        int targetAngle,
        int targetMagnitude,
        RidleyAiFunction nextFunction,
        int nextTimer)
    {
        UpdateRidleySwoopVelocity(state, angleDelta, targetAngle, targetMagnitude);
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (state.FunctionTimer.IsNegative)
        {
            state.Function = nextFunction;
            state.FunctionTimer = WrappingCountdown.FromRaw(nextTimer);
        }
    }

    /// <summary>Applies native signed-word comparisons while reversing Ridley's swoop velocity.</summary>
    internal static void UpdateRidleySwoopVelocity(
        RidleyEnemyState state,
        int angleDelta,
        int targetAngle,
        int targetMagnitude)
    {
        int magnitude = state.SwoopSpeedMagnitude;
        if (magnitude < targetMagnitude)
            magnitude = Math.Min(targetMagnitude, magnitude + 32);
        else if (magnitude > targetMagnitude)
            magnitude = Math.Max(targetMagnitude, magnitude - 32);
        state.SwoopSpeedMagnitude = magnitude;

        RotationAngle angle = state.SwoopAngleAccumulator.AddRaw(angleDelta);
        // Native CMP branches on N, not signed less-than (N xor V). The heading wraps around the
        // turn, particularly onto the half-turn recovery target after a rightward swoop.
        bool negativeDifference = unchecked((short)(angle.RawValue - targetAngle)) < 0;
        if (angleDelta < 0 ? negativeDifference : !negativeDifference)
            angle = RotationAngle.NormalizeRaw(targetAngle);
        state.SwoopAngleAccumulator = angle;
        ByteAngle phase = angle.ToByteAngle();
        state.Velocity = state.Velocity.WithX(new SignedEightEight(Trigonometry.MultiplySignedSine((ushort)magnitude, phase)));
        state.Velocity = state.Velocity.WithY(new SignedEightEight(Trigonometry.MultiplySignedSine((ushort)magnitude, phase.AddTableUnits(ByteAngle.QuarterTurn.RawValue))));
    }

    /// <summary>Switches the encounter into the initial scripted retreat.</summary>
    private static void BeginCeresRidleyRetreat(RidleyEnemyState state)
    {
        state.Function = RidleyAiFunction.CeresRealRetreatRising;
        state.MovementMinimum = state.MovementMinimum.WithY(unchecked((ushort)-192));
    }

    /// <summary>Moves the 100-shot retreat toward the Baby retrieval staging point.</summary>
    internal static void TickCeresRidleyFakeRetreatMoveToPosition(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        // Shared Ridley function $BD9A steers toward ($C0,$80). Ceres uses this only for
        // the 100-shot "fake retreat"; the low-energy branch bypasses the Baby retrieval.
        AccelerateRidleyToward(slot, state, new(192, 128), 1);
        if (unchecked((short)(slot.Position.X.Whole - 192)) >= 0)
            state.Function = RidleyAiFunction.CeresFakeRetreatRising;
    }

    /// <summary>Raises Ridley above Y=$20 before releasing the Baby into its independent fall.</summary>
    internal static void TickCeresRidleyFakeRetreatRising(
        RoomEnemySlot slot,
        RidleyEnemyState state)
    {
        // $BD:BC raises Ridley past Y=$20 while the Baby still follows his original hand
        // anchor. Crossing that line starts the Baby's independent falling function and a
        // 21-underflow wait before Ridley changes to the retrieval pose.
        state.MovementMinimum = state.MovementMinimum.WithY(unchecked((ushort)-192));
        AccelerateRidleyToward(slot, state, new(192, unchecked((ushort)-128)), 1);
        if (unchecked((short)(slot.Position.Y.Whole - 32)) < 0)
        {
            state.BabyFunction = CeresBabyFunction.Drop;
            state.Function = RidleyAiFunction.CeresWaitBeforeRetrievingBaby;
            state.FunctionTimer = WrappingCountdown.Start(21);
        }
    }

    /// <summary>Steers Ridley's hand toward the falling Baby and accepts overlapping radius-four grasp rectangles.</summary>
    internal static void TickCeresRidleyRetrieveBaby(RoomEnemySlot slot, RidleyEnemyState state)
    {
        // $BE03 flies Ridley's hand toward the falling Baby. The native collision compares
        // two radius-four rectangles centered on the hand and Baby points.
        AccelerateRidleyToward(slot, state, state.BabyPosition.Whole - CeresRidleyHandToBaby, 12);

        Offset16<RoomPixels> fromHand = state.BabyPosition.Whole - (slot.Position.Whole + CeresRidleyBabyScene.CeresBabyFeetAnchor);
        if (Math.Abs(fromHand.X) >= 8 || Math.Abs(fromHand.Y) >= 8)
        {
            return;
        }

        state.BabyFunction = CeresBabyFunction.CarriedInFeet;
        state.Velocity = state.Velocity.WithY(new SignedEightEight(unchecked((ushort)-512)));
        BeginCeresRidleyRetreat(state);
    }

    /// <summary>Accelerates Ridley toward the offscreen retreat point and begins the 64-frame getaway delay.</summary>
    internal void TickCeresRidleyRetreat(RoomEnemySlot slot, RidleyEnemyState state)
    {
        // $A6:A971 accelerates toward the off-screen point ($00C0,$FF80). The signed test
        // is performed before the common movement pass; once the origin is above Y=-128
        // the 64-frame delay is installed and $A6:A99D falls through into its first tick.
        AccelerateRidleyToward(slot, state, new(192, unchecked((ushort)-128)), 1);
        if (unchecked((short)(slot.Position.Y.Whole + 128)) < 0)
        {
            state.Function = RidleyAiFunction.CeresRetreatDelay;
            state.FunctionTimer = WrappingCountdown.Start(64);
            TickCeresRidleyRetreatDelay(state);
        }
    }

    /// <summary><c>Function_RidleyCeres_RealRetreat_WritePalettesSpawnWalls</c> ($A6:A9A0).</summary>
    internal void TickCeresRidleyRetreatDelay(RidleyEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (!state.FunctionTimer.IsNegative)
            return;

        // $A6:A9A5 spawns the two $E23F wall actors before the following
        // dispatcher switches BG mode. They are not decorative bookkeeping:
        // Mode 7 has no BG2, so these OBJ strips are what keep the chamber
        // visible around the rotating Ridley/Baby image.
        SpawnCeresRidleyGetawayWalls();
        state.Velocity = new EightEightVector<RoomPixels>(0, 0);
        state.TailFunctionIndex = RidleyTailFunction.None;
        state.Function = RidleyAiFunction.CeresPublishEscapeHandoff;

        // $A6:A9E3 replaces colors 1..15 of BG palette five. $A6:AA01 is
        // copied to colors 1..8 of BG palette two and OBJ palette seven.
        (_bossArtwork.CeresRidleyColors ?? throw new InvalidOperationException(
            "Ceres Ridley retreat requires installed colors.")).ApplyRetreat(_presentation.Colours);
    }

    /// <summary>The retrieval steering target relative to the falling Baby (<c>$BE03</c>).</summary>
    private static Offset16<RoomPixels> CeresRidleyHandToBaby => new(10, 56);

    /// <summary>Steers both 8.8 velocity axes toward a target, promoting zero hardware quotients to one.</summary>
    internal static void AccelerateRidleyToward(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        Position16<RoomPixels> target,
        int divisorIndex)
    {
        // Division truncates like the SNES hardware quotient and a zero quotient is promoted
        // to one, ensuring that even a one-pixel error continues to change velocity.
        int divisor = RidleyInertiaDefinitions.Divisor(divisorIndex);

        state.Velocity = new EightEightVector<RoomPixels>(
            AccelerateAxis(state.Velocity.X, slot.Position.X.Whole, target.X, divisor),
            AccelerateAxis(state.Velocity.Y, slot.Position.Y.Whole, target.Y, divisor));
    }

    /// <summary>
    /// One axis of <c>$A6:D62F</c> (Y) / <c>$A6:D6A6</c> (X). The SEC/SBC distance test's
    /// carry feeds the first ADC/SBC of the velocity chain, and each later ADC/SBC consumes
    /// the previous one's carry, so crossing zero while reversing adds one more unit.
    /// </summary>
    /// <param name="velocity">The axis velocity.</param>
    /// <param name="position">Ridley's whole coordinate on the axis, 0 through 65535.</param>
    /// <param name="target">The target's whole coordinate on the axis, 0 through 65535.</param>
    /// <param name="divisor">The inertia divisor, at least one.</param>
    private static SignedEightEight AccelerateAxis(SignedEightEight velocity, int position, int target, int divisor)
    {
        int difference = unchecked((short)(position - target));
        if (difference == 0)
            return velocity;
        bool carry = WordCarry.AfterCompare(checked((ushort)position), checked((ushort)target));
        bool targetBelow = difference < 0;
        // Hardware quotient of the distance, at most 32768; a zero quotient is promoted to one.
        int step = Math.Max(1, Math.Abs(difference) / divisor);
        int word = velocity.RawValue;

        if (targetBelow)
        {
            if (unchecked((short)word) < 0)
            {
                Add(step);
                Add(step);
            }
            Add(step);
            // CMP #$0500 : BMI keeps any value whose signed difference is negative.
            if (unchecked((short)(word - CeresRidleyBehavior.CeresRidleyMaximumSpeed)) >= 0)
                word = CeresRidleyBehavior.CeresRidleyMaximumSpeed;
        }
        else
        {
            if (unchecked((short)word) >= 0)
            {
                Subtract(step);
                Subtract(step);
            }
            Subtract(step);
            // CMP #$FB00 : BPL keeps any value whose signed difference is non-negative.
            if (unchecked((short)(word - CeresRidleyBehavior.CeresRidleyMinimumSpeed)) < 0)
                word = CeresRidleyBehavior.CeresRidleyMinimumSpeed;
        }
        return new SignedEightEight(checked((ushort)word));

        void Add(int operand) => word = WordCarry.AddWithCarry(checked((ushort)word), checked((ushort)operand), ref carry);
        void Subtract(int operand) => word = WordCarry.SubtractWithBorrow(checked((ushort)word), checked((ushort)operand), ref carry);
    }

    /// <summary>Applies Ridley's 8.8 motion and native arena clamps, invoking impact only at the strict left bound.</summary>
    internal void IntegrateRidleyMovement(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        RidleyWallImpact wallImpact = RidleyWallImpact.None)
    {
        // $A6:D86B calls the impact routine only when proposed X is strictly below the
        // left bound, before zeroing X velocity. Vertical speed can meet its threshold
        // even when horizontal motion is slow; the right and vertical clamps do not call it.
        FixedCoordinate proposedX = IntegrateUnclampedAxis(slot.Position.X, state.Velocity.X);
        FixedCoordinate proposedY = IntegrateUnclampedAxis(slot.Position.Y, state.Velocity.Y);
        state.HitRoomBoundary = unchecked((short)(proposedX.Whole - state.MovementMinimum.X)) < 0 ||
            unchecked((short)(proposedX.Whole - state.MovementMaximum.X)) >= 0 ||
            unchecked((short)(proposedY.Whole - state.MovementMinimum.Y)) < 0 ||
            unchecked((short)(proposedY.Whole - state.MovementMaximum.Y)) >= 0;
        if (wallImpact == RidleyWallImpact.CeresWall && unchecked((short)(proposedX.Whole - state.MovementMinimum.X)) < 0 &&
            Math.Max(Math.Abs((int)state.Velocity.X.SignedRawValue),
                Math.Abs((int)state.Velocity.Y.SignedRawValue)) >= RidleyWallImpactDefinitions.MinimumSpeed)
        {
            _earthquake.EarthquakeType = RidleyWallImpactDefinitions.EarthquakeType;
            _earthquake.EarthquakeTimer = WrappingCountdown.Start(RidleyWallImpactDefinitions.Duration);
        }
        (FixedCoordinate x, int xVelocity) = IntegrateAxis(
            slot.Position.X,
            state.Velocity.X,
            state.MovementMinimum.X,
            state.MovementMaximum.X);
        state.Velocity = state.Velocity.WithX(new SignedEightEight(checked((ushort)xVelocity)));
        slot.Position = slot.Position.WithX(x);
        (FixedCoordinate y, int yVelocity) = IntegrateAxis(
            slot.Position.Y,
            state.Velocity.Y,
            state.MovementMinimum.Y,
            state.MovementMaximum.Y);
        state.Velocity = state.Velocity.WithY(new SignedEightEight(checked((ushort)yVelocity)));
        slot.Position = slot.Position.WithY(y);
    }

    /// <summary>Integrates one 8.8 velocity into the high byte of a 16-bit subposition and clips to an arena bound.</summary>
    private static CeresRidleyAxisMotion IntegrateAxis(
        FixedCoordinate position,
        SignedEightEight velocity,
        int minimum,
        int maximum)
    {
        // Ridley's velocities are signed 8.8 values that the original routine accumulates into the
        // *high* byte of the ordinary 16-bit subposition.
        FixedCoordinate moved = position.AddEightEightToHighFraction(velocity.RawValue);

        if (unchecked((short)(moved.Whole - minimum)) < 0)
            return new CeresRidleyAxisMotion(moved.WithWhole(checked((ushort)minimum)), 0);
        if (unchecked((short)(moved.Whole - maximum)) >= 0)
            return new CeresRidleyAxisMotion(moved.WithWhole(checked((ushort)maximum)), 0);
        return new CeresRidleyAxisMotion(moved, velocity.RawValue);
    }

    /// <summary>Adds an 8.8 velocity to a native position before boundary checks.</summary>
    private static FixedCoordinate IntegrateUnclampedAxis(FixedCoordinate position, SignedEightEight velocity) =>
        position.AddEightEightToHighFraction(velocity.RawValue);

    /// <summary>Tests an inclusive actor rectangle for hover, lunge or retrieval handoffs.</summary>
    internal static bool IsWithinRidleyRectangle(
        RoomEnemySlot slot,
        Position16<RoomPixels> center,
        Extent16<RoomPixels> radius) =>
        // $A9:EF06 tests overlap with the actor's rectangle, including its boundary.
        // A center-only test delays flight and attack handoffs until Ridley gets too close.
        Math.Abs(unchecked((short)(slot.Position.X.Whole - center.X))) < unchecked((ushort)(slot.Radius.X + radius.X + 1)) &&
            Math.Abs(unchecked((short)(slot.Position.Y.Whole - center.Y))) < unchecked((ushort)(slot.Radius.Y + radius.Y + 1));

    /// <summary>Advances the initial battle hold before Ridley's eye fade begins.</summary>
    internal static void TickCeresRidleyInitialDelay(RidleyEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (!state.FunctionTimer.IsNegative)
            return;

        state.Function = RidleyAiFunction.FadeInEyes;
        state.FadePaletteOffset = 0;
        state.FunctionTimer = WrappingCountdown.Start(0);
    }

    /// <summary>Restarts Ridley's native enemy instruction cursor and frame timer at a selected list.</summary>
    internal static void StartRidleyProgram(RidleyEnemyState state, RoomEnemySlot slot, RidleyBodyProgram program)
    {
        state.BodyProgram = AnimationProgramCursor<RidleyBodyProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Ports $A6:D97D-$D9EC after the common Ridley movement pass.</summary>
    internal static void TickRidleyWingAnimation(RidleyEnemyState state)
    {
        int absoluteXVelocity = Math.Abs(state.Velocity.X.SignedRawValue);
        int absoluteYVelocity = Math.Abs(state.Velocity.Y.SignedRawValue);
        int largestAxisVelocity = Math.Max(absoluteXVelocity, absoluteYVelocity);
        if (largestAxisVelocity == 0)
        {
            state.WingAnimationTimerDelta = 0;
        }
        else
        {
            // $A6:D9CF isolates bits 8..11 after multiplying the 8.8 velocity by four,
            // converts the resulting word-table byte index, and clamps that index to $E.
            int tableIndex = Math.Min(7, EightEight.WholeOf(largestAxisVelocity << 2) % 16);
            int delta = CeresRidleyBehavior.CeresRidleyWingAnimationDeltas[tableIndex];
            if (state.Velocity.Y.SignedRawValue >= 0)
                delta >>= 1;
            state.WingAnimationTimerDelta = delta;
        }

        state.WingAnimationTimer = state.WingAnimationTimer.AddFrames(-state.WingAnimationTimerDelta);
        if (!state.WingAnimationTimer.IsNegative)
            return;

        state.WingAnimationTimer = WrappingCountdown.Start(32);
        state.WingFrame = (state.WingFrame + 1) % 10;
    }

    /// <summary>Allocates two real enemy records for the OBJ wall strips shown during Mode 7.</summary>
    private void SpawnCeresRidleyGetawayWalls()
    {
        // SpawnEnemy consumes the first two free native slots and initializes the literal
        // population records embedded at $A6:AA2F/$AA3F. Keeping these as real slots makes
        // their layer-two OAM order, off-screen processing, and $F7A5 visibility dispatcher
        // flow through the same scheduler as the cartridge instead of painting host panels.
        SpawnCeresRidleyGetawayWall(new(8, 127), parameter1: 5);
        SpawnCeresRidleyGetawayWall(new(248, 127), parameter1: 6);
    }

    /// <summary>Initializes one native wall actor at its authored position and variant parameter.</summary>
    private void SpawnCeresRidleyGetawayWall(Position16<RoomPixels> position, int parameter1)
    {
        int slotIndex = _slotTable.FindFreeSlotIndex();
        if (slotIndex < 0)
            throw new InvalidOperationException("Ceres Ridley getaway has no free enemy slot for a Mode-7 wall.");

        RoomEnemyDefinition definition = _room.LoadedDefinitions.ReadEnemyDefinition(EnemyDefinitionId.CeresDoor);
        RoomEnemyPopulationRecord population = new(
            EnemyDefinitionId.CeresDoor,
            Position: position,
            InitializationParameter: 0,
            Properties: new EnemyPropertyWord(
                EnemyProperties.ProcessInstructions |
                EnemyProperties.ProcessOffScreen),
            ExtraProperties: new EnemyExtraPropertyWord(EnemyExtraProperties.None),
            Parameter1: parameter1,
            Parameter2: 0);
        RoomEnemySlot wall = _slotTable.Slots[slotIndex];
        _dispatch.InitializeSlotFromDefinition(wall, population, definition);
        _dispatch.RunInitializationAi(wall, EnemySpawnSurroundings.None);
        _slotTable.EnemyCount = unchecked((ushort)Math.Max(_slotTable.EnemyCount, slotIndex + 1));
        _slotTable.FirstFreeEnemyIndex = unchecked((ushort)((slotIndex + 1) * EnemySlotTable.EnemyIndexStride));
    }
}
