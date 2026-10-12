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
    private readonly RoomEnemySlot _slot;

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

    /// <summary>Signed 8.8 velocity: the horizontal word is native common variable D and the vertical word variable C; the horizontal word feeds the 16.16 collision helper.</summary>
    public EightEightVector<RoomPixels> Velocity
    {
        get => new(new SnesSignedEightEight(_slot.VariableD), new SnesSignedEightEight(_slot.VariableC));
        internal set
        {
            _slot.VariableD = value.X.RawValue;
            _slot.VariableC = value.Y.RawValue;
        }
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

    private const ushort RioTriggerDistance = 0x00a0;
    private const ushort RioDiveSound = 0x0065;
    private const ushort RioGravityStep = 24;

    // These are live ROM words, not friendly host tuning constants. The retail routine
    // reads them at $A2:BBBB/$A2:BBBF immediately before a dive begins.

    private readonly RioEnemyState?[] _rioStates = new RioEnemyState?[MaximumEnemyCount];

    /// <summary>Most recent library-two sound request produced by Rio during this frame.</summary>
    public ushort? LastRioSoundEffect { get; private set; }

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
        Position16<RoomPixels> camera)
    {
        // Rio_Main advances the global enemy RNG even though this particular family never
        // consumes the returned word. Omitting that apparently-useless call changes every
        // later random enemy in the room, so it is part of Rio's observable behavior.
        EnemyRandom.Generate();

        switch (state.Function)
        {
            case RioEnemyFunction.WaitingForSamus:
                if (samus is null || !IsWithinStrictModularDistance(
                        samus.Position.X.Whole,
                        slot.Position.X.Whole,
                        RioTriggerDistance))
                {
                    return;
                }

                state.Velocity = new EightEightVector<RoomPixels>(
                    new SnesSignedEightEight(RioLaunchDefinitions.RioXVelocity),
                    new SnesSignedEightEight(RioLaunchDefinitions.RioYVelocity));
                if (unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0)
                    state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
                InstallRioInstructionList(
                    slot,
                    state,
                    RioInstructionProgramDefinitions.SwoopingPart1);
                state.Function = RioEnemyFunction.Diving;

                // CheckIfEnemyIsOnScreen returns zero for an on-screen origin. The native
                // branch consequently queues sound $65 only for the visible attack start.
                if (!EnemyWithNormalSpritesIsOffScreen(slot, camera))
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
                        ToEightBitVelocityDisplacement(state.Velocity.X.RawValue)))
                {
                    state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
                    ReverseRioVerticalVelocityAndBounce(state);
                    return;
                }
                if (MoveEnemyVertically(
                        level!,
                        slot,
                        ToEightBitVelocityDisplacement(state.Velocity.Y.RawValue)))
                {
                    ReverseRioVerticalVelocityAndBounce(state);
                    return;
                }

                state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue - RioGravityStep))));
                if (state.Velocity.Y.SignedRawValue < 0)
                {
                    state.SavedHorizontalVelocity = state.Velocity.X.RawValue;
                    state.Velocity = new EightEightVector<RoomPixels>(0, 0);
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
                        ToEightBitVelocityDisplacement(state.Velocity.X.RawValue)))
                {
                    state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
                }
                if (MoveEnemyVertically(
                        level!,
                        slot,
                        ToEightBitVelocityDisplacement(state.Velocity.Y.RawValue)))
                {
                    InstallRioInstructionList(
                        slot,
                        state,
                        RioInstructionProgramDefinitions.SwoopCooldown);
                    state.Function = RioEnemyFunction.WaitingForLandingAnimation;
                }
                else
                {
                    state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue - RioGravityStep))));
                }
                return;

            case RioEnemyFunction.HoveringTowardSamus:
                if (samus is null)
                    throw new InvalidOperationException("Rio homing requires the active Samus actor.");
                RequireRioLevel(level);

                // Once Rio reaches Samus's Y coordinate from above, it restores the saved
                // horizontal dive speed and starts the upward/bounce phase at -1/256 px.
                if (unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)) >= 0)
                {
                    state.Velocity = new EightEightVector<RoomPixels>(
                        new SnesSignedEightEight(state.SavedHorizontalVelocity),
                        new SnesSignedEightEight(0xffff));
                    state.Function = RioEnemyFunction.BouncingBackToPerch;
                    return;
                }

                byte angle = CalculateCartridgeAngle(
                    new(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))));
                state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(ReadRioSignedSineCosineSample(
                    unchecked((byte)(angle + 0x40)))));
                state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(ReadRioSignedSineCosineSample(angle)));
                MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                    level!,
                    slot,
                    ToEightBitVelocityDisplacement(state.Velocity.X.RawValue));
                MoveEnemyVertically(
                    level!,
                    slot,
                    ToEightBitVelocityDisplacement(state.Velocity.Y.RawValue));
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
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Rio ||
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

    private static void ReverseRioVerticalVelocityAndBounce(RioEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.Y.SignedRawValue)));
        state.Function = RioEnemyFunction.BouncingBackToPerch;
    }

    /// <summary>
    /// Reads one sign-extended entry from kSinCosTable8bit_Sext at $A0:B443. Rio consumes
    /// the raw table sample as an 8.8 velocity; unlike Rinka, it performs no speed multiply.
    /// </summary>
    private static ushort ReadRioSignedSineCosineSample(byte angle) =>
        unchecked((ushort)EnemyTrigonometryTables.SignedSine(angle));

    private static void RequireRioLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Rio movement requires the current room level data.");
    }

    private RioEnemyState RequireRioState(RoomEnemySlot slot) =>
        _rioStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Rio state.");
}
