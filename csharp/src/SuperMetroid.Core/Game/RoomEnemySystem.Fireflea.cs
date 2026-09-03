namespace SuperMetroid.Core.Game;

/// <summary>
/// Typed debugger view of Fireflea's six common-slot words and three per-slot words in
/// extra WRAM <c>$7E:7800</c>. The latter stay in parallel arrays because they are genuinely
/// outside the 64-byte enemy record on the cartridge; flattening them into generic vars
/// would misrepresent the memory contract this decompilation is intended to expose.
/// </summary>
public sealed class FirefleaEnemyState
{
    private readonly RoomEnemySlot _slot;
    private readonly ushort[] _minimumYPositions;
    private readonly ushort[] _maximumYPositions;
    private readonly ushort[] _speedTableIndexes;

    internal FirefleaEnemyState(
        RoomEnemySlot slot,
        ushort[] minimumYPositions,
        ushort[] maximumYPositions,
        ushort[] speedTableIndexes)
    {
        _slot = slot;
        _minimumYPositions = minimumYPositions;
        _maximumYPositions = maximumYPositions;
        _speedTableIndexes = speedTableIndexes;
    }

    /// <summary>Fractional word from the selected signed 16.16 angular speed entry.</summary>
    public ushort SubAngleDelta
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Signed whole word from the selected angular speed entry.</summary>
    public short AngleDelta
    {
        get => unchecked((short)_slot.VariableB);
        internal set => _slot.VariableB = unchecked((ushort)value);
    }

    /// <summary>Movement radius selected by parameter-two's high byte.</summary>
    public ushort Radius
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>
    /// Circle angle in 8.8 form. Main AI uses the high byte as its eight-bit table angle.
    /// </summary>
    public ushort Angle
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    public ushort XCenter
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    public ushort YCenter
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    public ushort MinimumYPosition
    {
        get => _minimumYPositions[_slot.SlotIndex];
        internal set => _minimumYPositions[_slot.SlotIndex] = value;
    }

    public ushort MaximumYPosition
    {
        get => _maximumYPositions[_slot.SlotIndex];
        internal set => _maximumYPositions[_slot.SlotIndex] = value;
    }

    /// <summary>Native byte offset into the linear speed table, not a host array index.</summary>
    public ushort SpeedTableIndex
    {
        get => _speedTableIndexes[_slot.SlotIndex];
        internal set => _speedTableIndexes[_slot.SlotIndex] = value;
    }

    public bool UsesCircularMovement => (_slot.Parameter1 & 0x0002) != 0;
}

