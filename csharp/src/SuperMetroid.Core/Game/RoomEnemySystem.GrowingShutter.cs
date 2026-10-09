namespace SuperMetroid.Core.Game;

/// <summary>Literal translation of the four-section growing shutter enemy $D4FF.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Vertical distance in pixels that each of the shutter's four growth sections spans.</summary>
    private const ushort GrowingShutterSectionLength = 0x0010;
    /// <summary>Vertical inset applied between the first three completed shutter sections.</summary>
    private const ushort GrowingShutterIntermediateInset = 0x0007;

    /// <summary>Ports <c>GrowingShutter_Init</c> at $A2:E9DA.</summary>
    private void InitializeGrowingShutter(RoomEnemySlot slot)
    {
        var state = new GrowingShutterEnemyState(slot);
        _growingShutterStates[slot.SlotIndex] = state;

        // The initializer consumes init0 and the low extra-property word as a two-bit table
        // selector before clearing the latter. Keep native dispatch ordering independent
        // of the origin direction encoded below.
        int initialFunctionIndex = slot.ExtraProperties * 2 + slot.CurrentInstruction;
        state.Function = GrowingShutterDefinitions.InitialFunction(initialFunctionIndex);

        bool growsUp = slot.ExtraProperties != 0;
        state.GrowthLevel0OriginY = slot.YPosition;
        state.GrowthLevel1OriginY = unchecked((ushort)(slot.YPosition + (growsUp ? -8 : 8)));
        state.GrowthLevel2OriginY = unchecked((ushort)(slot.YPosition + (growsUp ? -16 : 16)));
        state.GrowthLevel3OriginY = unchecked((ushort)(slot.YPosition + (growsUp ? -24 : 24)));
        state.GrowthLevel = 0;

        (state.GrowthVelocity, state.GrowthSubvelocity) = GrowingShutterDefinitions.Speed(slot.Parameter2);

        slot.ExtraProperties = 0;
        InstallGrowingShutterInstruction(
            slot,
            GrowingShutterInstructionProgramDefinitions.TenPixels,
            yRadius: 8);
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

    /// <summary>Switches the shutter from its wait state to directional growth and queues its on-screen activation sound.</summary>
    /// <param name="slot">The room enemy slot whose native function state is being advanced.</param>
    /// <param name="state">The shutter's per-instance growth state.</param>
    /// <param name="growsUp">Whether this activation starts upward rather than downward growth.</param>
    /// <param name="cameraX">The current horizontal camera position used to determine sound visibility.</param>
    /// <param name="cameraY">The current vertical camera position used to determine sound visibility.</param>
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

    /// <summary>Transfers upward shutter movement to Samus when she is standing on the shutter platform.</summary>
    /// <param name="slot">The shutter slot supplying the platform position and collision bounds.</param>
    /// <param name="state">The shutter state containing its position from the preceding update.</param>
    /// <param name="samus">The player state whose extra vertical displacement is adjusted while riding.</param>
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

    /// <summary>Snaps a completed section to its boundary, advances the growth level, and selects the next size instruction.</summary>
    /// <param name="slot">The enemy slot whose position and drawing instruction are updated.</param>
    /// <param name="state">The per-instance growth level to advance.</param>
    /// <param name="target">The exact vertical boundary reached by this section.</param>
    /// <param name="growsUp">Whether the section is extending toward decreasing Y coordinates.</param>
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
            1 => GrowingShutterInstructionProgramDefinitions.TwentyPixels,
            2 => GrowingShutterInstructionProgramDefinitions.ThirtyPixels,
            3 => GrowingShutterInstructionProgramDefinitions.FortyPixels,
            _ => throw new InvalidDataException(
                $"Growing shutter advanced to invalid level {state.GrowthLevel}."),
        };
        ushort radius = unchecked((ushort)(8 + state.GrowthLevel * 8));
        InstallGrowingShutterInstruction(slot, instruction, radius);
    }

    /// <summary>Installs a shutter size instruction and resets its animation timer, growth timer, and vertical collision radius.</summary>
    /// <param name="slot">The enemy slot receiving the instruction and collision settings.</param>
    /// <param name="instruction">The native instruction-list address for the completed growth size.</param>
    /// <param name="yRadius">The vertical collision radius matching that size.</param>
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
