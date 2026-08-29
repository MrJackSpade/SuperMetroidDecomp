namespace SuperMetroid.Core.Game;

/// <summary>Literal translation of the four-section growing shutter enemy $D4FF.</summary>
public sealed partial class RoomEnemySystem
{
    private const ushort GrowingShutterTenPixelInstruction = 0xe998;
    private const ushort GrowingShutterTwentyPixelInstruction = 0xe99e;
    private const ushort GrowingShutterThirtyPixelInstruction = 0xe9a4;
    private const ushort GrowingShutterFortyPixelInstruction = 0xe9aa;
    private const ushort GrowingShutterSectionLength = 0x0010;
    private const ushort GrowingShutterIntermediateInset = 0x0007;
    private const int GrowingShutterInitialFunctionTable = 0xa2ea4e;
    private const int GrowingShutterSpeedTable = 0xa2ea56;

    /// <summary>Ports <c>GrowingShutter_Init</c> at $A2:E9DA.</summary>
    private void InitializeGrowingShutter(RoomEnemySlot slot)
    {
        var state = new GrowingShutterEnemyState(slot);
        _growingShutterStates[slot.SlotIndex] = state;

        // The initializer consumes init0 and the low extra-property word as a two-bit table
        // selector before clearing the latter. Read Nintendo's actual function-pointer table:
        // its ordering is deliberately independent of the origin direction encoded below.
        int initialFunctionIndex = slot.ExtraProperties * 2 + slot.CurrentInstruction;
        if ((uint)initialFunctionIndex >= 4)
        {
            throw new InvalidDataException(
                $"Growing shutter initial selector {initialFunctionIndex} exceeds its four-entry ROM table.");
        }
        state.Function = (GrowingShutterFunction)ReadWord(
            _bus!,
            GrowingShutterInitialFunctionTable + initialFunctionIndex * 2);
        if (state.Function is not GrowingShutterFunction.WaitToGrowUpForTimer and
            not GrowingShutterFunction.WaitToGrowUpForProximity and
            not GrowingShutterFunction.WaitToGrowDownForProximity and
            not GrowingShutterFunction.WaitToGrowDownForTimer)
        {
            throw new InvalidDataException(
                $"Growing shutter selector {initialFunctionIndex} resolved to invalid " +
                $"bank-$A2 function ${(ushort)state.Function:X4}.");
        }

        bool growsUp = slot.ExtraProperties != 0;
        state.GrowthLevel0OriginY = slot.YPosition;
        state.GrowthLevel1OriginY = unchecked((ushort)(slot.YPosition + (growsUp ? -8 : 8)));
        state.GrowthLevel2OriginY = unchecked((ushort)(slot.YPosition + (growsUp ? -16 : 16)));
        state.GrowthLevel3OriginY = unchecked((ushort)(slot.YPosition + (growsUp ? -24 : 24)));
        state.GrowthLevel = 0;

        // Parameter 2's low byte indexes four-byte 16.16 records at $A2:EA56. Keeping this
        // as a cartridge read preserves every table entry and any revision-specific data.
        ushort speedIndex = unchecked((byte)slot.Parameter2);
        int speedAddress = GrowingShutterSpeedTable + speedIndex * 4;
        state.GrowthVelocity = unchecked((short)ReadWord(_bus!, speedAddress));
        state.GrowthSubvelocity = ReadWord(_bus!, speedAddress + 2);

        slot.ExtraProperties = 0;
        InstallGrowingShutterInstruction(slot, GrowingShutterTenPixelInstruction, yRadius: 8);
    }