/// <summary>Literal translation of Fireflea enemy <c>$D6BF</c> at <c>$A3:8C0F-$8EA4</c>.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort FirefleaDefinition = 0xd6bf;

    private const int FirefleaMovementRadii = 0xa38d1d;

    private readonly ushort[] _firefleaMinimumYPositions =
        new ushort[MaximumEnemyCount];
    private readonly ushort[] _firefleaMaximumYPositions =
        new ushort[MaximumEnemyCount];
    private readonly ushort[] _firefleaSpeedTableIndexes =
        new ushort[MaximumEnemyCount];
    private readonly FirefleaEnemyState?[] _firefleaStates =
        new FirefleaEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Fireflea state for all 32 physical enemy slots.</summary>
    public IReadOnlyList<FirefleaEnemyState?> FirefleaStates => _firefleaStates;

    /// <summary>Ports <c>InitAI_Fireflea</c> at <c>$A3:8D2D</c>.</summary>
    private void InitializeFireflea(RoomEnemySlot slot)
    {
        var state = new FirefleaEnemyState(
            slot,
            _firefleaMinimumYPositions,
            _firefleaMaximumYPositions,
            _firefleaSpeedTableIndexes);
        _firefleaStates[slot.SlotIndex] = state;
        slot.CurrentInstruction = OrdinaryEnemyInstructionLists.FirefleaInitial;

        // Parameter one is native `init0`: bit one selects circle versus vertical motion,
        // bit zero selects the sign half of the speed table, and its high byte is the initial
        // circle angle. Parameter two (`init1`) packs speed index low and radius index high.
        ushort speedTableIndex = unchecked((ushort)((slot.Parameter2 & 0x00ff) * 8));
        if ((slot.Parameter1 & 1) == 0)
            speedTableIndex = unchecked((ushort)(speedTableIndex + 4));
        state.SpeedTableIndex = speedTableIndex;
        (state.AngleDelta, state.SubAngleDelta) = ReadLinearEnemySpeed(speedTableIndex);

        int radiusIndex = (slot.Parameter2 >> 8) & 0xff;
        state.Radius = unchecked((ushort)(ReadWord(
            _bus!,
            FirefleaMovementRadii + radiusIndex * 2) & 0x00ff));

        if (state.UsesCircularMovement)
        {
            state.XCenter = slot.XPosition;
            state.YCenter = slot.YPosition;
            state.Angle = unchecked((ushort)(slot.Parameter1 & 0xff00));

            // The initializer passes the complete 8.8 angle word to an eight-bit routine,
            // which reads its low byte. That is the retail missing-division bug; main AI
            // begins using the high byte on frame one and corrects the position immediately.
            slot.XPosition = unchecked((ushort)(state.XCenter +
                ReadEightBitCosineProduct(state.Angle, state.Radius)));
            slot.YPosition = unchecked((ushort)(state.YCenter +
                ReadEightBitNegativeSineProduct(state.Angle, state.Radius)));
            return;
        }

        state.MinimumYPosition = unchecked((ushort)(slot.YPosition - state.Radius));
        state.MaximumYPosition = unchecked((ushort)(slot.YPosition + state.Radius));
    }

    /// <summary>Ports <c>MainAI_Fireflea</c> at <c>$A3:8DEE</c>.</summary>
    private void RunFirefleaMain(RoomEnemySlot slot, FirefleaEnemyState state)
    {
        if (state.UsesCircularMovement)
        {
            ushort angle = unchecked((ushort)(state.Angle >> 8));
            slot.XPosition = unchecked((ushort)(state.XCenter +
                ReadEightBitCosineProduct(angle, state.Radius)));
            slot.YPosition = unchecked((ushort)(state.YCenter +
                ReadEightBitNegativeSineProduct(angle, state.Radius)));

            // `$A3:8E2D` performs an unaligned word read at var0+1. Combining the high byte
            // of sub-angle with the low byte of whole-angle reproduces that exact signed 8.8
            // delta, including negative half-speeds such as `$FF80`.
            ushort packedDelta = unchecked((ushort)(
                (state.AngleDelta << 8) | (state.SubAngleDelta >> 8)));
            state.Angle = unchecked((ushort)(state.Angle + packedDelta));
            return;
        }

        (short wholeSpeed, ushort fractionalSpeed) =
            ReadLinearEnemySpeed(state.SpeedTableIndex);
        uint fixedY = ((uint)slot.YPosition << 16) | slot.YSubposition;
        int displacement = (wholeSpeed << 16) | fractionalSpeed;
        fixedY = unchecked(fixedY + (uint)displacement);
        slot.YPosition = unchecked((ushort)(fixedY >> 16));
        slot.YSubposition = unchecked((ushort)fixedY);

        // Native CMP/BMI and CMP/BPL consume wrapped signed subtraction, not ordinary host
        // bounds comparisons. The actor is intentionally allowed to overshoot before XOR
        // bit two switches between the adjacent positive/negative table entries.
        bool crossedMinimum = unchecked((short)(
            slot.YPosition - state.MinimumYPosition)) < 0;
        bool reachedMaximum = unchecked((short)(
            slot.YPosition - state.MaximumYPosition)) >= 0;
        if (crossedMinimum || reachedMaximum)
            state.SpeedTableIndex ^= 4;
    }

    /// <summary>
    /// Ports Fireflea touch AI at $A3:8E6B, including its retail double-death bug. The
    /// private handler calls common touch AI and then calls <c>EnemyDeath</c> unconditionally:
    /// normal contact therefore kills an otherwise healthy Fireflea, while Screw Attack and
    /// similar contact attacks can increment the room kill count twice.
    /// </summary>
    private void ResolveFirefleaTouch(
        RoomEnemySlot slot,
        SamusState samus,
        ushort controllerInput)
    {
        ResolveNormalEnemyTouch(slot, samus, controllerInput);

        // EnemyDeath clears the actor even when common touch merely damaged Samus. If common
        // touch already killed it, this second call still increments the native kill counter;
        // retaining that erroneous accounting is necessary for an actual decompilation.
        slot.Health = 0;
        slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
        EnemiesKilled = unchecked((ushort)(EnemiesKilled + 1));
        AdvanceFirefleaDarknessLevel();
    }

    /// <summary>
    /// Advances global <c>$177E</c> after one Fireflea death. Retail rooms contain at most
    /// five, so valid progression is 0,2,4,6,8,10; the original comparison nevertheless
    /// permits 12 and rejects 14, and that exact boundary remains visible here.
    /// </summary>
    private void AdvanceFirefleaDarknessLevel()
    {
        ushort next = unchecked((ushort)(FirefleaDarknessLevel + 2));
        if (unchecked((short)(next - 0x000e)) < 0)
            FirefleaDarknessLevel = next;
    }

    private FirefleaEnemyState RequireFirefleaState(RoomEnemySlot slot) =>
        _firefleaStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Fireflea state.");
}
