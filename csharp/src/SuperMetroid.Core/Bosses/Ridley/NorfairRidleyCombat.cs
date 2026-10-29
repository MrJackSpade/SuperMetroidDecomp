using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Norfair Ridley's arrival and fight: hovering, swoops, pogo and ground attacks, and grabbing Samus.</summary>
[RoomLifetime]
internal sealed class NorfairRidleyCombat
{
    /// <summary>Room services, level state, and native RNG for the Norfair fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Publishes tail-impact tremors to the room earthquake state.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Queues roars, tail impacts, and other enemy-bank sounds.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Publishes Ridley's health and transition palettes.</summary>
    private readonly EnemyPresentation _presentation;
    /// <summary>Owns fight state shared with cutscenes and room transitions.</summary>
    private readonly RidleyEncounter _ridleyEncounter;
    /// <summary>Advances Ridley's independent tail segments and their hit checks.</summary>
    private readonly RidleyTail _ridleyTail;
    /// <summary>Coordinates Ridley's dedicated projectile effects.</summary>
    private readonly RidleyProjectiles _ridleyProjectiles;
    /// <summary>Raises the arena liquid during Ridley's reveal.</summary>
    private readonly EnemyRoomFx _roomFx;
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork;


    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal NorfairRidleyCombat(NorfairRidleyCombatDependencies dependencies)
    {
        _bossArtwork = dependencies.BossArtwork;
        _room = dependencies.Room;
        _earthquake = dependencies.Earthquake;
        _audio = dependencies.Audio;
        _presentation = dependencies.Presentation;
        _ridleyEncounter = dependencies.RidleyEncounter;
        _ridleyTail = dependencies.RidleyTail;
        _ridleyProjectiles = dependencies.RidleyProjectiles;
        _roomFx = dependencies.RoomFx;
    }

