using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Exact bank-$A2 function words dispatched by Rio main AI at $A2:BBE3.</summary>
public enum RioEnemyFunction : ushort
{
    /// <summary>$A2:BBED Function_Rio_WaitForSamusToGetNear: starts a dive when Samus is within a strict 160-world-pixel horizontal distance, selecting launch direction toward her.</summary>
    WaitingForSamus = 0xbbed,
    /// <summary>$A2:BC32 Function_Rio_SwoopCooldown: waits for the $A2:BBC3 animation handshake, then installs the post-swoop idle list and resumes proximity waiting.</summary>
    WaitingForLandingAnimation = 0xbc32,
    /// <summary>$A2:BC48 Function_Rio_Swoop_Descending: moves with signed 8.8 velocities, subtracting $0018 from Y velocity per AI update until homing begins or collision starts the bounce.</summary>
    Diving = 0xbc48,
    /// <summary>$A2:BCB7 Function_Rio_Swoop_Ascending: follows the upward bounce with signed 8.8 acceleration; horizontal collision reverses X, and vertical collision starts the cooldown animation.</summary>
    BouncingBackToPerch = 0xbcb7,
    /// <summary>$A2:BCFF Function_Rio_Homing: uses angle-table samples as 8.8 velocities while above Samus, then restores saved horizontal speed and begins ascent at Y velocity $FFFF.</summary>
    HoveringTowardSamus = 0xbcff,
}

