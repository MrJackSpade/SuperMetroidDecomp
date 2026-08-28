using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Exact bank-$A2 function words dispatched by Rio main AI at $A2:BBE3.</summary>
public enum RioEnemyFunction : ushort
{
    WaitingForSamus = 0xbbed,
    WaitingForLandingAnimation = 0xbc32,
    Diving = 0xbc48,
    BouncingBackToPerch = 0xbcb7,
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
    internal const ushort RioDefinition = 0xd27f;

    private const ushort RioInitialInstructionList = 0xbb4b;
    private const ushort RioIdleAfterLandingInstructionList = 0xbb53;
    private const ushort RioDiveInstructionList = 0xbb7f;
    private const ushort RioLateDiveInstructionList = 0xbb97;
    private const ushort RioLandingInstructionList = 0xbba3;
    private const ushort RioTriggerDistance = 0x00a0;
    private const ushort RioDiveSound = 0x0065;
    private const ushort RioGravityStep = 24;

    // These are live ROM words, not friendly host tuning constants. The retail routine
    // reads them at $A2:BBBB/$A2:BBBF immediately before a dive begins.
    private const int RioInitialYVelocityAddress = 0xa2bbbb;
    private const int RioInitialXVelocityAddress = 0xa2bbbf;

    private readonly RioEnemyState?[] _rioStates = new RioEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for every physical enemy slot currently owned by a Rio.</summary>
    public IReadOnlyList<RioEnemyState?> RioStates => _rioStates;

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
        slot.CurrentInstruction = RioInitialInstructionList;
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

                state.YVelocity = ReadWord(_bus!, RioInitialYVelocityAddress);
                state.XVelocity = ReadWord(_bus!, RioInitialXVelocityAddress);
                if (unchecked((short)(samus.XPosition - slot.XPosition)) < 0)
                    state.XVelocity = unchecked((ushort)-(short)state.XVelocity);
                InstallRioInstructionList(slot, state, RioDiveInstructionList);
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
                InstallRioInstructionList(slot, state, RioIdleAfterLandingInstructionList);
                state.Function = RioEnemyFunction.WaitingForSamus;
                return;

            case RioEnemyFunction.Diving:
                RequireRioLevel(level);
                if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level!,
                        slot,
                        ToRioDisplacement(state.XVelocity)))
                {
                    state.XVelocity = unchecked((ushort)-(short)state.XVelocity);
                    ReverseRioVerticalVelocityAndBounce(state);
                    return;
                }
                if (MoveEnemyVertically(level!, slot, ToRioDisplacement(state.YVelocity)))
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
                    InstallRioInstructionList(slot, state, RioLateDiveInstructionList);
                }
                return;

            case RioEnemyFunction.BouncingBackToPerch:
                RequireRioLevel(level);
                if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level!,
                        slot,
                        ToRioDisplacement(state.XVelocity)))
                {
                    state.XVelocity = unchecked((ushort)-(short)state.XVelocity);
                }
                if (MoveEnemyVertically(level!, slot, ToRioDisplacement(state.YVelocity)))
                {
                    InstallRioInstructionList(slot, state, RioLandingInstructionList);
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
                    ToRioDisplacement(state.XVelocity));
                MoveEnemyVertically(level!, slot, ToRioDisplacement(state.YVelocity));
                return;

            default:
                throw new NotSupportedException(
                    $"Rio function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the one private instruction at $A2:BBC3.</summary>
    private bool TryProcessRioInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != RioDefinition || opcode != 0xbbc3)
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
        state.YVelocity = unchecked((ushort)-(short)state.YVelocity);
        state.Function = RioEnemyFunction.BouncingBackToPerch;
    }

    /// <summary>
    /// INT16_SHL8 sign-extends Rio's 8.8 word, then shifts it into a signed 16.16 movement.
    /// </summary>
    private static int ToRioDisplacement(ushort velocity) => unchecked((short)velocity) << 8;

    /// <summary>
    /// Reads one sign-extended entry from kSinCosTable8bit_Sext at $A0:B443. Rio consumes
    /// the raw table sample as an 8.8 velocity; unlike Rinka, it performs no speed multiply.
    /// </summary>
    private ushort ReadRioSignedSineCosineSample(byte angle) =>
        ReadWord(_bus!, 0xa0b443 + angle * 2);

    private static void RequireRioLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Rio movement requires the current room level data.");
    }

    private RioEnemyState RequireRioState(RoomEnemySlot slot) =>
        _rioStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Rio state.");
}
