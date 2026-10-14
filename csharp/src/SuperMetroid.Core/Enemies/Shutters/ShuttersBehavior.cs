using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Shutters.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>The Shutters species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ShuttersBehavior : IEnemySpecies
{
    /// <summary>Each growing section advances sixteen pixels before its final placement.</summary>
    private const ushort GrowingShutterSectionLength = 0x0010;
    /// <summary>Intermediate sections stop seven pixels inside their full-size endpoint.</summary>
    private const ushort GrowingShutterIntermediateInset = 0x0007;

    /// <summary>Ports <c>GrowingShutter_Init</c> at $A2:E9DA.</summary>
    internal void InitializeGrowingShutter(RoomEnemySlot slot)
    {
        var state = new GrowingShutterEnemyState(slot);
        _growingShutterStates[slot.SlotIndex] = state;

        // The initializer consumes init0 and the low extra-property word as a two-bit table
        // selector before clearing the latter. Keep native dispatch ordering independent
        // of the origin direction encoded below.
        int initialFunctionIndex = slot.ExtraProperties * 2 + slot.CurrentInstruction;
        state.Function = GrowingShutterDefinitions.InitialFunction(initialFunctionIndex);

        bool growsUp = slot.ExtraProperties != 0;
        state.GrowthLevel0OriginY = slot.Position.Y.Whole;
        state.GrowthLevel1OriginY = unchecked((ushort)(slot.Position.Y.Whole + (growsUp ? -8 : 8)));
        state.GrowthLevel2OriginY = unchecked((ushort)(slot.Position.Y.Whole + (growsUp ? -16 : 16)));
        state.GrowthLevel3OriginY = unchecked((ushort)(slot.Position.Y.Whole + (growsUp ? -24 : 24)));
        state.GrowthLevel = 0;

        (state.GrowthVelocity, state.GrowthSubvelocity) = GrowingShutterDefinitions.Speed(slot.Parameter2);

        slot.ExtraProperties = 0;
        InstallGrowingShutterProgram(
            slot,
            state,
            ShutterProgram.GrowingTenPixels,
            yRadius: 8);
    }

    /// <summary>Ports <c>GrowingShutter_Main</c> at $A2:EAB6.</summary>
    private void RunGrowingShutterMain(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Function)
        {
            case GrowingShutterFunction.WaitToGrowDownForTimer:
                if (AdvanceGrowingShutterTimer(slot))
                    ActivateGrowingShutter(slot, state, growsUp: false, camera);
                return;

            case GrowingShutterFunction.WaitToGrowUpForTimer:
                if (AdvanceGrowingShutterTimer(slot))
                    ActivateGrowingShutter(slot, state, growsUp: true, camera);
                return;

            case GrowingShutterFunction.WaitToGrowDownForProximity:
                if (samus is null)
                    throw new InvalidOperationException("Growing-shutter proximity AI requires Samus state.");
                if (IsSamusWithinShutterHorizontalDistance(slot, samus, slot.Parameter1))
                    ActivateGrowingShutter(slot, state, growsUp: false, camera);
                return;

            case GrowingShutterFunction.WaitToGrowUpForProximity:
                if (samus is null)
                    throw new InvalidOperationException("Growing-shutter proximity AI requires Samus state.");
                if (IsSamusWithinShutterHorizontalDistance(slot, samus, slot.Parameter1))
                    ActivateGrowingShutter(slot, state, growsUp: true, camera);
                return;

            case GrowingShutterFunction.GrowDown:
                GrowShutterDown(slot, state);
                return;

            case GrowingShutterFunction.GrowUp:
                if (samus is null)
                    throw new InvalidOperationException("Upward growing-shutter carry requires Samus state.");
                GrowShutterUp(slot, state, samus);
                return;

            default:
                throw new InvalidDataException(
                    $"Growing shutter function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>
    /// Native code tests the timer before decrementing it. Zero therefore activates on the
    /// current call, while a positive N waits N complete calls and activates on call N + 1.
    /// </summary>
    private static bool AdvanceGrowingShutterTimer(RoomEnemySlot slot)
    {
        if (slot.Parameter1 == 0)
            return true;
        slot.Parameter1--;
        return false;
    }

    /// <summary>Switches a timed or proximity shutter into growth and queues audible activation on screen.</summary>
    private void ActivateGrowingShutter(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        bool growsUp,
        Position16<RoomPixels> camera)
    {
        state.Function = growsUp
            ? GrowingShutterFunction.GrowUp
            : GrowingShutterFunction.GrowDown;
        QueueShutterActivationSoundIfOnScreen(slot, camera);
    }

    /// <summary>Ports the four growth stages at $A2:EB11-$EC12.</summary>
    private static void GrowShutterDown(RoomEnemySlot slot, GrowingShutterEnemyState state)
    {
        if (state.GrowthLevel >= 4)
            return;

        slot.Position = slot.Position.WithY(AddShutterVelocity(
            slot.Position.Y,
            state.GrowthVelocity,
            state.GrowthSubvelocity));
        ushort target = unchecked((ushort)(state.OriginForLevel() + GrowingShutterSectionLength));

        // BPL returns at equality. A section advances only after its center has passed the
        // full sixteen-pixel threshold, after which the first three sections are inset seven.
        if (unchecked((short)(target - slot.Position.Y.Whole)) >= 0)
            return;
        FinishGrowingShutterSection(slot, state, target, growsUp: false);
    }

    /// <summary>Ports the four upward growth stages at $A2:EC13-$ED20.</summary>
    private static void GrowShutterUp(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        SamusState samus)
    {
        if (state.GrowthLevel >= 4)
            return;

        state.PreviousYPosition = slot.Position.Y.Whole;
        slot.Position = slot.Position.WithY(AddShutterVelocity(
            slot.Position.Y,
            unchecked((short)(-state.GrowthVelocity - (state.GrowthSubvelocity == 0 ? 0 : 1))),
            unchecked((ushort)-state.GrowthSubvelocity)));
        ushort target = unchecked((ushort)(state.OriginForLevel() - GrowingShutterSectionLength));
        if (unchecked((short)(target - slot.Position.Y.Whole)) < 0)
        {
            CarrySamusWithGrowingShutter(slot, state, samus);
            return;
        }

        FinishGrowingShutterSection(slot, state, target, growsUp: true);
        CarrySamusWithGrowingShutter(slot, state, samus);
    }

    /// <summary>Adds upward displacement only while Samus is riding the growing platform.</summary>
    private static void CarrySamusWithGrowingShutter(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        SamusState samus)
    {
        if (!PlatformsBehavior.IsSamusRidingPlatform(slot, samus))
            return;

        short wholeDelta = unchecked((short)(slot.Position.Y.Whole - state.PreviousYPosition));
        if (wholeDelta < 0)
        {
            samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, unchecked((short)(wholeDelta)));
        }
    }

    /// <summary>Snaps a completed section, enlarges its collision radius, and selects the next artwork.</summary>
    internal static void FinishGrowingShutterSection(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        ushort target,
        bool growsUp)
    {
        bool finalSection = state.GrowthLevel == 3;
        slot.Position = slot.Position.WithWholeY(finalSection
            ? target
            : unchecked((ushort)(target + (growsUp
                ? GrowingShutterIntermediateInset
                : -GrowingShutterIntermediateInset))));
        state.GrowthLevel = unchecked((ushort)(state.GrowthLevel + 1));

        if (finalSection)
            return;

        ShutterProgram program = state.GrowthLevel switch
        {
            1 => ShutterProgram.GrowingTwentyPixels,
            2 => ShutterProgram.GrowingThirtyPixels,
            3 => ShutterProgram.FortyPixels,
            _ => throw new InvalidDataException(
                $"Growing shutter advanced to invalid level {state.GrowthLevel}."),
        };
        ushort radius = unchecked((ushort)(8 + state.GrowthLevel * 8));
        InstallGrowingShutterProgram(slot, state, program, radius);
    }

    /// <summary>Restarts the next growth pose and updates the shutter's vertical collision radius.</summary>
    private static void InstallGrowingShutterProgram(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        ShutterProgram program,
        ushort yRadius)
    {
        state.Program = AnimationProgramCursor<ShutterProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        slot.Radius = slot.Radius.WithY(yRadius);
    }
    /// <summary>Samus's four-pixel correction when pushing against a horizontal shutter.</summary>
    private const short HorizontalShutterManualPushPixels = 4;

    /// <summary>Ports <c>HorizontalShutter_Init</c> at $A2:F111/$A2:F11E.</summary>
    internal void InitializeHorizontalShutter(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new HorizontalShutterEnemyState(slot);
        _horizontalShutterStates[slot.SlotIndex] = state;

        ushort packedSpeedAndDirection = slot.CurrentInstruction;
        state.SpeedTableIndex = unchecked((byte)packedSpeedAndDirection);
        state.PrimaryDirection = unchecked((byte)(SnesEightEight.WholeOf(packedSpeedAndDirection)));
        state.ReactionDirection = unchecked((ushort)(state.PrimaryDirection ^ 1));

        int speedRecordOffset = state.SpeedTableIndex * 8;
        (state.RightVelocity, state.RightSubvelocity) = EnemyMath.ReadLinearEnemySpeed(
            unchecked((ushort)speedRecordOffset));
        (state.LeftVelocity, state.LeftSubvelocity) = EnemyMath.ReadLinearEnemySpeed(
            unchecked((ushort)(speedRecordOffset + 4)));

        state.MovedLeftRestParameter = unchecked((byte)slot.ExtraProperties);
        state.MovedRightRestParameter = SnesLittleEndian.HighByte(slot.ExtraProperties);
        state.MovedLeftRestTime = unchecked((ushort)(state.MovedLeftRestParameter << 4));
        state.MovedRightRestTime = unchecked((ushort)(state.MovedRightRestParameter << 4));

        state.TriggerMode = unchecked((byte)slot.Parameter1);
        state.InitialFunctionTableOffset = unchecked((ushort)(state.TriggerMode * 2));
        if (state.InitialFunctionTableOffset > 8)
        {
            throw new InvalidDataException(
                $"Horizontal shutter trigger mode {state.TriggerMode} exceeds its five-entry ROM table.");
        }
        state.TravelDistance = SnesLittleEndian.HighByte(slot.Parameter1);
        state.HorizontalProximityOrWaitTime = slot.Parameter2;
        state.FunctionTimer = NativeCountdown.FromRaw(slot.Parameter2);

        state.MinimumXPosition = slot.Position.X.Whole;
        state.MaximumXPosition = unchecked((ushort)(slot.Position.X.Whole + state.TravelDistance));
        if (state.PrimaryDirection == 0)
        {
            state.MaximumXPosition = slot.Position.X.Whole;
            state.MinimumXPosition = unchecked((ushort)(slot.Position.X.Whole - state.TravelDistance));
        }

        state.Function = HorizontalShutterFunction.Initial;
        state.MovingSamus = false;
        state.ShotActivated = false;
        state.PreviousSamusXPosition = samus?.Position.X.Whole ?? 0;
        state.PreviousSamusXSubposition = samus?.Kinematics.Position.X.Fraction ?? 0;
        slot.ExtraProperties = 0;
        InstallHorizontalShutterProgram(slot, state);
    }

    /// <summary>Ports <c>HorizontalShutter_Main</c> at $A2:F1DE.</summary>
    private void RunHorizontalShutterMain(
        RoomEnemySlot slot,
        HorizontalShutterEnemyState state,
        SamusState? samus,
        ushort controllerInput)
    {
        if (samus is null)
            throw new InvalidOperationException("Horizontal-shutter AI requires Samus state.");

        switch (state.Function)
        {
            case HorizontalShutterFunction.Initial:
                SelectInitialHorizontalShutterFunction(state);
                break;

            case HorizontalShutterFunction.WaitForTimer:
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsZero)
                {
                    state.FunctionTimer = NativeCountdown.FromRaw(state.HorizontalProximityOrWaitTime);
                    ActivateHorizontalShutter(state);
                }
                break;

            case HorizontalShutterFunction.WaitForHorizontalProximity:
                if (IsSamusWithinShutterHorizontalDistance(
                    slot,
                    samus,
                    state.HorizontalProximityOrWaitTime))
                {
                    ActivateHorizontalShutter(state);
                }
                break;

            case HorizontalShutterFunction.Activate:
                ActivateHorizontalShutter(state);
                break;

            case HorizontalShutterFunction.InitialNoOp:
            case HorizontalShutterFunction.PermanentNoOp:
                break;

            case HorizontalShutterFunction.MovingLeft:
                MoveHorizontalShutterLeft(slot, state, samus, controllerInput);
                break;

            case HorizontalShutterFunction.MovingRight:
                MoveHorizontalShutterRight(slot, state, samus, controllerInput);
                break;

            case HorizontalShutterFunction.StoppedAfterMovingLeft:
                RunHorizontalShutterStoppedAfterLeft(state);
                break;

            case HorizontalShutterFunction.StoppedAfterMovingRight:
                RunHorizontalShutterStoppedAfterRight(state);
                break;

            default:
                throw new InvalidDataException(
                    $"Horizontal shutter function $A2:{(ushort)state.Function:X4} is not translated.");
        }

        // $A2:F1E4-$A2:F211: an idle shutter reacts when Samus's four solid-enemy collision
        // words AND to this enemy's index and she has contact damage (e.g. screw attack).
        if (state.Function is not (HorizontalShutterFunction.MovingLeft or HorizontalShutterFunction.MovingRight) &&
            SamusContactsIdleShutter(slot, samus))
        {
            ReactHorizontalShutter(slot);
        }

        // The native main routine snapshots these after the dispatcher, including on no-op
        // frames. They are retained even though the known routines do not subsequently read
        // the host copies; doing so keeps debugger state and future bank-$A2 work faithful.
        state.PreviousSamusXPosition = samus.Position.X.Whole;
        state.PreviousSamusXSubposition = samus.Kinematics.Position.X.Fraction;
    }

    /// <summary>Decodes the population's trigger offset into timer, proximity, activation, or idle mode.</summary>
    internal static void SelectInitialHorizontalShutterFunction(HorizontalShutterEnemyState state)
    {
        state.Function = state.InitialFunctionTableOffset switch
        {
            0 => HorizontalShutterFunction.WaitForTimer,
            2 => HorizontalShutterFunction.WaitForHorizontalProximity,
            4 => HorizontalShutterFunction.Activate,
            6 or 8 => HorizontalShutterFunction.InitialNoOp,
            _ => throw new InvalidDataException(
                $"Horizontal shutter initial offset ${state.InitialFunctionTableOffset:X4} is invalid."),
        };
    }

    /// <summary>Starts movement in the population's primary direction.</summary>
    private static void ActivateHorizontalShutter(HorizontalShutterEnemyState state) =>
        state.Function = state.PrimaryDirection == 0
            ? HorizontalShutterFunction.MovingLeft
            : HorizontalShutterFunction.MovingRight;

    /// <summary>Moves left, carries Samus on contact, and enters rest or permanent stop at the minimum X.</summary>
    private static void MoveHorizontalShutterLeft(
        RoomEnemySlot slot,
        HorizontalShutterEnemyState state,
        SamusState samus,
        ushort controllerInput)
    {
        state.PreviousXPosition = slot.Position.X.Whole;
        state.MovingSamus = HorizontalShutterOverlapsSamus(slot, samus) &&
            unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0;
        slot.Position = slot.Position.WithX(AddShutterVelocity(
            slot.Position.X,
            state.LeftVelocity,
            state.LeftSubvelocity));
        if (state.MovingSamus)
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(
                SnesSignedSixteenSixteen.FromParts(unchecked((short)state.LeftVelocity), state.LeftSubvelocity));
            if ((controllerInput & (ushort)SnesButton.Right) != 0)
            {
                samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(unchecked((short)(HorizontalShutterManualPushPixels)), 0);
                samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithFractionY(0);
            }
        }

        if (unchecked((short)(state.MinimumXPosition - slot.Position.X.Whole)) < 0)
            return;
        if (state.MovedLeftRestTime == PermanentStopRestTime)
        {
            state.Function = HorizontalShutterFunction.PermanentNoOp;
            return;
        }
        state.FunctionTimer = NativeCountdown.FromRaw(state.MovedLeftRestTime);
        state.Function = HorizontalShutterFunction.StoppedAfterMovingLeft;
    }

    /// <summary>Moves right, carries Samus on contact, and enters rest or permanent stop at the maximum X.</summary>
    private static void MoveHorizontalShutterRight(
        RoomEnemySlot slot,
        HorizontalShutterEnemyState state,
        SamusState samus,
        ushort controllerInput)
    {
        state.PreviousXPosition = slot.Position.X.Whole;
        state.MovingSamus = HorizontalShutterOverlapsSamus(slot, samus) &&
            unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) >= 0;
        slot.Position = slot.Position.WithX(AddShutterVelocity(
            slot.Position.X,
            state.RightVelocity,
            state.RightSubvelocity));
        if (state.MovingSamus)
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(
                SnesSignedSixteenSixteen.FromParts(unchecked((short)state.RightVelocity), state.RightSubvelocity));
            if ((controllerInput & (ushort)SnesButton.Left) != 0)
            {
                samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(unchecked((short)(HorizontalShutterManualPushPixels)), 0);
                samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithFractionY(0);
            }
        }

        if (unchecked((short)(state.MaximumXPosition - slot.Position.X.Whole)) > 0)
            return;
        if (state.MovedRightRestTime == PermanentStopRestTime)
        {
            state.Function = HorizontalShutterFunction.PermanentNoOp;
            return;
        }
        state.FunctionTimer = NativeCountdown.FromRaw(state.MovedRightRestTime);
        state.Function = HorizontalShutterFunction.StoppedAfterMovingRight;
    }

    /// <summary>Counts down the left-end rest before reversing or waiting for Samus proximity.</summary>
    private static void RunHorizontalShutterStoppedAfterLeft(HorizontalShutterEnemyState state)
    {
        NativeWordCounterStep timer = state.FunctionTimer.Decrement();
        state.FunctionTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return;
        state.Function = state.TriggerMode == 1 && state.PrimaryDirection != 0
            ? HorizontalShutterFunction.WaitForHorizontalProximity
            : HorizontalShutterFunction.MovingRight;
    }

    /// <summary>Counts down the right-end rest before reversing or waiting for Samus proximity.</summary>
    private static void RunHorizontalShutterStoppedAfterRight(HorizontalShutterEnemyState state)
    {
        NativeWordCounterStep timer = state.FunctionTimer.Decrement();
        state.FunctionTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return;
        state.Function = state.TriggerMode == 1 && state.PrimaryDirection == 0
            ? HorizontalShutterFunction.WaitForHorizontalProximity
            : HorizontalShutterFunction.MovingLeft;
    }

    /// <summary>Tests shutter and Samus radius boxes before applying horizontal carry.</summary>
    private static bool HorizontalShutterOverlapsSamus(RoomEnemySlot slot, SamusState samus) =>
        EnemyCollision.RadiusBoxesOverlap(
            slot.Position.Whole,
            slot.Radius,
            samus.Position.Whole,
            samus.Kinematics.Radius);

    /// <summary>Ports the stationary/manual ejection portion of touch AI $A2:F3D8.</summary>
    private void TouchHorizontalShutter(
        RoomEnemySlot slot,
        SamusState samus,
        ushort controllerInput)
    {
        HorizontalShutterEnemyState state = RequireHorizontalShutterState(slot);
        if (state.Function != HorizontalShutterFunction.PermanentNoOp)
            return;

        bool samusIsLeft = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0;
        if (samusIsLeft && (controllerInput & (ushort)SnesButton.Right) != 0)
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeX(-HorizontalShutterManualPushPixels).WithFractionY(0);
        }
        else if (!samusIsLeft && (controllerInput & (ushort)SnesButton.Left) != 0)
        {
            samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeX(HorizontalShutterManualPushPixels).WithFractionY(0);
        }
    }

    /// <summary>Ports the movement-trigger tail shared by shot and power-bomb AI $A2:F41A.</summary>
    private void ReactHorizontalShutter(RoomEnemySlot slot)
    {
        HorizontalShutterEnemyState state = RequireHorizontalShutterState(slot);
        if (state.InitialFunctionTableOffset < 6)
            return;

        if (state.InitialFunctionTableOffset == 6)
        {
            if (state.ShotActivated)
                return;
            state.ShotActivated = true;
        }

        if (state.Function is HorizontalShutterFunction.MovingLeft or HorizontalShutterFunction.MovingRight)
            return;

        state.ReactionDirection ^= 1;
        state.Function = state.ReactionDirection == 0
            ? HorizontalShutterFunction.MovingLeft
            : HorizontalShutterFunction.MovingRight;
    }

    /// <summary>Restarts the stationary shutter pose after movement or reaction.</summary>
    private static void InstallHorizontalShutterProgram(RoomEnemySlot slot, HorizontalShutterEnemyState state)
    {
        state.Program = AnimationProgramCursor<ShutterProgram>.Start(ShutterProgram.Horizontal);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Library-two sound queued when an on-screen shutter begins moving.</summary>
    private const ushort ShutterActivationSound = 0x000e;
    /// <summary>Rest-time sentinel selecting a permanent stop at an endpoint.</summary>
    private const ushort PermanentStopRestTime = 0x0ff0;

    /// <summary>Per-slot growth state for room-loaded expanding shutters.</summary>
    internal readonly GrowingShutterEnemyState?[] _growingShutterStates = new GrowingShutterEnemyState?[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Per-slot movement state for shootable, destroyable, and Kamer vertical shutters.</summary>
    internal readonly VerticalShutterEnemyState?[] _verticalShutterStates = new VerticalShutterEnemyState?[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Per-slot movement state for horizontal shootable shutters.</summary>
    internal readonly HorizontalShutterEnemyState?[] _horizontalShutterStates = new HorizontalShutterEnemyState?[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Frame camera used to admit activation sound only for on-screen shutters.</summary>
    private Position16<RoomPixels> _shutterCamera;

    /// <summary>Last library-two shutter sound queued during the current enemy frame.</summary>
    public ushort? LastShutterSoundEffect { get; private set; }

    /// <summary>Clears every shutter slot state and sound request on room change.</summary>
    internal void ResetShutterRoomState(Position16<RoomPixels> camera)
    {
        Array.Clear(_growingShutterStates);
        Array.Clear(_verticalShutterStates);
        Array.Clear(_horizontalShutterStates);
        _shutterCamera = camera;
        LastShutterSoundEffect = null;
    }

    /// <summary>Captures the current camera and clears the prior frame's activation sound.</summary>
    internal void BeginShutterFrame(Position16<RoomPixels> camera)
    {
        _shutterCamera = camera;
        LastShutterSoundEffect = null;
    }

    /// <summary>Includes shootable, destroyable, and Kamer definitions in the shared vertical AI.</summary>
    internal static bool IsVerticalShutterDefinition(EnemyDefinitionId definition) => definition is
        EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.ShutterDestroyable or
        EnemyDefinitionId.Kamer;

    /// <summary>Adds a signed 16.16 velocity using the same wrapping carry as ADC.</summary>
    private static SnesFixedPosition AddShutterVelocity(
        SnesFixedPosition position,
        short wholeVelocity,
        ushort subvelocity)
    {
        uint fixedPosition = position.RawValue;
        int velocity = unchecked((SnesSixteenSixteen.FromWhole(wholeVelocity)) | subvelocity);
        fixedPosition = unchecked(fixedPosition + (uint)velocity);
        return SnesFixedPosition.FromRaw(fixedPosition);
    }

    /// <summary>
    /// The shared idle-shutter contact test at $A2:EEE9 and $A2:F1F6: the bitwise AND of
    /// Samus's four solid-enemy collision words equals this enemy (one or more directions
    /// hit it and the rest hold $FFFF), and her contact-damage index is nonzero.
    /// </summary>
    private static bool SamusContactsIdleShutter(RoomEnemySlot slot, SamusState samus)
    {
        IReadOnlyList<ushort> collisions = samus.Kinematics.SolidEnemyCollisionIndexes;
        ushort combined = unchecked((ushort)(collisions[0] & collisions[1] & collisions[2] & collisions[3]));
        return combined != 0xffff &&
            combined == slot.NativeIndex &&
            samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None;
    }

    /// <summary>Tests the native strict, wrapped absolute-X proximity threshold.</summary>
    private static bool IsSamusWithinShutterHorizontalDistance(
        RoomEnemySlot slot,
        SamusState samus,
        ushort maximumDistance)
    {
        ushort distance = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        if (unchecked((short)distance) < 0)
            distance = unchecked((ushort)-distance);
        return distance < maximumDistance;
    }

    /// <summary>
    /// Ports $A0:AD70. Its bottom edge is 256 pixels, deliberately not the 224-line visible
    /// viewport, because the original sound admission test uses a square screen window.
    /// </summary>
    private static bool IsShutterCenterOnScreen(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera) =>
        unchecked((short)(slot.Position.X.Whole - camera.X)) >= 0 &&
        unchecked((short)(camera.X + 0x0100 - slot.Position.X.Whole)) >= 0 &&
        unchecked((short)(slot.Position.Y.Whole - camera.Y)) >= 0 &&
        unchecked((short)(camera.Y + 0x0100 - slot.Position.Y.Whole)) >= 0;

    /// <summary>Publishes the activation sound only when the shutter center lies in the 256-pixel camera square.</summary>
    private void QueueShutterActivationSoundIfOnScreen(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera)
    {
        if (IsShutterCenterOnScreen(slot, camera))
            LastShutterSoundEffect = ShutterActivationSound;
    }

    /// <summary>Finds a growing shutter's room state by its physical enemy slot.</summary>
    private GrowingShutterEnemyState RequireGrowingShutterState(RoomEnemySlot slot) =>
        _growingShutterStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized growing-shutter state.");

    /// <summary>Finds a vertical shutter's room state by its physical enemy slot.</summary>
    private VerticalShutterEnemyState RequireVerticalShutterState(RoomEnemySlot slot) =>
        _verticalShutterStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized vertical-shutter state.");

    /// <summary>Finds a horizontal shutter's room state by its physical enemy slot.</summary>
    private HorizontalShutterEnemyState RequireHorizontalShutterState(RoomEnemySlot slot) =>
        _horizontalShutterStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized horizontal-shutter state.");

    /// <summary>Lets bombs trigger shootable vertical shutters without ordinary damage AI.</summary>
    internal void RegisterShuttersBombReactions(EnemyAiTable table)
    {
        // `$A2:F0A2` never calls common shot AI. Bomb collision has already marked the physical bomb, then the
        // callback only advances the shutter state machine exactly as a beam, missile, touch, or power bomb would.
        table.AddBombSpecial(c => c.Enemy.EnemyDefinitionPointer is EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.Kamer &&
                                  c.SelectedShotAi == EnemyInteractionCallback.ShootableVerticalShutterShot,
            c => ReactVerticalShutter(c.Enemy, _shutterCamera));
    }

    /// <summary>Runs common bomb damage before reacting a destroyable vertical shutter.</summary>
    internal void RegisterDestroyableShutterBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.ShutterDestroyable &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.DestroyableVerticalShutterShot)
        {
            AfterCommon = c => ReactVerticalShutter(c.Enemy, _shutterCamera),
        });
    }

    /// <summary>Runs every shutter's pose and Kamer's platform loop as typed programs (#641).</summary>
    internal void RegisterShuttersTypedAnimations(EnemyAiTable table)
    {
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterGrowing, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            GrowingShutterEnemyState state = RequireGrowingShutterState(slot);
            AnimationProgramCursor<ShutterProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, ShutterAnimations.Programs, ref cursor, ShutterFrameDefinitions.Spritemap);
            state.Program = cursor;
        });
        table.AddTypedAnimation(slot => IsVerticalShutterDefinition(slot.EnemyDefinitionPointer), (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            VerticalShutterEnemyState state = RequireVerticalShutterState(slot);
            AnimationProgramCursor<ShutterProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, ShutterAnimations.Programs, ref cursor, ShutterFrameDefinitions.Spritemap);
            state.Program = cursor;
        });
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterHorizShootable, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            HorizontalShutterEnemyState state = RequireHorizontalShutterState(slot);
            AnimationProgramCursor<ShutterProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, ShutterAnimations.Programs, ref cursor, ShutterFrameDefinitions.Spritemap);
            state.Program = cursor;
        });
    }

    /// <summary>Binds growing, vertical, Kamer, and horizontal shutter room-load routines.</summary>
    internal void RegisterShuttersInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_ShutterGrowing, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterGrowing, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeGrowingShutter(slot);
        });

        EnemyInitializationAiHandler shared24 = (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeVerticalShutter(slot);
        };

        table.RegisterInitialization(EnemyAiRoutine.InitAI_ShutterShootable_ShutterDestroyable, slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.ShutterDestroyable, shared24);

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Kamer, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Kamer, shared24);

        table.RegisterInitialization(EnemyAiRoutine.InitAI_ShutterHorizShootable, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterHorizShootable, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeHorizontalShutter(slot, samus);
        });
    }

    /// <summary>Binds each shutter family to its native per-frame state machine.</summary>
    internal void RegisterShuttersMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_ShutterGrowing, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterGrowing, (slot, frame, nmiFrameCounter8) =>
        {
            RunGrowingShutterMain(
                slot,
                RequireGrowingShutterState(slot),
                frame.Samus,
                frame.Camera);
        });

        table.RegisterMain(EnemyAiRoutine.MainAI_ShutterShootable_ShutterDestroyable_Kamer, slot => IsVerticalShutterDefinition(slot.EnemyDefinitionPointer), (slot, frame, nmiFrameCounter8) =>
        {
            RunVerticalShutterMain(
                slot,
                RequireVerticalShutterState(slot),
                frame.Samus,
                frame.Camera);
        });

        table.RegisterMain(EnemyAiRoutine.MainAI_ShutterHorizShootable, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterHorizShootable, (slot, frame, nmiFrameCounter8) =>
        {
            RunHorizontalShutterMain(
                slot,
                RequireHorizontalShutterState(slot),
                frame.Samus,
                frame.ControllerInput);
        });
    }

    /// <summary>Uses movement reactions instead of common power-bomb damage for shutter families.</summary>
    internal void RegisterShuttersPowerBombReactions(EnemyAiTable table)
    {
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => IsVerticalShutterDefinition(enemy.EnemyDefinitionPointer) &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.VerticalShutterPowerBomb)
        {
            ReplacesCommon = (enemy, _) => ReactVerticalShutter(enemy, _shutterCamera),
        });
        table.AddPowerBomb(new EnemyPowerBombReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.ShutterHorizShootable &&
                                                               EnemyCollision.PowerBombReaction(enemy) == EnemyInteractionCallback.HorizontalShutterPowerBomb)
        {
            ReplacesCommon = (enemy, _) => ReactHorizontalShutter(enemy),
        });
    }

    /// <summary>Marks a shootable vertical shutter's projectile collision before activating it.</summary>
    internal void RegisterShuttersShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer is EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.Kamer &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.ShootableVerticalShutterShot)
        {
            // $F0A2 goes directly to the shutter reaction instead of normal shot AI. The bank-$A0 collision walker
            // has nevertheless already marked the colliding projectile, including Super Missile quake side effects.
            AfterSuperMissileQuake = (in EnemyShotContext c) =>
            {
                c.Projectiles.EnemyImpacts.ApplyEnemyCollisionPrelude(c.Projectile.SlotIndex, c.MarksCollision);
                ReactVerticalShutter(c.Enemy, _shutterCamera);
                return true;
            },
        });
    }

    /// <summary>Runs common shot damage for destroyable shutters and movement reactions for horizontal ones.</summary>
    internal void RegisterDestroyableAndHorizontalShutterShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.ShutterDestroyable &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.DestroyableVerticalShutterShot)
        {
            AfterCommon = (c, _) => ReactVerticalShutter(c.Enemy, _shutterCamera),
        });

        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.ShutterHorizShootable &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.HorizontalShutterShot)
        {
            ExemptFromGenericDeath = true,
            AfterCommon = (c, _) => ReactHorizontalShutter(c.Enemy),
        });
    }

    /// <summary>Uses Samus contact to react vertical shutters or manually eject her from stopped horizontal ones.</summary>
    internal void RegisterShuttersTouch(EnemyAiTable table)
    {
        // $F09D does not enter common contact damage. Body overlap is itself the trigger, and $F0B6 decides
        // whether this population mode may start moving.
        table.AddTouch(slot => IsVerticalShutterDefinition(slot.EnemyDefinitionPointer) &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.VerticalShutterTouch,
            context => ReactVerticalShutter(context.Slot, _shutterCamera));

        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterHorizShootable &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.HorizontalShutterTouch,
            context =>
            {
                // $F3D8 provides manual ejection only after a permanently stopped shutter.
                // The main AI's subsequent nonzero contact-damage-index check reuses the
                // shot reaction, allowing Screw/Speed contact to trigger modes three/four.
                TouchHorizontalShutter(context.Slot, context.Samus, context.ControllerInput);
                if (context.ContactDamageIndex != SamusContactDamageIndex.None)
                    ReactHorizontalShutter(context.Slot);
            });
    }
    /// <summary>Ports $A2:EE05/$A2:EE12 and their shared tail at $A2:EE1F.</summary>
    internal void InitializeVerticalShutter(RoomEnemySlot slot)
    {
        if (!IsVerticalShutterDefinition(slot.EnemyDefinitionPointer))
        {
            throw new InvalidDataException(
                $"Definition ${(int)slot.EnemyDefinitionPointer:X4} does not use vertical-shutter initialization.");
        }

        var state = new VerticalShutterEnemyState(slot);
        _verticalShutterStates[slot.SlotIndex] = state;

        ushort packedSpeedAndDirection = slot.CurrentInstruction;
        state.SpeedTableIndex = unchecked((byte)packedSpeedAndDirection);
        state.PrimaryDirection = unchecked((byte)(SnesEightEight.WholeOf(packedSpeedAndDirection)));
        state.ReactionDirection = state.PrimaryDirection;

        int speedRecordOffset = state.SpeedTableIndex * 8;
        (state.DownVelocity, state.DownSubvelocity) = EnemyMath.ReadLinearEnemySpeed(
            unchecked((ushort)speedRecordOffset));
        (state.UpVelocity, state.UpSubvelocity) = EnemyMath.ReadLinearEnemySpeed(
            unchecked((ushort)(speedRecordOffset + 4)));

        state.MovedUpRestParameter = unchecked((byte)slot.ExtraProperties);
        state.MovedDownRestParameter = SnesLittleEndian.HighByte(slot.ExtraProperties);
        state.MovedUpRestTime = unchecked((ushort)(state.MovedUpRestParameter << 4));
        state.MovedDownRestTime = unchecked((ushort)(state.MovedDownRestParameter << 4));

        state.TriggerMode = unchecked((byte)slot.Parameter1);
        state.InitialFunctionTableOffset = unchecked((ushort)(state.TriggerMode * 2));
        if (state.InitialFunctionTableOffset > 8)
        {
            throw new InvalidDataException(
                $"Vertical shutter trigger mode {state.TriggerMode} exceeds its five-entry ROM table.");
        }
        state.TravelDistance = SnesLittleEndian.HighByte(slot.Parameter1);
        state.HorizontalProximityOrWaitTime = slot.Parameter2;
        state.FunctionTimer = NativeCountdown.FromRaw(slot.Parameter2);

        state.MinimumYPosition = slot.Position.Y.Whole;
        state.MaximumYPosition = unchecked((ushort)(slot.Position.Y.Whole + state.TravelDistance));
        if (state.PrimaryDirection == 0)
        {
            state.MaximumYPosition = slot.Position.Y.Whole;
            state.MinimumYPosition = unchecked((ushort)(slot.Position.Y.Whole - state.TravelDistance));
        }

        state.Function = VerticalShutterFunction.Initial;
        state.MovingSamus = false;
        state.ShotActivated = false;
        slot.ExtraProperties = 0;
        InstallVerticalShutterProgram(
            slot,
            state,
            slot.EnemyDefinitionPointer == EnemyDefinitionId.Kamer ? ShutterProgram.KamerPlatform : ShutterProgram.FortyPixels);
    }

    /// <summary>Ports <c>VerticalShutter_Main</c> at $A2:EED1.</summary>
    internal void RunVerticalShutterMain(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        // The initial function JSRs through its parameter table during this very AI
        // call; it does not spend a frame installing the selected wait function.
        // Keep Initial live until activation writes the moving function, as native does.
        switch (state.Function == VerticalShutterFunction.Initial
            ? SelectInitialVerticalShutterFunction(state) : state.Function)
        {
            case VerticalShutterFunction.WaitForTimer:
                state.FunctionTimer = state.FunctionTimer.Decrement().Remaining;
                if (state.FunctionTimer.IsZero)
                {
                    state.FunctionTimer = NativeCountdown.FromRaw(state.HorizontalProximityOrWaitTime);
                    ActivateVerticalShutter(slot, state, camera);
                }
                break;

            case VerticalShutterFunction.WaitForHorizontalProximity:
                if (samus is null)
                    throw new InvalidOperationException("Vertical-shutter proximity AI requires Samus state.");
                if (IsSamusWithinShutterHorizontalDistance(
                    slot,
                    samus,
                    state.HorizontalProximityOrWaitTime))
                {
                    ActivateVerticalShutter(slot, state, camera);
                }
                break;

            case VerticalShutterFunction.Activate:
                ActivateVerticalShutter(slot, state, camera);
                break;

            case VerticalShutterFunction.InitialNoOp:
            case VerticalShutterFunction.PermanentNoOp:
                break;

            case VerticalShutterFunction.MovingUp:
                if (samus is null)
                    throw new InvalidOperationException("Moving vertical shutter requires Samus state.");
                MoveVerticalShutterUp(slot, state, samus);
                break;

            case VerticalShutterFunction.MovingDown:
                if (samus is null)
                    throw new InvalidOperationException("Moving vertical shutter requires Samus state.");
                MoveVerticalShutterDown(slot, state, samus);
                break;

            case VerticalShutterFunction.StoppedAfterMovingUp:
                RunVerticalShutterStoppedAfterUp(slot, state, camera);
                break;

            case VerticalShutterFunction.StoppedAfterMovingDown:
                RunVerticalShutterStoppedAfterDown(slot, state, camera);
                break;

            default:
                throw new InvalidDataException(
                    $"Vertical shutter function $A2:{(ushort)state.Function:X4} is not translated.");
        }

        // $A2:EED7-$A2:EF04: an idle shutter reacts when Samus's four solid-enemy collision
        // words AND to this enemy's index and she has contact damage (e.g. screw attack).
        if (state.Function is not (VerticalShutterFunction.MovingUp or VerticalShutterFunction.MovingDown) &&
            SamusContactsIdleShutter(slot, samus ?? throw new InvalidOperationException(
                "Vertical-shutter AI requires Samus state for its contact reaction.")))
        {
            ReactVerticalShutter(slot, camera);
        }
    }

    /// <summary>Decodes the vertical population mode into timer, proximity, activation, or idle state.</summary>
    internal static VerticalShutterFunction SelectInitialVerticalShutterFunction(VerticalShutterEnemyState state)
    {
        return state.InitialFunctionTableOffset switch
        {
            0 => VerticalShutterFunction.WaitForTimer,
            2 => VerticalShutterFunction.WaitForHorizontalProximity,
            4 => VerticalShutterFunction.Activate,
            6 or 8 => VerticalShutterFunction.InitialNoOp,
            _ => throw new InvalidDataException(
                $"Vertical shutter initial offset ${state.InitialFunctionTableOffset:X4} is invalid."),
        };
    }

    /// <summary>Starts movement in the primary vertical direction and queues on-screen audio.</summary>
    private void ActivateVerticalShutter(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        Position16<RoomPixels> camera)
    {
        state.Function = state.PrimaryDirection == 0
            ? VerticalShutterFunction.MovingUp
            : VerticalShutterFunction.MovingDown;
        QueueShutterActivationSoundIfOnScreen(slot, camera);
    }

    /// <summary>Moves toward minimum Y, carrying Samus when riding the shutter.</summary>
    private static void MoveVerticalShutterUp(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState samus)
    {
        state.PreviousYPosition = slot.Position.Y.Whole;
        state.MovingSamus = PlatformsBehavior.IsSamusRidingPlatform(slot, samus);
        slot.Position = slot.Position.WithY(AddShutterVelocity(
            slot.Position.Y,
            state.UpVelocity,
            state.UpSubvelocity));
        CarrySamusVertically(slot, state, samus);

        if (unchecked((short)(state.MinimumYPosition - slot.Position.Y.Whole)) < 0)
            return;
        StopVerticalShutterAfterUp(state);
    }

    /// <summary>Moves toward maximum Y, carrying Samus when riding the shutter.</summary>
    private static void MoveVerticalShutterDown(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState samus)
    {
        state.PreviousYPosition = slot.Position.Y.Whole;
        state.MovingSamus = PlatformsBehavior.IsSamusRidingPlatform(slot, samus);
        slot.Position = slot.Position.WithY(AddShutterVelocity(
            slot.Position.Y,
            state.DownVelocity,
            state.DownSubvelocity));
        CarrySamusVertically(slot, state, samus);

        if (unchecked((short)(state.MaximumYPosition - slot.Position.Y.Whole)) > 0)
            return;
        StopVerticalShutterAfterDown(state);
    }

    /// <summary>Publishes the shutter's whole-pixel Y delta to Samus's extra displacement.</summary>
    private static void CarrySamusVertically(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState samus)
    {
        if (!state.MovingSamus)
            return;
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(unchecked((short)(
            slot.Position.Y.Whole - state.PreviousYPosition)));
    }

    /// <summary>Enters a timed rest or permanent stop at the upper endpoint.</summary>
    private static void StopVerticalShutterAfterUp(VerticalShutterEnemyState state)
    {
        if (state.MovedUpRestTime == PermanentStopRestTime)
        {
            state.Function = VerticalShutterFunction.PermanentNoOp;
            return;
        }
        state.FunctionTimer = NativeCountdown.FromRaw(state.MovedUpRestTime);
        state.Function = VerticalShutterFunction.StoppedAfterMovingUp;
    }

    /// <summary>Enters a timed rest or permanent stop at the lower endpoint.</summary>
    private static void StopVerticalShutterAfterDown(VerticalShutterEnemyState state)
    {
        if (state.MovedDownRestTime == PermanentStopRestTime)
        {
            state.Function = VerticalShutterFunction.PermanentNoOp;
            return;
        }
        state.FunctionTimer = NativeCountdown.FromRaw(state.MovedDownRestTime);
        state.Function = VerticalShutterFunction.StoppedAfterMovingDown;
    }

    /// <summary>Counts down upper-end rest before descending or waiting for renewed proximity.</summary>
    private void RunVerticalShutterStoppedAfterUp(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        Position16<RoomPixels> camera)
    {
        NativeWordCounterStep timer = state.FunctionTimer.Decrement();
        state.FunctionTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return;

        state.Function = state.TriggerMode == 1 && state.PrimaryDirection != 0
            ? VerticalShutterFunction.WaitForHorizontalProximity
            : VerticalShutterFunction.MovingDown;
        QueueShutterActivationSoundIfOnScreen(slot, camera);
    }

    /// <summary>Counts down lower-end rest before ascending or waiting for renewed proximity.</summary>
    private void RunVerticalShutterStoppedAfterDown(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        Position16<RoomPixels> camera)
    {
        NativeWordCounterStep timer = state.FunctionTimer.Decrement();
        state.FunctionTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return;

        state.Function = state.TriggerMode == 1 && state.PrimaryDirection == 0
            ? VerticalShutterFunction.WaitForHorizontalProximity
            : VerticalShutterFunction.MovingUp;
        QueueShutterActivationSoundIfOnScreen(slot, camera);
    }

    /// <summary>
    /// Ports $A2:F0B6. Modes zero through two merely make the acknowledgement sound;
    /// mode three reacts once, while mode four may be reactivated after every stop.
    /// </summary>
    private void ReactVerticalShutter(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera)
    {
        VerticalShutterEnemyState state = RequireVerticalShutterState(slot);
        if (state.InitialFunctionTableOffset < 6)
        {
            QueueShutterActivationSoundIfOnScreen(slot, camera);
            return;
        }

        if (state.InitialFunctionTableOffset == 6)
        {
            if (state.ShotActivated)
                return;
            state.ShotActivated = true;
        }

        if (state.Function is VerticalShutterFunction.MovingUp or VerticalShutterFunction.MovingDown)
            return;

        state.Function = state.ReactionDirection == 0
            ? VerticalShutterFunction.MovingUp
            : VerticalShutterFunction.MovingDown;
        state.ReactionDirection ^= 1;
        QueueShutterActivationSoundIfOnScreen(slot, camera);
    }

    /// <summary>Restarts a vertical shutter's selected animation.</summary>
    private static void InstallVerticalShutterProgram(RoomEnemySlot slot, VerticalShutterEnemyState state, ShutterProgram program)
    {
        state.Program = AnimationProgramCursor<ShutterProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetShutterRoomState(request.Camera);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        BeginShutterFrame(frame.Camera);
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastShutterSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
