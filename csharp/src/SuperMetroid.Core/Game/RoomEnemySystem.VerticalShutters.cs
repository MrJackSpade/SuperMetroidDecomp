namespace SuperMetroid.Core.Game;

/// <summary>
/// Translation of the shared vertical-shutter engine used by $D53F, $D5BF, and $D5FF.
/// Their movement is identical; their definition headers select different graphics and
/// shot reactions, which remain separated in the combat dispatcher.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort PlainVerticalShutterInstruction = 0xe9aa;
    private const ushort KamerVerticalPlatformInstruction = 0xede7;

    /// <summary>Ports $A2:EE05/$A2:EE12 and their shared tail at $A2:EE1F.</summary>
    private void InitializeVerticalShutter(RoomEnemySlot slot)
    {
        if (!IsVerticalShutterDefinition(slot.EnemyDefinitionPointer))
        {
            throw new InvalidDataException(
                $"Definition ${slot.EnemyDefinitionPointer:X4} does not use vertical-shutter initialization.");
        }

        var state = new VerticalShutterEnemyState(slot);
        _verticalShutterStates[slot.SlotIndex] = state;

        ushort packedSpeedAndDirection = slot.CurrentInstruction;
        state.SpeedTableIndex = unchecked((byte)packedSpeedAndDirection);
        state.PrimaryDirection = unchecked((byte)(packedSpeedAndDirection >> 8));
        state.ReactionDirection = state.PrimaryDirection;

        int speedRecordOffset = state.SpeedTableIndex * 8;
        (state.DownVelocity, state.DownSubvelocity) = ReadLinearEnemySpeed(
            unchecked((ushort)speedRecordOffset));
        (state.UpVelocity, state.UpSubvelocity) = ReadLinearEnemySpeed(
            unchecked((ushort)(speedRecordOffset + 4)));

        state.MovedUpRestParameter = unchecked((byte)slot.ExtraProperties);
        state.MovedDownRestParameter = unchecked((byte)(slot.ExtraProperties >> 8));
        state.MovedUpRestTime = unchecked((ushort)(state.MovedUpRestParameter << 4));
        state.MovedDownRestTime = unchecked((ushort)(state.MovedDownRestParameter << 4));

        state.TriggerMode = unchecked((byte)slot.Parameter1);
        state.InitialFunctionTableOffset = unchecked((ushort)(state.TriggerMode * 2));
        if (state.InitialFunctionTableOffset > 8)
        {
            throw new InvalidDataException(
                $"Vertical shutter trigger mode {state.TriggerMode} exceeds its five-entry ROM table.");
        }
        state.TravelDistance = unchecked((byte)(slot.Parameter1 >> 8));
        state.HorizontalProximityOrWaitTime = slot.Parameter2;
        state.FunctionTimer = slot.Parameter2;

        state.MinimumYPosition = slot.YPosition;
        state.MaximumYPosition = unchecked((ushort)(slot.YPosition + state.TravelDistance));
        if (state.PrimaryDirection == 0)
        {
            state.MaximumYPosition = slot.YPosition;
            state.MinimumYPosition = unchecked((ushort)(slot.YPosition - state.TravelDistance));
        }

        state.Function = VerticalShutterFunction.Initial;
        state.MovingSamus = false;
        state.ShotActivated = false;
        slot.ExtraProperties = 0;
        InstallVerticalShutterInstruction(
            slot,
            slot.EnemyDefinitionPointer == KamerVerticalPlatformDefinition
                ? KamerVerticalPlatformInstruction
                : PlainVerticalShutterInstruction);
    }

    /// <summary>Ports <c>VerticalShutter_Main</c> at $A2:EED1.</summary>
    private void RunVerticalShutterMain(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState? samus,
        ushort cameraX,
        ushort cameraY)
    {
        switch (state.Function)
        {
            case VerticalShutterFunction.Initial:
                SelectInitialVerticalShutterFunction(state);
                return;

            case VerticalShutterFunction.WaitForTimer:
                state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
                if (state.FunctionTimer == 0)
                {
                    state.FunctionTimer = state.HorizontalProximityOrWaitTime;
                    ActivateVerticalShutter(slot, state, cameraX, cameraY);
                }
                return;

            case VerticalShutterFunction.WaitForHorizontalProximity:
                if (samus is null)
                    throw new InvalidOperationException("Vertical-shutter proximity AI requires Samus state.");
                if (IsSamusWithinShutterHorizontalDistance(
                    slot,
                    samus,
                    state.HorizontalProximityOrWaitTime))
                {
                    ActivateVerticalShutter(slot, state, cameraX, cameraY);
                }
                return;

            case VerticalShutterFunction.Activate:
                ActivateVerticalShutter(slot, state, cameraX, cameraY);
                return;

            case VerticalShutterFunction.InitialNoOp:
            case VerticalShutterFunction.PermanentNoOp:
                return;

            case VerticalShutterFunction.MovingUp:
                if (samus is null)
                    throw new InvalidOperationException("Moving vertical shutter requires Samus state.");
                MoveVerticalShutterUp(slot, state, samus);
                return;

            case VerticalShutterFunction.MovingDown:
                if (samus is null)
                    throw new InvalidOperationException("Moving vertical shutter requires Samus state.");
                MoveVerticalShutterDown(slot, state, samus);
                return;

            case VerticalShutterFunction.StoppedAfterMovingUp:
                RunVerticalShutterStoppedAfterUp(slot, state, cameraX, cameraY);
                return;

            case VerticalShutterFunction.StoppedAfterMovingDown:
                RunVerticalShutterStoppedAfterDown(slot, state, cameraX, cameraY);
                return;

            default:
                throw new InvalidDataException(
                    $"Vertical shutter function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private static void SelectInitialVerticalShutterFunction(VerticalShutterEnemyState state)
    {
        state.Function = state.InitialFunctionTableOffset switch
        {
            0 => VerticalShutterFunction.WaitForTimer,
            2 => VerticalShutterFunction.WaitForHorizontalProximity,
            4 => VerticalShutterFunction.Activate,
            6 or 8 => VerticalShutterFunction.InitialNoOp,
            _ => throw new InvalidDataException(
                $"Vertical shutter initial offset ${state.InitialFunctionTableOffset:X4} is invalid."),
        };
    }

    private void ActivateVerticalShutter(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        ushort cameraX,
        ushort cameraY)
    {
        state.Function = state.PrimaryDirection == 0
            ? VerticalShutterFunction.MovingUp
            : VerticalShutterFunction.MovingDown;
        QueueShutterActivationSoundIfOnScreen(slot, cameraX, cameraY);
    }

    private static void MoveVerticalShutterUp(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState samus)
    {
        state.PreviousYPosition = slot.YPosition;
        state.MovingSamus = IsSamusRidingPlatform(slot, samus);
        (slot.YPosition, slot.YSubposition) = AddShutterVelocity(
            slot.YPosition,
            slot.YSubposition,
            state.UpVelocity,
            state.UpSubvelocity);
        CarrySamusVertically(slot, state, samus);

        if (unchecked((short)(state.MinimumYPosition - slot.YPosition)) < 0)
            return;
        StopVerticalShutterAfterUp(state);
    }

    private static void MoveVerticalShutterDown(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState samus)
    {
        state.PreviousYPosition = slot.YPosition;
        state.MovingSamus = IsSamusRidingPlatform(slot, samus);
        (slot.YPosition, slot.YSubposition) = AddShutterVelocity(
            slot.YPosition,
            slot.YSubposition,
            state.DownVelocity,
            state.DownSubvelocity);
        CarrySamusVertically(slot, state, samus);

        if (unchecked((short)(state.MaximumYPosition - slot.YPosition)) > 0)
            return;
        StopVerticalShutterAfterDown(state);
    }

    private static void CarrySamusVertically(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState samus)
    {
        if (!state.MovingSamus)
            return;
        samus.Kinematics.ExtraYDisplacement = unchecked((ushort)(
            slot.YPosition - state.PreviousYPosition));
    }

    private static void StopVerticalShutterAfterUp(VerticalShutterEnemyState state)
    {
        if (state.MovedUpRestTime == PermanentStopRestTime)
        {
            state.Function = VerticalShutterFunction.PermanentNoOp;
            return;
        }
        state.FunctionTimer = state.MovedUpRestTime;
        state.Function = VerticalShutterFunction.StoppedAfterMovingUp;
    }

    private static void StopVerticalShutterAfterDown(VerticalShutterEnemyState state)
    {
        if (state.MovedDownRestTime == PermanentStopRestTime)
        {
            state.Function = VerticalShutterFunction.PermanentNoOp;
            return;
        }
        state.FunctionTimer = state.MovedDownRestTime;
        state.Function = VerticalShutterFunction.StoppedAfterMovingDown;
    }

    private void RunVerticalShutterStoppedAfterUp(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        ushort cameraX,
        ushort cameraY)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
        state.FunctionTimer = timer.Value;
        if (timer.IsNonNegative)
            return;

        state.Function = state.TriggerMode == 1 && state.PrimaryDirection != 0
            ? VerticalShutterFunction.WaitForHorizontalProximity
            : VerticalShutterFunction.MovingDown;
        QueueShutterActivationSoundIfOnScreen(slot, cameraX, cameraY);
    }

    private void RunVerticalShutterStoppedAfterDown(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        ushort cameraX,
        ushort cameraY)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
        state.FunctionTimer = timer.Value;
        if (timer.IsNonNegative)
            return;

        state.Function = state.TriggerMode == 1 && state.PrimaryDirection == 0
            ? VerticalShutterFunction.WaitForHorizontalProximity
            : VerticalShutterFunction.MovingUp;
        QueueShutterActivationSoundIfOnScreen(slot, cameraX, cameraY);
    }

    /// <summary>
    /// Ports $A2:F0B6. Modes zero through two merely make the acknowledgement sound;
    /// mode three reacts once, while mode four may be reactivated after every stop.
    /// </summary>
    private void ReactVerticalShutter(
        RoomEnemySlot slot,
        ushort cameraX,
        ushort cameraY)
    {
        VerticalShutterEnemyState state = RequireVerticalShutterState(slot);
        if (state.InitialFunctionTableOffset < 6)
        {
            QueueShutterActivationSoundIfOnScreen(slot, cameraX, cameraY);
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
        QueueShutterActivationSoundIfOnScreen(slot, cameraX, cameraY);
    }

    private static void InstallVerticalShutterInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }
}