/// <summary>
/// Named projection of Rio's six native AI variables. The properties deliberately remain
/// backed by the physical enemy slot so a debugger watch agrees with WRAM $0FA8-$0FB2.
/// </summary>
public sealed class RioEnemyState
{
    /// <summary>Physical slot whose native variable words back this debugger-facing Rio state view.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a state view over one initialized Rio enemy slot.</summary>
    /// <param name="slot">The enemy slot containing Rio's native AI variables.</param>
    internal RioEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Horizontal velocity retained while the hovering phase temporarily stops X.</summary>
    public ushort SavedHorizontalVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Gets the bank-$A2 function pointer corresponding to native $0FAA,x Rio.function, backed by the physical slot and dispatched after the main AI's RNG advance.</summary>
    public RioEnemyFunction Function
    {
        get => (RioEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>Signed 8.8 vertical velocity passed to the 16.16 collision helper.</summary>
    public ushort YVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Signed 8.8 horizontal velocity passed to the 16.16 collision helper.</summary>
    public ushort XVelocity
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Handshake set by instruction $A2:BBC3 and consumed by main AI.</summary>
    public bool AnimationFinished
    {
        get => _slot.VariableE != 0;
        internal set => _slot.VariableE = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Last instruction-list address installed by Rio_6; zero at room load.</summary>
    public ushort InstalledInstructionList
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }
}

/// <summary>Literal translation of Rio enemy AI $A2:BBC3-$BD6B.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Bank-local enemy definition pointer used to identify Rio during initialization and instruction dispatch.</summary>
    internal const ushort RioDefinition = 0xd27f;

    /// <summary>Strict horizontal proximity threshold, in world pixels, that starts a Rio dive.</summary>
    private const ushort RioTriggerDistance = 0x00a0;

    /// <summary>Library-two sound effect requested when the visible Rio dive begins.</summary>
    private const ushort RioDiveSound = 0x0065;

    /// <summary>Amount subtracted from signed 8.8 vertical velocity on each gravity-driven AI update.</summary>
    private const ushort RioGravityStep = 24;

    // These are live ROM words, not friendly host tuning constants. The retail routine
    // reads them at $A2:BBBB/$A2:BBBF immediately before a dive begins.

    /// <summary>Stores initialized Rio state views by enemy slot index for the current room.</summary>
    private readonly RioEnemyState?[] _rioStates = new RioEnemyState?[MaximumEnemyCount];

    /// <summary>Most recent library-two sound request produced by Rio during this frame.</summary>
    public ushort? LastRioSoundEffect { get; private set; }

    /// <summary>Clears all per-slot Rio state and any pending dive sound when room state is reset.</summary>
    private void ResetRioRoomState()
    {
        Array.Clear(_rioStates);
        LastRioSoundEffect = null;
    }

    /// <summary>Ports <c>Rio_Init</c> at $A2:BBCD.</summary>
    private void InitializeRio(RoomEnemySlot slot)
    {
        var state = new RioEnemyState(slot)
        {
            AnimationFinished = false,
            InstalledInstructionList = 0,
            Function = RioEnemyFunction.WaitingForSamus,
        };
        _rioStates[slot.SlotIndex] = state;

        // Rio_Init writes the list directly rather than calling Rio_6. Variable F must
        // therefore remain zero until the first behavioral transition installs a new list.
        slot.CurrentInstruction = RioInstructionProgramDefinitions.Idle;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Ports <c>Rio_Main</c> and Rio_1..Rio_5 at $A2:BBE3-$BD53.</summary>
    private void RunRioMain(
        RoomEnemySlot slot,
        RioEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        ushort cameraX,
        ushort cameraY)
    {
        // Rio_Main advances the global enemy RNG even though this particular family never
        // consumes the returned word. Omitting that apparently-useless call changes every
        // later random enemy in the room, so it is part of Rio's observable behavior.
        _nextRandom!();

        switch (state.Function)
        {
            case RioEnemyFunction.WaitingForSamus:
                if (samus is null || !IsWithinStrictModularDistance(
                        samus.XPosition,
                        slot.XPosition,
                        RioTriggerDistance))
                {
                    return;
                }

                state.YVelocity = RioLaunchDefinitions.RioYVelocity;
                state.XVelocity = RioLaunchDefinitions.RioXVelocity;
                if (unchecked((short)(samus.XPosition - slot.XPosition)) < 0)
                    state.XVelocity = unchecked((ushort)-(short)state.XVelocity);
                InstallRioInstructionList(
                    slot,
                    state,
                    RioInstructionProgramDefinitions.SwoopingPart1);
                state.Function = RioEnemyFunction.Diving;

                // CheckIfEnemyIsOnScreen returns zero for an on-screen origin. The native
                // branch consequently queues sound $65 only for the visible attack start.
                if (!EnemyWithNormalSpritesIsOffScreen(slot, cameraX, cameraY))
                    LastRioSoundEffect = RioDiveSound;
                return;

            case RioEnemyFunction.WaitingForLandingAnimation:
                if (!state.AnimationFinished)
                    return;
                state.AnimationFinished = false;
                InstallRioInstructionList(
                    slot,
                    state,
                    RioInstructionProgramDefinitions.PostSwoopIdle);
                state.Function = RioEnemyFunction.WaitingForSamus;
                return;

            case RioEnemyFunction.Diving:
                RequireRioLevel(level);
                if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level!,
                        slot,
                        ToEightBitVelocityDisplacement(state.XVelocity)))
                {
                    state.XVelocity = unchecked((ushort)-(short)state.XVelocity);
                    ReverseRioVerticalVelocityAndBounce(state);
                    return;
                }
                if (MoveEnemyVertically(
                        level!,
                        slot,
                        ToEightBitVelocityDisplacement(state.YVelocity)))
                {
                    ReverseRioVerticalVelocityAndBounce(state);
                    return;
                }

                state.YVelocity = unchecked((ushort)(state.YVelocity - RioGravityStep));
                if (unchecked((short)state.YVelocity) < 0)
                {
                    state.SavedHorizontalVelocity = state.XVelocity;
                    state.XVelocity = 0;
                    state.YVelocity = 0;
                    state.Function = RioEnemyFunction.HoveringTowardSamus;
                }
                else if (state.AnimationFinished)
                {
                    state.AnimationFinished = false;
                    InstallRioInstructionList(
                        slot,
                        state,
                        RioInstructionProgramDefinitions.SwoopingPart2);
                }
                return;

            case RioEnemyFunction.BouncingBackToPerch:
                RequireRioLevel(level);
                if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level!,
                        slot,
                        ToEightBitVelocityDisplacement(state.XVelocity)))
                {
                    state.XVelocity = unchecked((ushort)-(short)state.XVelocity);
                }
                if (MoveEnemyVertically(
                        level!,
                        slot,
                        ToEightBitVelocityDisplacement(state.YVelocity)))
                {
                    InstallRioInstructionList(
                        slot,
                        state,
                        RioInstructionProgramDefinitions.SwoopCooldown);
                    state.Function = RioEnemyFunction.WaitingForLandingAnimation;
                }
                else
                {
                    state.YVelocity = unchecked((ushort)(state.YVelocity - RioGravityStep));
                }
                return;

