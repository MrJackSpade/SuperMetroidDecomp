namespace SuperMetroid.Core.Game;

/// <summary>
/// Selects one of Boyon's two ROM movement functions at <c>$A2:8718</c>. The values are
/// table indexes, not invented host states: zero dispatches falling and one dispatches
/// rising.
/// </summary>
public enum BoyonBounceMovement : ushort
{
    Falling = 0,
    Rising = 1,
}

/// <summary>
/// Typed view of Boyon's six ordinary enemy variables plus its six parallel extended-WRAM
/// words. The ordinary words deliberately remain backed by <see cref="RoomEnemySlot"/> so
/// a debugger shows the same storage layout as <c>$0FA8-$0FB2</c>.
/// </summary>
public sealed class BoyonEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal BoyonEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Low-byte multiplier selected by parameter-one's low byte.</summary>
    public ushort SpeedMultiplier
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Latest unsigned product of the curve byte and speed multiplier.</summary>
    public ushort Speed
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Target triangular distance in 1/256-pixel units.</summary>
    public ushort JumpHeight
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Wrapped accumulator used only while deriving the initial curve index.</summary>
    public ushort DistanceAccumulator
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Current byte index into the 23-entry triangular speed curve.</summary>
    public ushort SpeedTableIndex
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Native one-word latch preventing a second animation restart mid-bounce.</summary>
    public bool Bouncing
    {
        get => _slot.VariableF != 0;
        internal set => _slot.VariableF = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Curve index at which falling reaches the spawn-height baseline.</summary>
    public ushort InitialBounceSpeedTableIndex { get; internal set; }

    /// <summary>Current indirect movement-table selection.</summary>
    public BoyonBounceMovement BounceMovement { get; internal set; }

    /// <summary>
    /// Native <c>$7E:7804</c>. Set at the baseline and cleared while Samus is close enough
    /// to request another bounce.
    /// </summary>
    public bool BounceDisabled { get; internal set; }

    /// <summary>Per-frame copy restored before movement, matching <c>$7E:7806</c>.</summary>
    public ushort SpeedMultiplierMirror { get; internal set; }

    /// <summary>Prevents the idle instruction list from being reinstalled every frame.</summary>
    public bool IdleDisabled { get; internal set; }

    /// <summary>One-time gate for the initial curve-index calculation.</summary>
    public bool BounceSpeedCalculated { get; internal set; }
}

