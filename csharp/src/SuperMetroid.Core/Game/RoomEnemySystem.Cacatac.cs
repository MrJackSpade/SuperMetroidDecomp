namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A2 function pointers stored at Cacatac's <c>$0FB2,x</c>. The stopped
/// pointer is the real RTS used while an attack animation owns the actor.
/// </summary>
public enum CacatacEnemyFunction : ushort
{
    MovingLeft = 0x9fba,
    MovingRight = 0x9fec,
    Stopped = 0xa01b,
}

/// <summary>
/// Population parameter-one's low-byte domain. The idle animation treats either nonzero
/// value as right, but the initializer's three-word pointer table distinguishes the shipped
/// value two as an initial stop.
/// </summary>
public enum CacatacDirection : ushort
{
    Left = 0,
    Right = 1,
    InitiallyStopped = 2,
}

/// <summary>
/// Even byte offsets used by both bank-$A2's spike spawn command and bank-$86's ten-entry
/// instruction/movement tables. Values are not flags and must remain even.
/// </summary>
public enum CacatacSpikeDirection : ushort
{
    LeftFacingUp = 0x00,
    Up = 0x02,
    RightFacingUp = 0x04,
    LeftFacingDown = 0x06,
    Down = 0x08,
    RightFacingDown = 0x0a,
    UpLeft = 0x0c,
    UpRight = 0x0e,
    DownLeft = 0x10,
    DownRight = 0x12,
}

/// <summary>
/// Typed view of Cacatac's six native ordinary words plus its two extended patrol bounds.
/// The native record uniquely uses <c>$0FA8,x</c> for right subspeed; because the generic
/// slot projection begins at <c>$0FAA,x</c>, that one word remains explicit host storage.
/// </summary>
public sealed class CacatacEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal CacatacEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Native <c>$0FA8,x</c>, the fractional half of rightward velocity.</summary>
    public ushort RightSubvelocity { get; internal set; }

    /// <summary>Native <c>$0FAA,x</c>, the whole half of rightward velocity.</summary>
    public ushort RightVelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Native <c>$0FAC,x</c>, the fractional half of leftward velocity.</summary>
    public ushort LeftSubvelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Native <c>$0FAE,x</c>, the whole half of leftward velocity.</summary>
    public ushort LeftVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Native <c>$0FB0,x</c>; zero is left, one is right, two initially stops.</summary>
    public CacatacDirection Direction
    {
        get => (CacatacDirection)_slot.VariableD;
        internal set => _slot.VariableD = (ushort)value;
    }

    /// <summary>Native indirect function pointer at <c>$0FB2,x</c>.</summary>
    public CacatacEnemyFunction Function
    {
        get => (CacatacEnemyFunction)_slot.VariableE;
        internal set => _slot.VariableE = (ushort)value;
    }

    /// <summary>Native extended word <c>$7E:7800,x</c>.</summary>
    public ushort MinimumXPosition { get; internal set; }

    /// <summary>Native extended word <c>$7E:7802,x</c>.</summary>
    public ushort MaximumXPosition { get; internal set; }

    /// <summary>
    /// Parameter-one's high-byte presentation selector. The assembly tests only zero versus
    /// nonzero, so this is a Boolean discriminator rather than a guessed flags enum.
    /// </summary>
    public bool UpsideUp { get; internal set; }
}

