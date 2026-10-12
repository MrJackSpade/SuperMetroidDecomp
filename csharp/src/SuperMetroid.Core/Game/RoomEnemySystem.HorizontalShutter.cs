using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Full translation of horizontal shutter $D57F. Nintendo left this definition unused by
/// named retail room populations, but its header and complete AI remain executable ROM data;
/// implementing it prevents the family audit from quietly equating “unused” with “missing.”
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const short HorizontalShutterManualPushPixels = 4;

    /// <summary>Ports <c>HorizontalShutter_Init</c> at $A2:F111/$A2:F11E.</summary>
    private void InitializeHorizontalShutter(RoomEnemySlot slot, SamusState? samus)
    {
        var state = new HorizontalShutterEnemyState(slot);
        _horizontalShutterStates[slot.SlotIndex] = state;

        ushort packedSpeedAndDirection = slot.CurrentInstruction;
        state.SpeedTableIndex = unchecked((byte)packedSpeedAndDirection);
        state.PrimaryDirection = unchecked((byte)(SnesEightEight.WholeOf(packedSpeedAndDirection)));
        state.ReactionDirection = unchecked((ushort)(state.PrimaryDirection ^ 1));

        int speedRecordOffset = state.SpeedTableIndex * 8;
        (state.RightVelocity, state.RightSubvelocity) = ReadLinearEnemySpeed(
            unchecked((ushort)speedRecordOffset));
        (state.LeftVelocity, state.LeftSubvelocity) = ReadLinearEnemySpeed(
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
        state.FunctionTimer = slot.Parameter2;

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
        InstallHorizontalShutterInstruction(slot);
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
                state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
                if (state.FunctionTimer == 0)
                {
                    state.FunctionTimer = state.HorizontalProximityOrWaitTime;
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

    private static void SelectInitialHorizontalShutterFunction(HorizontalShutterEnemyState state)
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

    private static void ActivateHorizontalShutter(HorizontalShutterEnemyState state) =>
        state.Function = state.PrimaryDirection == 0
            ? HorizontalShutterFunction.MovingLeft
            : HorizontalShutterFunction.MovingRight;

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
        state.FunctionTimer = state.MovedLeftRestTime;
        state.Function = HorizontalShutterFunction.StoppedAfterMovingLeft;
    }

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
        state.FunctionTimer = state.MovedRightRestTime;
        state.Function = HorizontalShutterFunction.StoppedAfterMovingRight;
    }

    private static void RunHorizontalShutterStoppedAfterLeft(HorizontalShutterEnemyState state)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
        state.FunctionTimer = timer.Value;
        if (timer.IsNonNegative)
            return;
        state.Function = state.TriggerMode == 1 && state.PrimaryDirection != 0
            ? HorizontalShutterFunction.WaitForHorizontalProximity
            : HorizontalShutterFunction.MovingRight;
    }

    private static void RunHorizontalShutterStoppedAfterRight(HorizontalShutterEnemyState state)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
        state.FunctionTimer = timer.Value;
        if (timer.IsNonNegative)
            return;
        state.Function = state.TriggerMode == 1 && state.PrimaryDirection == 0
            ? HorizontalShutterFunction.WaitForHorizontalProximity
            : HorizontalShutterFunction.MovingLeft;
    }

    private static bool HorizontalShutterOverlapsSamus(RoomEnemySlot slot, SamusState samus) =>
        RadiusBoxesOverlap(
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

    private static void InstallHorizontalShutterInstruction(RoomEnemySlot slot)
    {
        slot.CurrentInstruction = HorizontalShutterInstructionProgramDefinitions.Stationary;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }
}
