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
                ? VerticalShutterInstructionProgramDefinitions.KamerPlatform
                : VerticalShutterInstructionProgramDefinitions.Plain);
    }

    /// <summary>Ports <c>VerticalShutter_Main</c> at $A2:EED1.</summary>
    private void RunVerticalShutterMain(
        RoomEnemySlot slot,
        VerticalShutterEnemyState state,
        SamusState? samus,
        ushort cameraX,
        ushort cameraY)
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
                    ActivateVerticalShutter(slot, state, cameraX, cameraY);
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
                    ActivateVerticalShutter(slot, state, cameraX, cameraY);
                }
                break;

            case VerticalShutterFunction.Activate:
                ActivateVerticalShutter(slot, state, cameraX, cameraY);
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
                RunVerticalShutterStoppedAfterUp(slot, state, cameraX, cameraY);
                break;

            case VerticalShutterFunction.StoppedAfterMovingDown:
                RunVerticalShutterStoppedAfterDown(slot, state, cameraX, cameraY);
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
            ReactVerticalShutter(slot, cameraX, cameraY);
        }
    }

    /// <summary>Maps the validated trigger-table offset to the shutter's first wait, activation, or no-op state.</summary>
    /// <param name="state">Initialized shutter state containing the selected trigger-table offset.</param>
    /// <returns>The initial function dispatched by the vertical-shutter update.</returns>
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

    /// <summary>Starts travel in the configured primary direction and queues the activation sound when visible.</summary>
    /// <param name="slot">Shutter slot whose movement state is activated.</param>
    /// <param name="state">State supplying the primary travel direction.</param>
    /// <param name="cameraX">Current camera horizontal position used for sound visibility.</param>
    /// <param name="cameraY">Current camera vertical position used for sound visibility.</param>
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

    /// <summary>Moves the shutter upward by its configured fixed-point velocity and stops at its upper bound.</summary>
    /// <param name="slot">Shutter slot whose position is updated.</param>
    /// <param name="state">Movement state supplying velocity, limit, and rider status.</param>
    /// <param name="samus">Samus state that is carried by the shutter when she is riding it.</param>
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

    /// <summary>Moves the shutter downward by its configured fixed-point velocity and stops at its lower bound.</summary>
    /// <param name="slot">Shutter slot whose position is updated.</param>
    /// <param name="state">Movement state supplying velocity, limit, and rider status.</param>
    /// <param name="samus">Samus state that is carried by the shutter when she is riding it.</param>
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

    /// <summary>Applies the shutter's vertical displacement to Samus when the update began with her riding the platform.</summary>
    /// <param name="slot">Shutter slot at its new vertical position.</param>
    /// <param name="state">State containing the previous position and riding flag for this movement step.</param>
    /// <param name="samus">Samus state whose extra vertical displacement is updated.</param>
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

    /// <summary>Enters permanent no-op or the configured rest period after upward travel reaches its limit.</summary>
    /// <param name="state">Shutter state whose post-upward function and timer are selected.</param>
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

    /// <summary>Enters permanent no-op or the configured rest period after downward travel reaches its limit.</summary>
    /// <param name="state">Shutter state whose post-downward function and timer are selected.</param>
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

    /// <summary>Counts down the post-upward rest and then waits for proximity or begins downward travel.</summary>
    /// <param name="slot">Shutter slot used for the activation sound visibility check.</param>
    /// <param name="state">State supplying the rest timer, trigger mode, and travel direction.</param>
    /// <param name="cameraX">Current camera horizontal position.</param>
    /// <param name="cameraY">Current camera vertical position.</param>
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

    /// <summary>Counts down the post-downward rest and then waits for proximity or begins upward travel.</summary>
    /// <param name="slot">Shutter slot used for the activation sound visibility check.</param>
    /// <param name="state">State supplying the rest timer, trigger mode, and travel direction.</param>
    /// <param name="cameraX">Current camera horizontal position.</param>
    /// <param name="cameraY">Current camera vertical position.</param>
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

    /// <summary>Installs an instruction-list entry and resets the per-list timer state on the shutter slot.</summary>
    /// <param name="slot">Shutter slot receiving the instruction pointer.</param>
    /// <param name="instruction">Banked instruction address to execute next.</param>
    private static void InstallVerticalShutterInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }
}
