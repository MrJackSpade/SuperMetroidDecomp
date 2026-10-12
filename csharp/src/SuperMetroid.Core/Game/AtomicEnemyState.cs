namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A8 vertical movement function stored in Atomic's common variable A.
/// The original symbol calls this the X movement function even though both targets update Y;
/// the debugger-facing name describes the behavior instead of perpetuating that label error.
/// </summary>
public enum AtomicVerticalMovement : ushort
{
    /// <summary>$A8:E405, Function_Atomic_MoveUp: add the stored negative whole/fraction speed to Y without terrain collision, selected when wrapped Samus-minus-enemy Y is signed-negative.</summary>
    MoveUp = 0xe405,
    /// <summary>$A8:E424, Function_Atomic_MoveDown: add the positive whole/fraction speed to Y without terrain collision, selected for signed-nonnegative Samus-minus-enemy Y, including equality.</summary>
    MoveDown = 0xe424,
}

/// <summary>
/// Literal bank-$A8 horizontal movement function stored in Atomic's common variable B.
/// The original symbol calls this the Y movement function even though both targets update X.
/// </summary>
public enum AtomicHorizontalMovement : ushort
{
    /// <summary>$A8:E443, Function_Atomic_MoveLeft: add the stored negative whole/fraction speed to X without terrain collision, selected when wrapped Samus-minus-enemy X is signed-negative.</summary>
    MoveLeft = 0xe443,
    /// <summary>$A8:E462, Function_Atomic_MoveRight: add the positive whole/fraction speed to X without terrain collision, selected for signed-nonnegative Samus-minus-enemy X, including equality.</summary>
    MoveRight = 0xe462,
}

/// <summary>
/// Debugger-visible view of Atomic's two common function words and four bank-$7E speed words.
/// Each speed is kept as the cartridge's separate whole/fraction pair. In particular, the
/// negative pair comes from the ROM table and is not recomputed with host signed arithmetic.
/// </summary>
public sealed class AtomicEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Stores speed fractions.</summary>
    private readonly ushort[] _speedFractions;
    /// <summary>Stores speed wholes.</summary>
    private readonly ushort[] _speedWholes;
    /// <summary>Stores negative speed fractions.</summary>
    private readonly ushort[] _negativeSpeedFractions;
    /// <summary>Stores negative speed wholes.</summary>
    private readonly ushort[] _negativeSpeedWholes;

    /// <summary>Initializes a new atomic enemy state instance.</summary>
    internal AtomicEnemyState(
        RoomEnemySlot slot,
        ushort[] speedFractions,
        ushort[] speedWholes,
        ushort[] negativeSpeedFractions,
        ushort[] negativeSpeedWholes)
    {
        _slot = slot;
        _speedFractions = speedFractions;
        _speedWholes = speedWholes;
        _negativeSpeedFractions = negativeSpeedFractions;
        _negativeSpeedWholes = negativeSpeedWholes;
    }

    /// <summary>
    /// Native <c>Atomic.XMovementFunction</c> at enemy variable A. Despite that original
    /// name, the selected routine changes the actor's Y coordinate.
    /// </summary>
    public AtomicVerticalMovement VerticalMovement
    {
        get => (AtomicVerticalMovement)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Native <c>Atomic.YMovementFunction</c> at enemy variable B. Despite that original
    /// name, the selected routine changes the actor's X coordinate.
    /// </summary>
    public AtomicHorizontalMovement HorizontalMovement
    {
        get => (AtomicHorizontalMovement)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>Native $7E:7800 subSpeed word for this slot: positive velocity's low sixteen bits, in 1/65536-pixel units per AI update, added to either coordinate's subposition after its whole-word update.</summary>
    public ushort SpeedFraction
    {
        get => _speedFractions[_slot.SlotIndex];
        internal set => _speedFractions[_slot.SlotIndex] = value;
    }

    /// <summary>Native $7E:7802 speed word for this slot: positive velocity's signed whole-pixel component, loaded from the shared linear-speed record selected by parameter two and used for down/right movement.</summary>
    public ushort SpeedWhole
    {
        get => _speedWholes[_slot.SlotIndex];
        internal set => _speedWholes[_slot.SlotIndex] = value;
    }

    /// <summary>Native $7E:7804 negativeSubSpeed word for this slot: the table-supplied low sixteen bits of the negative velocity, added with carry for up/left movement rather than negating the positive fraction alone.</summary>
    public ushort NegativeSpeedFraction
    {
        get => _negativeSpeedFractions[_slot.SlotIndex];
        internal set => _negativeSpeedFractions[_slot.SlotIndex] = value;
    }

    /// <summary>Native $7E:7806 negativeSpeed word for this slot: the table-supplied signed whole-pixel component for up/left movement, exposed as its raw ushort bits and added before fractional carry.</summary>
    public ushort NegativeSpeedWhole
    {
        get => _negativeSpeedWholes[_slot.SlotIndex];
        internal set => _negativeSpeedWholes[_slot.SlotIndex] = value;
    }
}
