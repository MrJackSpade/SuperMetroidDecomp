using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The direction word stored at Stoke WRAM <c>$0FB0,x</c>. Stoke's turn routine deliberately
/// leaves this word unchanged until the newly selected animation executes command
/// <c>$8990</c> or <c>$899D</c>; the enum therefore describes cartridge storage, not merely
/// the sign of the current velocity.
/// </summary>
public enum StokeDirection : ushort
{
    Left = 0,
    Right = 1,
}

/// <summary>
/// Indirect main-AI pointers stored at Stoke WRAM <c>$0FB2,x</c>. The values are the actual
/// bank-$A2 routine offsets so a debugger watch can be compared directly with the ROM.
/// </summary>
public enum StokeAiFunction : ushort
{
    IdleDuringAttack = 0x8a75,
    MovingLeft = 0x8a43,
    MovingRight = 0x8a5c,
}

/// <summary>
/// Typed view over Stoke's six generic enemy-variable words. These properties remain backed
/// by <see cref="RoomEnemySlot"/> rather than duplicating state in host-only fields: inspecting
/// variables A-F consequently shows the same values as <c>$0FA8-$0FB2</c> in the original.
/// </summary>
public sealed class StokeEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal StokeEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Fractional half of the positive 16.16 walking displacement.</summary>
    public ushort RightSubvelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Signed whole half of the positive 16.16 walking displacement.</summary>
    public ushort RightVelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Fractional half of the negative 16.16 walking displacement.</summary>
    public ushort LeftSubvelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Signed whole half of the negative 16.16 walking displacement.</summary>
    public ushort LeftVelocity
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Facing latch changed by animation commands, one frame after a turn request.</summary>
    public StokeDirection Direction
    {
        get => (StokeDirection)_slot.VariableE;
        internal set => _slot.VariableE = (ushort)value;
    }

    /// <summary>Current indirect main-AI routine.</summary>
    public StokeAiFunction Function
    {
        get => (StokeAiFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}

/// <summary>
/// Literal translation of the unused-but-shipped Stoke (mini-Crocomire) actor
/// <c>$A0:CEFF</c>, covering its bank-$A2 loader, walking, ledge/wall turns, random attack
/// choice, and animation-control commands. Its bank-$86 projectile is translated in the
/// adjacent projectile partial rather than being hidden inside this actor state machine.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort StokeDefinition = 0xceff;

    private const ushort StokeMovingLeftInstructionList = 0x8932;
    private const ushort StokeAttackingLeftInstructionList = 0x8948;
    private const ushort StokeMovingRightInstructionList = 0x8958;
    private const ushort StokeAttackingRightInstructionList = 0x896e;
    private const int CommonEnemySpeedTable = 0xa08187;
    private const int CommonEnemySpeedEntrySize = 8;
    private const int MaximumNtscCommonEnemySpeedIndex = 0x40;
    private const int StokeFloorProbeDisplacement = 2 << 16;

    private readonly StokeEnemyState?[] _stokeStates =
        new StokeEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Stoke state for each of the 32 physical enemy slots.</summary>
    public IReadOnlyList<StokeEnemyState?> StokeStates => _stokeStates;

    /// <summary>Ports <c>InitAI_Stoke</c> at <c>$A2:89AD</c>.</summary>
    private void InitializeStoke(RoomEnemySlot slot)
    {
        if (slot.Parameter2 > MaximumNtscCommonEnemySpeedIndex)
        {
            throw new InvalidDataException(
                $"Stoke speed parameter ${slot.Parameter2:X4} exceeds the NTSC common " +
                $"speed table's ${MaximumNtscCommonEnemySpeedIndex:X2} maximum index.");
        }

        // The parameter is an entry number. ASL three times converts it to the byte offset
        // of {positive whole, positive fraction, negative whole, negative fraction}.
        int speedAddress = CommonEnemySpeedTable + slot.Parameter2 * CommonEnemySpeedEntrySize;
        var state = new StokeEnemyState(slot)
        {
            RightVelocity = ReadWord(_bus!, speedAddress),
            RightSubvelocity = ReadWord(_bus!, speedAddress + 2),
            LeftVelocity = ReadWord(_bus!, speedAddress + 4),
            LeftSubvelocity = ReadWord(_bus!, speedAddress + 6),
            Function = StokeAiFunction.MovingLeft,
            Direction = (StokeDirection)slot.Parameter1,
        };
        _stokeStates[slot.SlotIndex] = state;

        // Initialization writes the common empty map, then picks the left list by default.
        // A nonzero init0 takes the native right-facing branch. The room-loader tail later
        // restores the transient empty map, and the first instruction tick supplies art.
        slot.SpritemapPointer = 0x804d;
        SetStokeInstructionList(slot, StokeMovingLeftInstructionList);
        if (slot.Parameter1 != 0)
        {
            SetStokeInstructionList(slot, StokeMovingRightInstructionList);
            state.Function = StokeAiFunction.MovingRight;
        }
    }

    /// <summary>Ports <c>MainAI_Stoke</c> at <c>$A2:89F0</c>.</summary>
    private void RunStokeMain(RoomEnemySlot slot, StokeEnemyState state, RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Stoke walking AI requires the active room level.");

        switch (state.Function)
        {
            case StokeAiFunction.IdleDuringAttack:
                return;

            case StokeAiFunction.MovingLeft:
                RunMovingStoke(
                    slot,
                    state,
                    level,
                    ((int)(short)state.LeftVelocity << 16) | state.LeftSubvelocity,
                    StokeAttackingLeftInstructionList);
                return;

            case StokeAiFunction.MovingRight:
                RunMovingStoke(
                    slot,
                    state,
                    level,
                    ((int)(short)state.RightVelocity << 16) | state.RightSubvelocity,
                    StokeAttackingRightInstructionList);
                return;

            default:
                throw new InvalidDataException(
                    $"Stoke main-AI pointer $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the shared body of moving-left <c>$8A43</c> and moving-right <c>$8A5C</c>.</summary>
    private void RunMovingStoke(
        RoomEnemySlot slot,
        StokeEnemyState state,
        RoomLevelData level,
        int horizontalDisplacement,
        ushort attackingInstructionList)
    {
        MoveStokeAndTurnAtTerrain(slot, state, level, horizontalDisplacement);

        // GenerateRandomNumber advances the global seed before the comparison. Only low
        // byte sums zero and one attack; byte wrapping is intentional and observable.
        ushort random = _nextRandom!();
        byte attackRoll = unchecked((byte)((byte)slot.FrameCounter + (byte)random));
        if (attackRoll >= 2)
            return;

        // The attack list owns Stoke until its trailing goto reaches $8932/$8958 and that
        // wrapper's command installs the corresponding moving function again.
        state.Function = StokeAiFunction.IdleDuringAttack;
        SetStokeInstructionList(slot, attackingInstructionList);
    }

    /// <summary>Ports <c>StokeMovement</c> at <c>$A2:8A76</c>.</summary>
    private static void MoveStokeAndTurnAtTerrain(
        RoomEnemySlot slot,
        StokeEnemyState state,
        RoomLevelData level,
        int horizontalDisplacement)
    {
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalDisplacement))
        {
            TurnStokeAround(slot, state);
            return;
        }

        // $A0:BC76 is a probe, not MoveEnemyDown: it computes a candidate Y coordinate and
        // scans for any level word with bit 15 set without changing Stoke's Y/subposition.
        // Conflating it with vertical movement makes the actor sink two pixels every frame.
        if (!StokeHasSolidFloorTwoPixelsBelow(level, slot))
            TurnStokeAround(slot, state);
    }

    /// <summary>Ports <c>CheckForVerticalSolidBlockCollision</c>'s Stoke use at $A0:BC76.</summary>
    private static bool StokeHasSolidFloorTwoPixelsBelow(RoomLevelData level, RoomEnemySlot slot)
        => EnemyHasSolidHighBitVerticallyAhead(level, slot, StokeFloorProbeDisplacement);

    /// <summary>Ports <c>TurnStokeAround</c> at <c>$A2:8A95</c>.</summary>
    private static void TurnStokeAround(RoomEnemySlot slot, StokeEnemyState state)
    {
        // The routine always selects the left wrapper first, then replaces it with right
        // only when the old direction was not one. It does not update direction/function;
        // the wrapper command executes later in this same enemy instruction phase.
        SetStokeInstructionList(slot, StokeMovingLeftInstructionList);
        if (state.Direction != StokeDirection.Right)
            SetStokeInstructionList(slot, StokeMovingRightInstructionList);
    }

    private static void SetStokeInstructionList(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private StokeEnemyState RequireStokeState(RoomEnemySlot slot) =>
        _stokeStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Stoke state.");

    /// <summary>Instruction <c>$A2:8990</c>: resume leftward movement.</summary>
    private static void SetStokeMovingLeft(StokeEnemyState state)
    {
        state.Function = StokeAiFunction.MovingLeft;
        state.Direction = StokeDirection.Left;
    }

    /// <summary>Instruction <c>$A2:899D</c>: resume rightward movement.</summary>
    private static void SetStokeMovingRight(StokeEnemyState state)
    {
        state.Function = StokeAiFunction.MovingRight;
        state.Direction = StokeDirection.Right;
    }
}
