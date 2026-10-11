namespace SuperMetroid.Core.Game;

/// <summary>
/// Translation of the shared vertical-shutter engine used by $D53F, $D5BF, and $D5FF.
/// Their movement is identical; their definition headers select different graphics and
/// shot reactions, which remain separated in the combat dispatcher.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Ports $A2:EE05/$A2:EE12 and their shared tail at $A2:EE1F.</summary>
    private void InitializeVerticalShutter(RoomEnemySlot slot)
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
        InstallVerticalShutterInstruction(
            slot,
            slot.EnemyDefinitionPointer == EnemyDefinitionId.Kamer
                ? VerticalShutterInstructionProgramDefinitions.KamerPlatform
                : VerticalShutterInstructionProgramDefinitions.Plain);
    }

    /// <summary>Ports <c>VerticalShutter_Main</c> at $A2:EED1.</summary>
    private void RunVerticalShutterMain(
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
                state.FunctionTimer = unchecked((ushort)(state.FunctionTimer - 1));
                if (state.FunctionTimer == 0)
                {
                    state.FunctionTimer = state.HorizontalProximityOrWaitTime;
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

    private static VerticalShutterFunction SelectInitialVerticalShutterFunction(VerticalShutterEnemyState state)
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

    private static void MoveVerticalShutterUp(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState samus)
    {
        state.PreviousYPosition = slot.Position.Y.Whole;
        state.MovingSamus = IsSamusRidingPlatform(slot, samus);
        slot.Position = slot.Position.WithY(AddShutterVelocity(
            slot.Position.Y,
            state.UpVelocity,
            state.UpSubvelocity));
        CarrySamusVertically(slot, state, samus);

        if (unchecked((short)(state.MinimumYPosition - slot.Position.Y.Whole)) < 0)
            return;
        StopVerticalShutterAfterUp(state);
    }

    private static void MoveVerticalShutterDown(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState samus)
    {
        state.PreviousYPosition = slot.Position.Y.Whole;
        state.MovingSamus = IsSamusRidingPlatform(slot, samus);
        slot.Position = slot.Position.WithY(AddShutterVelocity(
            slot.Position.Y,
            state.DownVelocity,
            state.DownSubvelocity));
        CarrySamusVertically(slot, state, samus);

        if (unchecked((short)(state.MaximumYPosition - slot.Position.Y.Whole)) > 0)
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
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithWholeY(unchecked((short)(
            slot.Position.Y.Whole - state.PreviousYPosition)));
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
        Position16<RoomPixels> camera)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
        state.FunctionTimer = timer.Value;
        if (timer.IsNonNegative)
            return;

        state.Function = state.TriggerMode == 1 && state.PrimaryDirection != 0
            ? VerticalShutterFunction.WaitForHorizontalProximity
            : VerticalShutterFunction.MovingDown;
        QueueShutterActivationSoundIfOnScreen(slot, camera);
    }

    private void RunVerticalShutterStoppedAfterDown(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        Position16<RoomPixels> camera)
    {
        NativeWordCounterStep timer = NativeWordCounter.Decrement(state.FunctionTimer);
        state.FunctionTimer = timer.Value;
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

    private static void InstallVerticalShutterInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }
}