            case RioEnemyFunction.HoveringTowardSamus:
                if (samus is null)
                    throw new InvalidOperationException("Rio homing requires the active Samus actor.");
                RequireRioLevel(level);

                // Once Rio reaches Samus's Y coordinate from above, it restores the saved
                // horizontal dive speed and starts the upward/bounce phase at -1/256 px.
                if (unchecked((short)(slot.YPosition - samus.YPosition)) >= 0)
                {
                    state.XVelocity = state.SavedHorizontalVelocity;
                    state.YVelocity = 0xffff;
                    state.Function = RioEnemyFunction.BouncingBackToPerch;
                    return;
                }

                byte angle = CalculateCartridgeAngle(
                    unchecked((short)(samus.XPosition - slot.XPosition)),
                    unchecked((short)(samus.YPosition - slot.YPosition)));
                state.XVelocity = ReadRioSignedSineCosineSample(
                    unchecked((byte)(angle + 0x40)));
                state.YVelocity = ReadRioSignedSineCosineSample(angle);
                MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                    level!,
                    slot,
                    ToEightBitVelocityDisplacement(state.XVelocity));
                MoveEnemyVertically(
                    level!,
                    slot,
                    ToEightBitVelocityDisplacement(state.YVelocity));
                return;

            default:
                throw new InvalidDataException(
                    $"Rio function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the one private instruction at $A2:BBC3.</summary>
    private bool TryProcessRioInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != RioDefinition ||
            opcode != RioInstructionCodes.SetAnimationFinished)
            return false;

        RequireRioState(slot).AnimationFinished = true;
        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    /// <summary>Ports Rio_6: install a list only when its address actually changes.</summary>
    private static void InstallRioInstructionList(
        RoomEnemySlot slot,
        RioEnemyState state,
        ushort instructionList)
    {
        if (state.InstalledInstructionList == instructionList)
            return;
        state.InstalledInstructionList = instructionList;
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Reverses Rio's signed vertical velocity and switches its AI to the upward bounce phase.</summary>
    /// <param name="state">The Rio state whose velocity and function are updated.</param>
    private static void ReverseRioVerticalVelocityAndBounce(RioEnemyState state)
    {
        state.YVelocity = unchecked((ushort)-(short)state.YVelocity);
        state.Function = RioEnemyFunction.BouncingBackToPerch;
    }

    /// <summary>
    /// Reads one sign-extended entry from kSinCosTable8bit_Sext at $A0:B443. Rio consumes
    /// the raw table sample as an 8.8 velocity; unlike Rinka, it performs no speed multiply.
    /// </summary>
    private static ushort ReadRioSignedSineCosineSample(byte angle) =>
        unchecked((ushort)EnemyTrigonometryTables.SignedSine(angle));

    /// <summary>Requires room level data before executing a Rio movement phase that collides with the level.</summary>
    /// <param name="level">The current room's level data, if available.</param>
    /// <exception cref="InvalidOperationException">Rio movement was requested without current room level data.</exception>
    private static void RequireRioLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Rio movement requires the current room level data.");
    }

    /// <summary>Returns the initialized per-enemy Rio state associated with a physical slot.</summary>
    /// <param name="slot">The enemy slot whose Rio state is required.</param>
    /// <returns>The state view created for that slot during Rio initialization.</returns>
    /// <exception cref="InvalidOperationException">The slot does not contain initialized Rio state.</exception>
    private RioEnemyState RequireRioState(RoomEnemySlot slot) =>
        _rioStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Rio state.");
}