/// <summary>
/// Literal translation of Boyon enemy <c>$CEBF</c>, covering <c>$A2:86A7-$890A</c>.
/// Boyon has no terrain collision or projectile attack; its entire attack is ordinary body
/// contact while the proximity-controlled vertical bounce is active.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort BoyonDefinition = 0xcebf;

    private const ushort BoyonIdleInstructionList = 0x86a7;
    private const ushort BoyonBouncingInstructionList = 0x86bf;
    private const int BoyonSpeedMultiplierTable = 0xa286df;
    private const int BoyonJumpHeightTable = 0xa286ef;
    private const int BoyonSpeedCurve = 0xa28701;
    private const int BoyonSpeedCurveLength = 23;
    private const ushort BoyonBounceSound = 0x000e;

    private readonly BoyonEnemyState?[] _boyonStates =
        new BoyonEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for all 32 physical Boyon-capable enemy slots.</summary>
    public IReadOnlyList<BoyonEnemyState?> BoyonStates => _boyonStates;

    /// <summary>Last library-two bounce sound requested during the current enemy frame.</summary>
    public ushort? LastBoyonSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Boyon</c> at <c>$A2:871C</c>.</summary>
    private void InitializeBoyon(RoomEnemySlot slot)
    {
        int multiplierIndex = slot.Parameter1 & 0x00ff;
        int jumpHeightIndex = slot.Parameter1 >> 8;
        if (multiplierIndex >= 8 || jumpHeightIndex >= 9)
        {
            throw new InvalidDataException(
                $"Boyon parameter one ${slot.Parameter1:X4} indexes beyond its " +
                "eight multipliers or nine jump heights.");
        }

        // Initialization writes the ordinary common-bank empty map before selecting the
        // idle list. RoomEnemySystem's load tail intentionally restores the same transient
        // empty map; the first processed instruction frame installs real bank-$A2 art.
        slot.SpritemapPointer = 0x804d;
        SetBoyonInstructionList(slot, BoyonIdleInstructionList);

        var state = new BoyonEnemyState(slot)
        {
            BounceMovement = BoyonBounceMovement.Rising,
            SpeedMultiplier = ReadWord(
                _bus!,
                BoyonSpeedMultiplierTable + multiplierIndex * 2),
            JumpHeight = ReadWord(
                _bus!,
                BoyonJumpHeightTable + jumpHeightIndex * 2),
            DistanceAccumulator = 0,
            SpeedTableIndex = 0,
            Bouncing = false,
            InitialBounceSpeedTableIndex = 0,
            BounceDisabled = false,
            SpeedMultiplierMirror = 0,
            IdleDisabled = false,
            BounceSpeedCalculated = false,
        };
        _boyonStates[slot.SlotIndex] = state;
    }

    /// <summary>Ports <c>MainAI_Boyon</c> at <c>$A2:879C</c>.</summary>
    private void RunBoyonMain(RoomEnemySlot slot, BoyonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Boyon proximity AI requires the active Samus actor.");

        // The cartridge spends Boyon's first active AI frame deriving the curve index and
        // returns without moving or testing proximity. Preserving that frame is important:
        // animation and main AI otherwise become one frame early immediately after a load.
        if (!state.BounceSpeedCalculated)
        {
            CalculateInitialBoyonBounceSpeed(state);
            state.BounceSpeedCalculated = true;
            return;
        }

        state.SpeedMultiplierMirror = state.SpeedMultiplier;
        bool samusIsClose = IsBoyonSamusWithinHorizontalRange(slot, state, samus);
        if (samusIsClose)
        {
            // Proximity continuously permits another bounce. The separate Bouncing latch
            // prevents resetting the animation list every frame during the current arc.
            state.BounceDisabled = false;
            state.IdleDisabled = false;
            if (!state.Bouncing)
            {
                state.Bouncing = true;
                SetBoyonInstructionList(slot, BoyonBouncingInstructionList);
            }
        }
        else if (state.BounceDisabled)
        {
            // Boyon may only return to idle after the falling half has reached its baseline.
            // Leaving the trigger range in midair therefore finishes the current arc.
            if (!state.IdleDisabled)
            {
                state.IdleDisabled = true;
                SetBoyonInstructionList(slot, BoyonIdleInstructionList);
            }
            return;
        }

        // The native routine restores variable A from its mirror before indirect dispatch.
        // This looks redundant for retail data but is observable and prevents an animation
        // command or debugger edit during this frame from changing the current displacement.
        state.SpeedMultiplier = state.SpeedMultiplierMirror;
        switch (state.BounceMovement)
        {
            case BoyonBounceMovement.Falling:
                MoveBoyonFalling(slot, state);
                break;
            case BoyonBounceMovement.Rising:
                MoveBoyonRising(slot, state);
                break;
            default:
                throw new NotSupportedException(
                    $"Boyon movement-table index {(ushort)state.BounceMovement} is not translated.");
        }
    }

    /// <summary>Ports the one-time loop at <c>$A2:8755</c>.</summary>
    private void CalculateInitialBoyonBounceSpeed(BoyonEnemyState state)
    {
        // Every iteration multiplies two low bytes through hardware registers $4202/$4203,
        // adds the 16-bit product with wraparound, then compares via the 65C816 negative
        // flag. The curve saturates at $FF once its 23 stored bytes are exhausted.
        for (int guard = 0; guard < ushort.MaxValue; guard++)
        {
            state.Speed = MultiplyBoyonCurveEntry(state);
            state.DistanceAccumulator = unchecked((ushort)(
                state.DistanceAccumulator + state.Speed));
            state.SpeedTableIndex = unchecked((ushort)(state.SpeedTableIndex + 1));

            if (!IsNegative16(state.DistanceAccumulator - state.JumpHeight))
            {
                state.InitialBounceSpeedTableIndex = state.SpeedTableIndex;
                state.BounceDisabled = true;
                state.IdleDisabled = true;
                return;
            }
        }

        throw new InvalidDataException(
            "Boyon initial bounce-speed calculation did not reach its ROM jump height.");
    }

    /// <summary>Ports falling movement at <c>$A2:8801</c>.</summary>
    private void MoveBoyonFalling(RoomEnemySlot slot, BoyonEnemyState state)
    {
        state.SpeedTableIndex = unchecked((ushort)(state.SpeedTableIndex + 1));
        state.Speed = MultiplyBoyonCurveEntry(state);

        // XBA/AND #$00FF discards the product's fractional byte. Boyon has no Y subposition
        // integration here: each frame moves by exactly the product's high byte.
        slot.YPosition = unchecked((ushort)(slot.YPosition + (state.Speed >> 8)));
        if (IsNegative16(state.SpeedTableIndex - state.InitialBounceSpeedTableIndex))
            return;

        state.BounceMovement = BoyonBounceMovement.Rising;
        state.BounceDisabled = true;
        state.Bouncing = false;
    }

    /// <summary>Ports rising movement at <c>$A2:8850</c>.</summary>
    private void MoveBoyonRising(RoomEnemySlot slot, BoyonEnemyState state)
    {
        state.Speed = MultiplyBoyonCurveEntry(state);
        slot.YPosition = unchecked((ushort)(slot.YPosition - (state.Speed >> 8)));
        state.SpeedTableIndex = unchecked((ushort)(state.SpeedTableIndex - 1));
        if ((state.SpeedTableIndex & 0x8000) != 0)
            state.BounceMovement = BoyonBounceMovement.Falling;
    }

    /// <summary>
    /// Recreates the native 8×8 unsigned multiply for the current curve index. Values after
    /// the stored table use the explicit saturated byte rather than reading adjacent ROM.
    /// </summary>
    private ushort MultiplyBoyonCurveEntry(BoyonEnemyState state)
    {
        byte curveValue = state.SpeedTableIndex < BoyonSpeedCurveLength
            ? _bus!.ReadByte(BoyonSpeedCurve + state.SpeedTableIndex)
            : (byte)0xff;
        return unchecked((ushort)(curveValue * (byte)state.SpeedMultiplier));
    }

    /// <summary>Ports <c>IsSamusWithinAPixelColumnsOfEnemy</c> for Boyon's init1 radius.</summary>
    private static bool IsBoyonSamusWithinHorizontalRange(
        RoomEnemySlot slot,
        BoyonEnemyState state,
        SamusState samus)
    {
        _ = state; // Retains a symmetric family-helper signature for debugger inspection.
        int distance = Math.Abs((int)unchecked((short)(samus.XPosition - slot.XPosition)));
        return IsNegative16(distance - slot.Parameter2);
    }

    private static void SetBoyonInstructionList(RoomEnemySlot slot, ushort instructionList)
    {
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private BoyonEnemyState RequireBoyonState(RoomEnemySlot slot) =>
        _boyonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Boyon state.");

    /// <summary>Instruction <c>$A2:88C6</c>: permit the arc and request sound $0E.</summary>
    private void StartBoyonBounce(BoyonEnemyState state)
    {
        state.BounceDisabled = false;
        LastBoyonSoundEffect = BoyonBounceSound;
    }
}
