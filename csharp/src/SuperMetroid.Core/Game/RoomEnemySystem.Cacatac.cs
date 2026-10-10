namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A2 function pointers stored at Cacatac's <c>$0FB2,x</c>.
/// Native <c>InitAI_Cacatac</c> reads the three-word table at $A2:9F42 + 2*i
/// for parameter-one low-byte selector i = 0..2: $9FBA moves left, $9FEC
/// moves right, and $A01B is the stopped RTS while an attack animation owns
/// the actor. Other selectors are outside the authored table.
/// </summary>
public enum CacatacEnemyFunction : ushort
{
    /// <summary>$A2:9FBA, <c>Function_Cacatac_MovingLeft</c>: advances the split 16.16 left velocity, turns right below the minimum X bound, then evaluates the random attack gate.</summary>
    MovingLeft = 0x9fba,
    /// <summary>$A2:9FEC, <c>Function_Cacatac_MovingRight</c>: advances the split 16.16 right velocity, turns left at or beyond the maximum X bound, then evaluates the random attack gate.</summary>
    MovingRight = 0x9fec,
    /// <summary>$A2:A01B, <c>RTS_A2A01B</c>: performs no patrol update while attack animation commands own the actor; also the initial target for population direction selector two.</summary>
    Stopped = 0xa01b,
}

/// <summary>
/// Population parameter-one's low-byte domain. The idle animation treats either nonzero
/// value as right, but the initializer's three-word pointer table distinguishes the shipped
/// value two as an initial stop.
/// </summary>
public enum CacatacDirection : ushort
{
    /// <summary>Population and native direction word zero: leftward patrol, restored by animation instruction $A2:A095 after an attack.</summary>
    Left = 0,
    /// <summary>Population and native direction word one: rightward patrol; the restore instruction also treats the separate initial-stop selector two as rightward.</summary>
    Right = 1,
    /// <summary>Population selector two: an initial stop. The word stays two until an attack; the restore instruction then treats it as rightward.</summary>
    InitialStop = 2,
}

/// <summary>
/// Even byte offsets used by both bank-$A2's spike spawn command and bank-$86's ten-entry
/// instruction/movement tables. Values are not flags and must remain even.
/// </summary>
public enum CacatacSpikeDirection : ushort
{
    /// <summary>Selector $00: moves left two pixels per projectile update with the upward-facing horizontal spike program at $86:D92E.</summary>
    LeftFacingUp = 0x00,
    /// <summary>Selector $02: moves straight up two pixels per projectile update with the upward spike program at $86:D93A.</summary>
    Up = 0x02,
    /// <summary>Selector $04: moves right two pixels per projectile update with the upward-facing horizontal spike program at $86:D946.</summary>
    RightFacingUp = 0x04,
    /// <summary>Selector $06: moves left two pixels per projectile update with the downward-facing horizontal spike program at $86:D94C.</summary>
    LeftFacingDown = 0x06,
    /// <summary>Selector $08: moves straight down two pixels per projectile update with the downward spike program at $86:D958.</summary>
    Down = 0x08,
    /// <summary>Selector $0A: moves right two pixels per projectile update with the downward-facing horizontal spike program at $86:D964, natively named <c>InstList_EnemyProjectile_CacatacSpike_Down_FacingRight</c>.</summary>
    RightFacingDown = 0x0a,
    /// <summary>Selector $0C: moves up and left by 1.5 pixels on each axis per projectile update, using the diagonal spike program at $86:D934.</summary>
    UpLeft = 0x0c,
    /// <summary>Selector $0E: moves up and right by 1.5 pixels on each axis per projectile update, using the diagonal spike program at $86:D940.</summary>
    UpRight = 0x0e,
    /// <summary>Selector $10: moves down and left by 1.5 pixels on each axis per projectile update, using the diagonal spike program at $86:D952.</summary>
    DownLeft = 0x10,
    /// <summary>Selector $12: moves down and right by 1.5 pixels on each axis per projectile update, using the diagonal spike program at $86:D95E.</summary>
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

    private const ushort CacatacSpikeSound = 0x0034;

    private readonly CacatacEnemyState?[] _cacatacStates =
        new CacatacEnemyState?[MaximumEnemyCount];

    /// <summary>Last library-two spike-release sound requested in this enemy frame.</summary>
    public ushort? LastCacatacSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Cacatac</c> at $A2:9F48.</summary>
    private void InitializeCacatac(RoomEnemySlot slot)
    {
        var direction = (CacatacDirection)(slot.Parameter1 & 0x00ff);
        int distanceIndex = slot.Parameter2 & 0x00ff;
        int speedIndex = slot.Parameter2 >> 8;
        if (!Enum.IsDefined(direction))
        {
            throw new InvalidDataException(
                $"Cacatac parameter one ${slot.Parameter1:X4} selects direction " +
                $"{(ushort)direction}, outside its three-word function table.");
        }
        if (distanceIndex >= 6)
        {
            throw new InvalidDataException(
                $"Cacatac parameter two ${slot.Parameter2:X4} selects travel-distance " +
                $"index {distanceIndex}, outside its six-word table.");
        }
        if (speedIndex >= EnemyLinearSpeedDefinitions.RecordCount)
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
                ? CacatacInstructionProgramDefinitions.UpsideUpIdle
                : CacatacInstructionProgramDefinitions.UpsideDownIdle);

        ushort distance = CacatacMovementDefinitions.TravelDistance((byte)distanceIndex);
        int speedRecord = speedIndex * EnemyLinearSpeedDefinitions.RecordSize;
        var right = EnemyLinearSpeedDefinitions.Read(speedRecord);
        var left = EnemyLinearSpeedDefinitions.Read(speedRecord + 4);
        var state = new CacatacEnemyState(slot)
        {
            RightVelocity = unchecked((ushort)right.Whole),
            RightSubvelocity = right.Fraction,
            LeftVelocity = unchecked((ushort)left.Whole),
            LeftSubvelocity = left.Fraction,
            Direction = direction,
            Function = direction switch
            {
                CacatacDirection.Left => CacatacEnemyFunction.MovingLeft,
                CacatacDirection.Right => CacatacEnemyFunction.MovingRight,
                CacatacDirection.InitialStop => CacatacEnemyFunction.Stopped,
                _ => throw new InvalidOperationException($"Undefined Cacatac direction {direction}."),
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
                ? CacatacInstructionProgramDefinitions.UpsideUpAttack
                : CacatacInstructionProgramDefinitions.UpsideDownAttack);
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