    /// <summary>
    /// Ports the area-two branch of the shared initializer at $A6:A0F5. A defeated Ridley
    /// is deleted before any shared workspace or palette side effects, matching the native
    /// boss-bit gate; a live fight begins in layer five below the visible arena.
    /// </summary>
    internal void InitializeNorfairRidley(RoomEnemySlot slot)
    {
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                $"Lower Norfair Ridley requires native enemy slot zero, not slot {slot.SlotIndex}.");
        }

        if (_room.Services.Bosses.IsDefeated(BossBits.AreaBoss))
        {
            slot.Properties = slot.Properties.With(
                EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.Deleted |
                EnemyProperties.Invisible);
            return;
        }
        slot.Colouring = RidleyEnemyState.OwnColours;
        slot.ExtraProperties = slot.ExtraProperties.With(EnemyExtraProperties.UsesExtendedSpritemap);

        // Native property $1000 blocks Plasma Beam penetration. Its meaning is established
        // only for this boss, so preserve the raw bit instead of widening the shared enum.
        slot.Properties = slot.Properties.With(EnemyProperties.BlocksPlasmaBeam);
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        slot.Position = slot.Position.WithWhole(new(96, 394));
        slot.Layer = 5;

        _ridleyEncounter.Ridley = new RidleyEnemyState
        {
            Function = RidleyAiFunction.WaitForDoorTransition,
            FightMode = RidleyFightMode.NotFighting,
            SpritemapColouring = RidleyEnemyState.OwnColours,
            CommonDrawColouring = RidleyEnemyState.OwnColours,
            MovementAnimationEnabled = true,
            FacingDirection = RidleyFacing.Right,
            IdleTailWhipEnabled = false,
            TailDamage = 120,
            MovementMinimum = new Position16<RoomPixels>(64, 64),
            MovementMaximum = new Position16<RoomPixels>(224, 416),
            WingFrame = 0,
            WingAnimationTimer = WrappingCountdown.Start(0),
            WingAnimationTimerDelta = 0,
            TailFunctionIndex = RidleyTailFunction.None,
            TailAngleDelta = 1,
            TailMinimumClockwiseAngle = RidleyTailDefinitions.InitialMinimumClockwise,
            TailMaximumCounterClockwiseAngle = RidleyTailDefinitions.InitialMaximumCounterClockwise,
            ClockwiseWhipTarget = null,
            CounterClockwiseWhipTarget = null,
            TailExtensionSpeed = 240,
            IdealInterSegmentTailAngle = RidleyTailDefinitions.IdealInterSegmentSpacing,
            TailSegments = RidleyTail.CreateInitialRidleyTailSegments(),
        };
        CeresRidleyFlight.StartRidleyProgram(_ridleyEncounter.Ridley, slot, RidleyBodyProgram.Initial);

        // WriteColorsToTargetPalette($140, $A6:E1CF, $20) installs both body and manual
        // tail/wing source palettes. The following loops clear the two fifteen-color OBJ
        // ranges that the reveal gradually fills; color zero remains transparent.
        (_bossArtwork.Artwork?.NorfairRidleyColors ?? throw new InvalidOperationException(
            "Norfair Ridley requires installed colors."))
            .ApplyInitial(_presentation.Colours);
        _presentation.BossColours().NorfairRidleyBody.Fill(RgbColor.Black);
        _presentation.BossColours().NorfairRidleyTail.Fill(RgbColor.Black);
    }

    /// <summary>
    /// $A6:A478 advances an area-two-only fourteen-color fade every third dispatcher call.
    /// A zero pointer terminates the table and activates the shared tail/wing composition.
    /// </summary>
    internal void TickNorfairRidleyArenaReveal(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
        if (!state.FunctionTimer.IsNegative)
            return;

        state.FunctionTimer = WrappingCountdown.Start(2);
        int row = state.FadePaletteOffset;
        bool hasPalette;
        var colors = _bossArtwork.Artwork?.NorfairRidleyColors ?? throw new InvalidOperationException(
            "Norfair Ridley reveal requires installed colors.");
        if (row > NorfairRidleyPaletteDefinitions.RevealRowCount)
            throw new InvalidDataException($"Norfair Ridley reveal row {row} escaped its native terminator.");
        hasPalette = row < NorfairRidleyPaletteDefinitions.RevealRowCount;
        if (hasPalette)
            colors.ApplyReveal(_presentation.Colours, row);
        state.FadePaletteOffset++;
        if (hasPalette)
            return;

        RidleyLiquidMotion.Publish(_roomFx, state, RidleyLiquidDefinitions.BattleHeight,
            RidleyLiquidDefinitions.RiseVelocity, RidleyLiquidDefinitions.RiseDelay);
        state.FadePaletteOffset = 0;
        CeresRidleyFlight.StartRidleyProgram(state, slot, RidleyBodyProgram.TransitionToFlying);
        state.WingAnimationTimer = WrappingCountdown.Start(8);
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

        Position16<RoomPixels> target = new(state.FacingDirection != RidleyFacing.Left ? (ushort)96 : (ushort)192, 256);
        NorfairRidleyMovement.MoveNorfairRidleyToward(
            slot,
            state,
            target,
            ReadRidleyHealthMovementDivisorIndex(state));
        if (CeresRidleyFlight.IsWithinRidleyRectangle(slot, target, new(8, 8)))
            state.Function = RidleyAiFunction.NorfairSelectAttack;
    }

    /// <summary>Moves to the high side point and initializes the swoop angle timer.</summary>
    internal static void TickNorfairRidleySwoopMoveToStart(RoomEnemySlot slot, RidleyEnemyState state)
    {
        Position16<RoomPixels> target = new(state.FacingDirection != RidleyFacing.Left ? (ushort)64 : (ushort)192, 128);
        NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, target, divisorIndex: 1);
        if (!CeresRidleyFlight.IsWithinRidleyRectangle(slot, target, new(8, 8)))
            return;

        state.Function = RidleyAiFunction.NorfairSwoopAimDown;
        state.SwoopPhaseTimer = WrappingCountdown.Start(32);
        state.SwoopAngleAccumulator = RotationAngle.Zero;
    }

    /// <summary>Advances the swoop velocity arc and changes phase when its timer expires.</summary>
    internal static bool TickNorfairRidleySwoopPhase(
        RidleyEnemyState state,
        int angleDelta,
        int targetAngle,
        int targetMagnitude,
        RidleyAiFunction nextFunction,
        int nextTimer)
    {
        CeresRidleyFlight.UpdateRidleySwoopVelocity(state, angleDelta, targetAngle, targetMagnitude);
        if (!state.SwoopPhaseTimer.IsZero)
        {
            state.SwoopPhaseTimer = state.SwoopPhaseTimer.Decrement().Remaining;
            return false;
        }

        state.Function = nextFunction;
        state.SwoopPhaseTimer = WrappingCountdown.FromRaw(nextTimer);
        return true;
    }

    /// <summary>
    /// Ports $B5E5/$B613. These two states alternate side targets while Samus is spin
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
                samus is null ? (ushort)352 : Math.Min(samus.Position.Y.Whole, (ushort)352)),
            ReadRidleyHealthMovementDivisorIndex(state));
        state.TailWhipRequest = 1;

        if (samus is null || SamusPoseMetadata.ReadMovementType(samus) != SamusMovementType.SpinJumping)
        {
            BeginNorfairRidleyGroundAttack(state);
            return;
        }

        // $A6:B669 uses the existing RNG byte; it does not advance the generator.
        if (SharedRandomState.LowerByte(_room.Random.Read()) >= 128 && !state.Roaring && state.FacingDirection != RidleyFacing.Turning)
            CeresRidleyFlight.StartRidleyProgram(state, slot, RidleyBodyProgram.Fireballing);

        if (!NorfairRidleyMovement.TickRidleyFunctionTimer(state))
            return;

        state.Function = direction == VerticalDirection.Down
            ? RidleyAiFunction.NorfairPogoAscending
            : RidleyAiFunction.NorfairPogoDescending;
        state.FunctionTimer = WrappingCountdown.Start(128);
        SelectNorfairRidleyFacingInstruction(slot, state);
    }

    /// <summary>Resets tail extension and enters the ground fireball/pogo setup.</summary>
    internal static void BeginNorfairRidleyGroundAttack(RidleyEnemyState state)
    {
        state.TailExtensionSpeed = 240;
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
        if (unchecked((short)(slot.Position.Y.Whole - 288)) < 0)
        {
            SelectNorfairRidleyFacingInstruction(slot, state);
            state.Function = RidleyAiFunction.NorfairFireballMoveToHeight;
            state.FunctionTimer = WrappingCountdown.Start(32);
            TickNorfairRidleyGroundAttackMoveToHeight(slot, state, samus);
            return;
        }

        NorfairRidleyMovement.MoveNorfairRidleyToward(
            slot, state, new Position16<RoomPixels>(RidleyMovementTargets.GroundAttackX(state.FacingDirection), 288), divisorIndex: 0);
    }

    /// <summary>Settles at Y=288, sets the pogo tail, and starts a randomized attack timer.</summary>
    internal void TickNorfairRidleyGroundAttackMoveToHeight(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState? samus)
    {
        NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, slot.Position.Whole.WithY(288), divisorIndex: 0);
        if (!NorfairRidleyMovement.TickRidleyFunctionTimer(state))
            return;

        state.TailFunctionIndex = RidleyTailFunction.PogoSetup;
        _ridleyTail.TickRidleyPogoTail(slot, state, samus);
        InitializeNorfairRidleyPogoVelocity(state);
        state.Function = RidleyAiFunction.NorfairFireballAttack;
        state.FunctionTimer = WrappingCountdown.Start(_room.Random.Read() % 64 + 128);
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
            state.Velocity = state.Velocity.WithY(new SignedEightEight(unchecked((ushort)-Math.Max(
                512,
                Math.Abs((int)state.Velocity.Y.SignedRawValue)))));
            state.TailFunctionIndex = RidleyTailFunction.Neutral;
            state.TailAngleDelta = 1;
            BeginNorfairRidleyGrab(slot, state, samus);
            return;
        }

        int nextVertical = state.Velocity.Y.SignedRawValue + state.PogoDownwardAcceleration;
        state.Velocity = state.Velocity.WithY(new SignedEightEight(unchecked((ushort)Math.Min(nextVertical, 1536))));

        if (!RidleyTail.RidleyTailTouchesTerrain(state, level))
            return;

        RidleyTailSegment tip = state.TailSegments[6];
        _ridleyProjectiles.SpawnRidleyDust(tip.Position + new Offset16<RoomPixels>(0, 12), MiscDustAnimation.SmallDustCloud);
        _audio.QueueEnemySound(Library2Sound.Quake, maximumQueued: SoundQueueLimit.Max6);
        _earthquake.EarthquakeType = 13;
        _earthquake.EarthquakeTimer = WrappingCountdown.Start(4);
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

        int nextVertical = state.Velocity.Y.SignedRawValue + state.PogoUpwardAcceleration;
        if (nextVertical >= 0)
        {
            state.Velocity = state.Velocity.WithY(new SignedEightEight(0));
            state.Function = RidleyAiFunction.NorfairFireballAttack;
        }
        else
        {
            state.Velocity = state.Velocity.WithY(new SignedEightEight(unchecked((ushort)nextVertical)));
        }
    }

    /// <summary>
    /// Ports the four ROM-selected velocity tables consumed by $A6:B90F. The signedness
    /// of the previous horizontal velocity chooses direction; magnitudes and both vertical
    /// accelerations are compiled cartridge definitions indexed by Ridley's health stage.
    /// </summary>
    internal void InitializeNorfairRidleyPogoVelocity(RidleyEnemyState state)
    {
        // The native routine samples the current word; it does not advance shared RNG.
        int randomIndex = _room.Random.Read() & 3;
        var definition = RidleyPogo.Read(randomIndex, Math.Min(state.HealthStage, 3) + 2);
        state.PogoUpwardAcceleration = definition.UpwardAcceleration;
        state.PogoDownwardAcceleration = definition.DownwardAcceleration;
        state.Velocity = state.Velocity.WithY(definition.Velocity.Y);
        SignedEightEight horizontal = definition.Velocity.X;
        state.Velocity = state.Velocity.WithX(state.Velocity.X.SignedRawValue < 0
            ? SignedEightEight.FromSignedRaw(unchecked((short)-horizontal.SignedRawValue))
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

        // The native lunge is a passing attack, not indefinite pursuit. Test the
        // previous movement's boundary result before applying another acceleration.
        int deltaX = unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole));
        bool passedSamus = state.FacingDirection == RidleyFacing.Right ? deltaX >= 0 : deltaX < 0;
        if (state.HitRoomBoundary ||
            (passedSamus && unchecked((short)(Math.Abs(deltaX) - RidleyLungeDefinitions.PassDistance)) >= 0) ||
            unchecked((short)(slot.Position.Y.Whole + RidleyLungeDefinitions.ClawHeight - samus.Position.Y.Whole)) >= 0)
        {
            HandleNorfairRidleyMissedLunge(slot, state);
            return;
        }

        int sideOffset = state.FacingDirection != RidleyFacing.Left ? 16 : -16;
        int divisorIndex = RidleyMovementTargets.GrabDivisorIndexes(Math.Min(state.HealthStage, 3));
        NorfairRidleyMovement.MoveNorfairRidleyToward(
            slot,
            state,
            samus.Position.Whole + new Offset16<RoomPixels>((short)sideOffset, -4),
            divisorIndex);

        if (RidleyClawOverlapsSamus(slot, state, samus, new Extent16<RoomPixels>(8, 12)))
        {
            // $A6:BB56-$BB5D reverses the lunge before the carry setup accelerates.
            state.Velocity = state.Velocity.WithY(new SignedEightEight(unchecked((ushort)-state.Velocity.Y.RawValue)));
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
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        if (slot.Health == 0)
        {
            StartNorfairRidleyDeathSequence(slot, state);
            // $A6:BB8C tail-jumps to $C538 on the same successful zero-health grab.
            TickNorfairRidleyMoveToDeathSpot(slot, state);
            return;
        }
        // $BB8F falls through $BBC4 on the grabbing update.
        BeginNorfairRidleyCarry(slot, state);
    }

    /// <summary>Chooses a facing-dependent carry anchor and starts Ridley's 32-frame move.</summary>
    internal static void BeginNorfairRidleyCarry(RoomEnemySlot slot, RidleyEnemyState state)
    {
        state.Target = new Position16<RoomPixels>(
            RidleyMovementTargets.CarryAnchorX(state.FacingDirection),
            unchecked((short)(slot.Position.Y.Whole - 320)) < 0
                ? (ushort)256
                : unchecked((ushort)(slot.Position.Y.Whole - 64)));
        state.Function = RidleyAiFunction.NorfairCarryMoveToAnchor;
        state.FunctionTimer = WrappingCountdown.Start(32);
        NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, state.Target, 0);
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
    private static short DecayRidleyGrabOffset(int offset)
    {
        if (offset > 0)
            return (short)Math.Max(0, offset - 4);
        if (offset < 0)
            return (short)Math.Min(0, offset + 4);
        return 0;
    }

    /// <summary>Tests Samus's radius against the facing and foot-frame adjusted claw point.</summary>
    internal static bool RidleyClawOverlapsSamus(
        RoomEnemySlot slot,
        RidleyEnemyState state,
        SamusState samus,
        Extent16<RoomPixels> radius) =>
        Math.Abs(unchecked((short)(samus.Position.X.Whole - GetNorfairRidleyClawX(slot, state)))) <
            samus.Kinematics.Radius.X + radius.X &&
        Math.Abs(unchecked((short)(samus.Position.Y.Whole - GetNorfairRidleyClawY(slot, state)))) <
            samus.Kinematics.Radius.Y + radius.Y;

    /// <summary>Applies the facing-dependent claw X offset to Ridley's body position.</summary>
    private static ushort GetNorfairRidleyClawX(RoomEnemySlot slot, RidleyEnemyState state) =>
        unchecked((ushort)(slot.Position.X.Whole + RidleyClawOffsets.ReadX(state.FacingDirection)));

    /// <summary>Applies the current feet-frame claw Y offset to Ridley's body position.</summary>
    private static ushort GetNorfairRidleyClawY(RoomEnemySlot slot, RidleyEnemyState state) =>
        unchecked((ushort)(slot.Position.Y.Whole + RidleyClawOffsets.ReadY(state.FeetDistanceIndex)));

    /// <summary>Starts a turn only when Ridley faces away from the arena half identified by X bit seven.</summary>
    internal static void SelectNorfairRidleyFacingInstruction(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (state.FacingDirection == RidleyFacing.Turning)
            return;
        // D955 reads the word at Enemy.XPosition-1: its sign is bit 7 of
        // the low X byte, not the sign of the full coordinate. Keep an inward
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
        slot.InstructionTimer = WrappingCountdown.Start(2);
    }

    /// <summary>Selects a hover acceleration divisor from Ridley's capped health stage.</summary>
    internal static int ReadRidleyHealthMovementDivisorIndex(RidleyEnemyState state) =>
        RidleyMovementTargets.HoverDivisorIndexes(Math.Min(state.HealthStage, 3));

    /// <summary>Checks the validated pose movement type against Ridley's grabbable states.</summary>
    internal static bool SamusMovementUsesRidleyGrab(SamusState? samus)
    {
        if (samus is null)
            return false;
        SamusMovementType movement = SamusPoseMetadata.ReadMovementType(samus);
        return RidleyGrabs.CanGrab(movement);
    }

    /// <summary>
    /// <c>LungeIfPowerBombActiveAndNotHoldingSamus</c> at $A6:BD2C, run before the function on
    /// every main and even hurt frame. It reads $0CEE, which is set as soon as a power bomb is
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

        // Function $B84D resets both tail control words before the lunge.
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
        body.Properties = body.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.Function = RidleyAiFunction.NorfairReleaseSamus;
    }

    /// <summary>
    /// $A6:C538 keeps the already-grabbed Samus attached while accelerating Ridley toward
    /// the four-pixel rectangle centered at (128,328). Reaching it immediately tail-calls
    /// the roar setup instead of spending a host-only transition frame.
    /// </summary>
    internal static void TickNorfairRidleyMoveToDeathSpot(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        NorfairRidleyMovement.MoveNorfairRidleyToward(body, state, new Position16<RoomPixels>(128, 328),
            RidleyInertiaDefinitions.DeathDivisorIndex, RidleyInertiaDefinitions.DeathReversalBoost);
        if (CeresRidleyFlight.IsWithinRidleyRectangle(body, new(128, 328), new(4, 4)))
            BeginNorfairRidleyDeathRoar(body, state);
    }

    /// <summary>Ports $A6:C53E: select the death-roar list and arm its 32-frame move.</summary>
    internal static void BeginNorfairRidleyDeathRoar(
        RoomEnemySlot body,
        RidleyEnemyState state)
    {
        CeresRidleyFlight.StartRidleyProgram(state, body, RidleyBodyProgram.DeathRoar);
        state.Function = RidleyAiFunction.NorfairDeathExplosions;
        state.FunctionTimer = WrappingCountdown.Start(32);
    }

    /// <summary>Ports $A6:BA85: death has priority over power-bomb dodge and ordinary retreat.</summary>
    private void HandleNorfairRidleyMissedLunge(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (state.ZeroHealthLungeCount >= RidleyLungeDefinitions.DeathLungeLimit)
        {
            StartNorfairRidleyDeathSequence(slot, state);
            BeginNorfairRidleyDeathRoar(slot, state);
            return;
        }
        if (_audio.PowerBomb is { IsArmed: true })
        {
            state.Function = RidleyAiFunction.NorfairReturnToArena;
            TickNorfairRidleyPowerBombDodge(slot, state);
            return;
        }
        state.Function = RidleyAiFunction.NorfairHoverSetup;
        state.TailWhipRequest = 1;
    }

    /// <summary>Ports $A6:BD4E, the power-bomb branch of missed-lunge recovery.</summary>
    internal void TickNorfairRidleyPowerBombDodge(RoomEnemySlot slot, RidleyEnemyState state)
    {
        if (_audio.PowerBomb is not { IsArmed: true } bomb)
        {
            state.FightMode = RidleyFightMode.Fighting;
            state.Function = state.GrabState == RidleyGrabState.Released
                ? RidleyAiFunction.NorfairSelectAttack : RidleyAiFunction.NorfairCarrySetup;
            return;
        }
        state.FightMode = RidleyFightMode.PowerBombDodge;
        Position16<RoomPixels> target = new(
            unchecked((short)(bomb.Position.X - RidleyLungeDefinitions.DodgeSplitX)) >= 0
                ? RidleyLungeDefinitions.DodgeLeftX : RidleyLungeDefinitions.DodgeRightX,
            unchecked((short)(bomb.Position.Y - RidleyLungeDefinitions.DodgeSplitY)) >= 0
                ? RidleyLungeDefinitions.DodgeUpperY : RidleyLungeDefinitions.DodgeLowerY);
        NorfairRidleyMovement.MoveNorfairRidleyToward(slot, state, target, ReadRidleyHealthMovementDivisorIndex(state));
    }

    /// <summary>$A6:B8A9 chooses the next bounce's direction from position and the current seed.</summary>
    internal void SetRidleyPogoHorizontalDirection(RoomEnemySlot slot, RidleyEnemyState state, SamusState? samus)
    {
        if (state.Velocity.X.RawValue == 0)
            state.Velocity = state.Velocity.WithX(SignedEightEight.FromSignedRaw(RoomGridConversions.OffsetWithinScreen(slot.Position.X.Whole) < 128 ? (short)192 : (short)-192));
        bool negative = state.Velocity.X.SignedRawValue < 0;
        bool reverse;
        if ((short)(slot.Position.X.Whole - state.MovementMinimum.X) < 0) reverse = negative;
        else if ((short)(slot.Position.X.Whole - state.MovementMaximum.X) >= 0) reverse = !negative;
        else
        {
            bool oppositeSigns = ((ushort)(slot.Position.X.Whole - (samus?.Position.X.Whole ?? 0)) ^ state.Velocity.X.RawValue) > short.MaxValue;
            reverse = oppositeSigns ? _room.Random.Read() < 1365 : _room.Random.Read() >= 1365;
        }
        if (reverse) state.Velocity = state.Velocity.WithX(new SignedEightEight(unchecked((ushort)-state.Velocity.X.RawValue)));
    }
}
