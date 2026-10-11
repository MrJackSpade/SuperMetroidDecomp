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
    private readonly RoomEnemySlot _slot;
    private readonly ushort[] _speedFractions;
    private readonly ushort[] _speedWholes;
    private readonly ushort[] _negativeSpeedFractions;
    private readonly ushort[] _negativeSpeedWholes;

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

/// <summary>
/// Literal translation of Atomic enemy <c>$E9FF</c> from <c>$A8:E230-$E586</c>. Atomic has
/// no private projectile attack or collision routine: it continuously chases Samus on both
/// axes, deals the header's 40 contact damage, and uses the common vulnerability/shot/death,
/// power-bomb, freeze, and grapple-cancel paths.
/// </summary>
public sealed partial class RoomEnemySystem
{

    private readonly AtomicEnemyState?[] _atomicStates =
        new AtomicEnemyState?[MaximumEnemyCount];
    private readonly ushort[] _atomicSpeedFractions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _atomicSpeedWholes = new ushort[MaximumEnemyCount];
    private readonly ushort[] _atomicNegativeSpeedFractions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _atomicNegativeSpeedWholes = new ushort[MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Atomic</c> at <c>$A8:E388</c>.</summary>
    private void InitializeAtomic(RoomEnemySlot slot)
    {
        var state = new AtomicEnemyState(
            slot,
            _atomicSpeedFractions,
            _atomicSpeedWholes,
            _atomicNegativeSpeedFractions,
            _atomicNegativeSpeedWholes);
        _atomicStates[slot.SlotIndex] = state;

        slot.InstructionTimer = 1;
        slot.Timer = 0;

        slot.CurrentInstruction = AtomicMovementDefinitions.InitialInstructionList(slot.Parameter1);

        // Parameter two is an index into the shared eight-byte linear-speed record:
        // positive whole/fraction followed by its ROM-generated negative whole/fraction.
        // Keeping all four words exactly avoids the one-subpixel errors caused by rebuilding
        // the negative pair from a C# integer after the fact.
        ushort speedTableOffset = unchecked((ushort)(slot.Parameter2 * 8));
        (short positiveWhole, ushort positiveFraction) =
            ReadLinearEnemySpeed(speedTableOffset);
        (short negativeWhole, ushort negativeFraction) =
            ReadLinearEnemySpeed(unchecked((ushort)(speedTableOffset + 4)));
        state.SpeedWhole = unchecked((ushort)positiveWhole);
        state.SpeedFraction = positiveFraction;
        state.NegativeSpeedWhole = unchecked((ushort)negativeWhole);
        state.NegativeSpeedFraction = negativeFraction;
    }

    /// <summary>
    /// Ports <c>MainAI_Atomic</c> at <c>$A8:E3C3</c>, including all four indirect targets.
    /// Atomic deliberately ignores terrain and chooses both signs again every frame.
    /// </summary>
    private static void RunAtomicMain(
        RoomEnemySlot slot,
        AtomicEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Atomic movement requires the active Samus actor.");

        // The cartridge treats equality as the positive direction: down for Y and right for
        // X. Compare the wrapped 16-bit subtraction as signed, matching BMI rather than a host
        // integer comparison that would disagree across the $0000/$FFFF coordinate boundary.
        state.VerticalMovement = unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole)) < 0
            ? AtomicVerticalMovement.MoveUp
            : AtomicVerticalMovement.MoveDown;
        state.HorizontalMovement = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0
            ? AtomicHorizontalMovement.MoveLeft
            : AtomicHorizontalMovement.MoveRight;

        switch (state.VerticalMovement)
        {
            case AtomicVerticalMovement.MoveUp:
                slot.Position = slot.Position.WithY(AddAtomicVelocity(
                    slot.Position.Y,
                    state.NegativeSpeedWhole,
                    state.NegativeSpeedFraction));
                break;

            case AtomicVerticalMovement.MoveDown:
                slot.Position = slot.Position.WithY(AddAtomicVelocity(
                    slot.Position.Y,
                    state.SpeedWhole,
                    state.SpeedFraction));
                break;

            default:
                throw new InvalidDataException(
                    $"Atomic vertical function $A8:{(ushort)state.VerticalMovement:X4} is not translated.");
        }

        switch (state.HorizontalMovement)
        {
            case AtomicHorizontalMovement.MoveLeft:
                slot.Position = slot.Position.WithX(AddAtomicVelocity(
                    slot.Position.X,
                    state.NegativeSpeedWhole,
                    state.NegativeSpeedFraction));
                break;

            case AtomicHorizontalMovement.MoveRight:
                slot.Position = slot.Position.WithX(AddAtomicVelocity(
                    slot.Position.X,
                    state.SpeedWhole,
                    state.SpeedFraction));
                break;

            default:
                throw new InvalidDataException(
                    $"Atomic horizontal function $A8:{(ushort)state.HorizontalMovement:X4} is not translated.");
        }
    }

    /// <summary>
    /// Reproduces each movement target's unusual ordering: add the signed whole word first,
    /// then add the fractional word and increment the already-updated whole on carry.
    /// </summary>
    private static SnesFixedPosition AddAtomicVelocity(
        SnesFixedPosition position,
        ushort wholeVelocity,
        ushort fractionalVelocity)
    {
        ushort whole = unchecked((ushort)(position.Whole + wholeVelocity));
        uint fractionalSum = (uint)position.Fraction + fractionalVelocity;
        if (fractionalSum > ushort.MaxValue)
            whole = unchecked((ushort)(whole + 1));
        return new(whole, unchecked((ushort)fractionalSum));
    }

    private AtomicEnemyState RequireAtomicState(RoomEnemySlot slot) =>
        _atomicStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Atomic state.");
}