/// <summary>
/// Literal translation of retail Cacatac enemy $CFFF, covering $A2:9E6A-$A0BA. Movement
/// uses the cartridge's split 16.16 speed words, attacks pause main AI through the real RTS,
/// and animation commands spawn the corresponding ten bank-$86 spike directions.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort CacatacDefinition = 0xcfff;

    private const ushort CacatacUpsideUpIdleInstructionList = 0x9e8a;
    private const ushort CacatacUpsideUpAttackInstructionList = 0x9eb0;
    private const ushort CacatacUpsideDownIdleInstructionList = 0x9eda;
    private const ushort CacatacUpsideDownAttackInstructionList = 0x9f00;
    private const int CacatacTravelDistanceTable = 0xa29f36;
    private const int CacatacLinearSpeedTable = 0xa08187;
    private const int CacatacLinearSpeedRecordSize = 8;
    private const int CacatacLinearSpeedRecordCount = 0x41;
    private const ushort CacatacSpikeSound = 0x0034;

    private readonly CacatacEnemyState?[] _cacatacStates =
        new CacatacEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for every physical Cacatac-capable enemy slot.</summary>
    public IReadOnlyList<CacatacEnemyState?> CacatacStates => _cacatacStates;

    /// <summary>Last library-two spike-release sound requested in this enemy frame.</summary>
    public ushort? LastCacatacSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Cacatac</c> at $A2:9F48.</summary>
    private void InitializeCacatac(RoomEnemySlot slot)
    {
        int rawDirection = slot.Parameter1 & 0x00ff;
        int distanceIndex = slot.Parameter2 & 0x00ff;
        int speedIndex = slot.Parameter2 >> 8;
        if (rawDirection >= 3)
        {
            throw new InvalidDataException(
                $"Cacatac parameter one ${slot.Parameter1:X4} selects direction " +
                $"{rawDirection}, outside its three-word function table.");
        }
        if (distanceIndex >= 6)
        {
            throw new InvalidDataException(
                $"Cacatac parameter two ${slot.Parameter2:X4} selects travel-distance " +
                $"index {distanceIndex}, outside its six-word table.");
        }
        if (speedIndex >= CacatacLinearSpeedRecordCount)
        {
            throw new InvalidDataException(
                $"Cacatac parameter two ${slot.Parameter2:X4} selects linear-speed " +
                $"index {speedIndex}, outside the NTSC retail table.");
        }

        bool upsideUp = (slot.Parameter1 & 0xff00) != 0;
        slot.SpritemapPointer = 0x804d;
        SetCacatacInstructionList(
            slot,
            upsideUp
                ? CacatacUpsideUpIdleInstructionList
                : CacatacUpsideDownIdleInstructionList);

        ushort distance = ReadWord(_bus!, CacatacTravelDistanceTable + distanceIndex * 2);
        int speedRecord = CacatacLinearSpeedTable + speedIndex * CacatacLinearSpeedRecordSize;
        var state = new CacatacEnemyState(slot)
        {
            RightVelocity = ReadWord(_bus!, speedRecord),
            RightSubvelocity = ReadWord(_bus!, speedRecord + 2),
            LeftVelocity = ReadWord(_bus!, speedRecord + 4),
            LeftSubvelocity = ReadWord(_bus!, speedRecord + 6),
            Direction = (CacatacDirection)rawDirection,
            Function = rawDirection switch
            {
                0 => CacatacEnemyFunction.MovingLeft,
                1 => CacatacEnemyFunction.MovingRight,
                _ => CacatacEnemyFunction.Stopped,
            },
            MaximumXPosition = unchecked((ushort)(slot.XPosition + distance)),
            MinimumXPosition = unchecked((ushort)(slot.XPosition - distance)),
            UpsideUp = upsideUp,
        };
        _cacatacStates[slot.SlotIndex] = state;
    }

    /// <summary>Ports <c>MainAI_Cacatac</c> and its three indirect targets.</summary>
    private void RunCacatacMain(RoomEnemySlot slot, CacatacEnemyState state)
    {
        switch (state.Function)
        {
            case CacatacEnemyFunction.MovingLeft:
                MoveCacatac(slot, state, movingRight: false);
                MaybeStartCacatacAttack(slot, state);
                return;
            case CacatacEnemyFunction.MovingRight:
                MoveCacatac(slot, state, movingRight: true);
                MaybeStartCacatacAttack(slot, state);
                return;
            case CacatacEnemyFunction.Stopped:
                return;
            default:
                throw new InvalidDataException(
                    $"Cacatac function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the split-word additions at $A2:9FBA and $A2:9FEC.</summary>
    private static void MoveCacatac(
        RoomEnemySlot slot,
        CacatacEnemyState state,
        bool movingRight)
    {
        ushort subvelocity = movingRight
            ? state.RightSubvelocity
            : state.LeftSubvelocity;
        ushort velocity = movingRight
            ? state.RightVelocity
            : state.LeftVelocity;

        uint fractionalSum = (uint)slot.XSubposition + subvelocity;
        slot.XSubposition = unchecked((ushort)fractionalSum);
        if (fractionalSum > ushort.MaxValue)
            slot.XPosition = unchecked((ushort)(slot.XPosition + 1));
        slot.XPosition = unchecked((ushort)(slot.XPosition + velocity));

        if (!movingRight)
        {
            if (!IsNegative16(unchecked((ushort)(slot.XPosition - state.MinimumXPosition))))
                return;
            state.Function = CacatacEnemyFunction.MovingRight;
            state.Direction = CacatacDirection.Right;
            return;
        }

        if (IsNegative16(unchecked((ushort)(slot.XPosition - state.MaximumXPosition))))
            return;
        state.Function = CacatacEnemyFunction.MovingLeft;
        state.Direction = CacatacDirection.Left;
    }

    /// <summary>Ports the 3/256 random attack gate at $A2:A01C.</summary>
    private void MaybeStartCacatacAttack(RoomEnemySlot slot, CacatacEnemyState state)
    {
        ushort decision = unchecked((ushort)((_nextRandom!() + slot.FrameCounter) & 0x00ff));
        if (decision >= 3)
            return;

        state.Function = CacatacEnemyFunction.Stopped;
        SetCacatacInstructionList(
            slot,
            state.UpsideUp
                ? CacatacUpsideUpAttackInstructionList
                : CacatacUpsideDownAttackInstructionList);
    }

    /// <summary>Animation instruction $A2:A095: restore patrol from the direction word.</summary>
    private static void RestoreCacatacPatrol(CacatacEnemyState state)
    {
        // The assembly uses BEQ, not CMP #1. Its special initial direction two therefore
        // becomes moving-right once the first idle instruction executes.
        state.Function = state.Direction == CacatacDirection.Left
            ? CacatacEnemyFunction.MovingLeft
            : CacatacEnemyFunction.MovingRight;
    }

    /// <summary>Animation instruction $A2:9F2A: publish library-two sound $34.</summary>
    private void PlayCacatacSpikeSound() => LastCacatacSoundEffect = CacatacSpikeSound;

    private static void SetCacatacInstructionList(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private CacatacEnemyState RequireCacatacState(RoomEnemySlot slot) =>
        _cacatacStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Cacatac state.");
}