    /// <summary>Ports <c>GrowingShutter_Main</c> at $A2:EAB6.</summary>
    private void RunGrowingShutterMain(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        SamusState? samus,
        ushort cameraX,
        ushort cameraY)
    {
        switch (state.Function)
        {
            case GrowingShutterFunction.WaitToGrowDownForTimer:
                if (AdvanceGrowingShutterTimer(slot))
                    ActivateGrowingShutter(slot, state, growsUp: false, cameraX, cameraY);
                return;

            case GrowingShutterFunction.WaitToGrowUpForTimer:
                if (AdvanceGrowingShutterTimer(slot))
                    ActivateGrowingShutter(slot, state, growsUp: true, cameraX, cameraY);
                return;

            case GrowingShutterFunction.WaitToGrowDownForProximity:
                if (samus is null)
                    throw new InvalidOperationException("Growing-shutter proximity AI requires Samus state.");
                if (IsSamusWithinShutterHorizontalDistance(slot, samus, slot.Parameter1))
                    ActivateGrowingShutter(slot, state, growsUp: false, cameraX, cameraY);
                return;

            case GrowingShutterFunction.WaitToGrowUpForProximity:
                if (samus is null)
                    throw new InvalidOperationException("Growing-shutter proximity AI requires Samus state.");
                if (IsSamusWithinShutterHorizontalDistance(slot, samus, slot.Parameter1))
                    ActivateGrowingShutter(slot, state, growsUp: true, cameraX, cameraY);
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
                throw new NotSupportedException(
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

    private void ActivateGrowingShutter(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        bool growsUp,
        ushort cameraX,
        ushort cameraY)
    {
        state.Function = growsUp
            ? GrowingShutterFunction.GrowUp
            : GrowingShutterFunction.GrowDown;
        QueueShutterActivationSoundIfOnScreen(slot, cameraX, cameraY);
    }

    /// <summary>Ports the four growth stages at $A2:EB11-$EC12.</summary>
    private static void GrowShutterDown(RoomEnemySlot slot, GrowingShutterEnemyState state)
    {
        if (state.GrowthLevel >= 4)
            return;

        (slot.YPosition, slot.YSubposition) = AddShutterVelocity(
            slot.YPosition,
            slot.YSubposition,
            state.GrowthVelocity,
            state.GrowthSubvelocity);
        ushort target = unchecked((ushort)(state.OriginForLevel() + GrowingShutterSectionLength));

        // BPL returns at equality. A section advances only after its center has passed the
        // full sixteen-pixel threshold, after which the first three sections are inset seven.
        if (unchecked((short)(target - slot.YPosition)) >= 0)
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

        state.PreviousYPosition = slot.YPosition;
        (slot.YPosition, slot.YSubposition) = AddShutterVelocity(
            slot.YPosition,
            slot.YSubposition,
            unchecked((short)(-state.GrowthVelocity - (state.GrowthSubvelocity == 0 ? 0 : 1))),
            unchecked((ushort)-state.GrowthSubvelocity));
        ushort target = unchecked((ushort)(state.OriginForLevel() - GrowingShutterSectionLength));
        if (unchecked((short)(target - slot.YPosition)) < 0)
        {
            CarrySamusWithGrowingShutter(slot, state, samus);
            return;
        }

        FinishGrowingShutterSection(slot, state, target, growsUp: true);
        CarrySamusWithGrowingShutter(slot, state, samus);
    }

    private static void CarrySamusWithGrowingShutter(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        SamusState samus)
    {
        if (!IsSamusRidingPlatform(slot, samus))
            return;

        short wholeDelta = unchecked((short)(slot.YPosition - state.PreviousYPosition));
        if (wholeDelta < 0)
        {
            samus.Kinematics.ExtraYDisplacement = unchecked((ushort)(
                samus.Kinematics.ExtraYDisplacement + wholeDelta));
        }
    }

    private static void FinishGrowingShutterSection(
        RoomEnemySlot slot,
        GrowingShutterEnemyState state,
        ushort target,
        bool growsUp)
    {
        bool finalSection = state.GrowthLevel == 3;
        slot.YPosition = finalSection
            ? target
            : unchecked((ushort)(target + (growsUp
                ? GrowingShutterIntermediateInset
                : -GrowingShutterIntermediateInset)));
        state.GrowthLevel = unchecked((ushort)(state.GrowthLevel + 1));

        if (finalSection)
            return;

        ushort instruction = state.GrowthLevel switch
        {
            1 => GrowingShutterTwentyPixelInstruction,
            2 => GrowingShutterThirtyPixelInstruction,
            3 => GrowingShutterFortyPixelInstruction,
            _ => throw new InvalidDataException(
                $"Growing shutter advanced to invalid level {state.GrowthLevel}."),
        };
        ushort radius = unchecked((ushort)(8 + state.GrowthLevel * 8));
        InstallGrowingShutterInstruction(slot, instruction, radius);
    }

    private static void InstallGrowingShutterInstruction(
        RoomEnemySlot slot,
        ushort instruction,
        ushort yRadius)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.YRadius = yRadius;
    }
}
