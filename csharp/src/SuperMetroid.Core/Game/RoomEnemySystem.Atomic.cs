namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$A8 vertical movement function stored in Atomic's common variable A.
/// The original symbol calls this the X movement function even though both targets update Y;
/// the debugger-facing name describes the behavior instead of perpetuating that label error.
/// </summary>
public enum AtomicVerticalMovement : ushort
{
    MoveUp = 0xe405,
    MoveDown = 0xe424,
}

/// <summary>
/// Literal bank-$A8 horizontal movement function stored in Atomic's common variable B.
/// The original symbol calls this the Y movement function even though both targets update X.
/// </summary>
public enum AtomicHorizontalMovement : ushort
{
    MoveLeft = 0xe443,
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

    public ushort SpeedFraction
    {
        get => _speedFractions[_slot.SlotIndex];
        internal set => _speedFractions[_slot.SlotIndex] = value;
    }

    public ushort SpeedWhole
    {
        get => _speedWholes[_slot.SlotIndex];
        internal set => _speedWholes[_slot.SlotIndex] = value;
    }

    public ushort NegativeSpeedFraction
    {
        get => _negativeSpeedFractions[_slot.SlotIndex];
        internal set => _negativeSpeedFractions[_slot.SlotIndex] = value;
    }

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
    internal const ushort AtomicDefinition = 0xe9ff;

    private const int AtomicInstructionPointerTable = 0xa8e380;

    private readonly AtomicEnemyState?[] _atomicStates =
        new AtomicEnemyState?[MaximumEnemyCount];
    private readonly ushort[] _atomicSpeedFractions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _atomicSpeedWholes = new ushort[MaximumEnemyCount];
    private readonly ushort[] _atomicNegativeSpeedFractions = new ushort[MaximumEnemyCount];
    private readonly ushort[] _atomicNegativeSpeedWholes = new ushort[MaximumEnemyCount];

    /// <summary>Typed Atomic state for all 32 physical enemy slots.</summary>
    public IReadOnlyList<AtomicEnemyState?> AtomicStates => _atomicStates;

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

        // Parameter one is normally zero through three. Native code performs an unchecked
        // word-table lookup, so retain that behavior by reading the cartridge address rather
        // than imposing a host-only range check or substituting host constants for any entry.
        slot.CurrentInstruction = ReadWord(
            _bus!,
            AtomicInstructionPointerTable + slot.Parameter1 * 2);

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
        state.VerticalMovement = unchecked((short)(samus.YPosition - slot.YPosition)) < 0
            ? AtomicVerticalMovement.MoveUp
            : AtomicVerticalMovement.MoveDown;
        state.HorizontalMovement = unchecked((short)(samus.XPosition - slot.XPosition)) < 0
            ? AtomicHorizontalMovement.MoveLeft
            : AtomicHorizontalMovement.MoveRight;

        switch (state.VerticalMovement)
        {
            case AtomicVerticalMovement.MoveUp:
                (slot.YPosition, slot.YSubposition) = AddAtomicVelocity(
                    slot.YPosition,
                    slot.YSubposition,
                    state.NegativeSpeedWhole,
                    state.NegativeSpeedFraction);
                break;

            case AtomicVerticalMovement.MoveDown:
                (slot.YPosition, slot.YSubposition) = AddAtomicVelocity(
                    slot.YPosition,
                    slot.YSubposition,
                    state.SpeedWhole,
                    state.SpeedFraction);
                break;

            default:
                throw new NotSupportedException(
                    $"Atomic vertical function $A8:{(ushort)state.VerticalMovement:X4} is not translated.");
        }

        switch (state.HorizontalMovement)
        {
            case AtomicHorizontalMovement.MoveLeft:
                (slot.XPosition, slot.XSubposition) = AddAtomicVelocity(
                    slot.XPosition,
                    slot.XSubposition,
                    state.NegativeSpeedWhole,
                    state.NegativeSpeedFraction);
                break;

            case AtomicHorizontalMovement.MoveRight:
                (slot.XPosition, slot.XSubposition) = AddAtomicVelocity(
                    slot.XPosition,
                    slot.XSubposition,
                    state.SpeedWhole,
                    state.SpeedFraction);
                break;

            default:
                throw new NotSupportedException(
                    $"Atomic horizontal function $A8:{(ushort)state.HorizontalMovement:X4} is not translated.");
        }
    }

    /// <summary>
    /// Reproduces each movement target's unusual ordering: add the signed whole word first,
    /// then add the fractional word and increment the already-updated whole on carry.
    /// </summary>
    private static (ushort Position, ushort Subposition) AddAtomicVelocity(
        ushort position,
        ushort subposition,
        ushort wholeVelocity,
        ushort fractionalVelocity)
    {
        position = unchecked((ushort)(position + wholeVelocity));
        uint fractionalSum = (uint)subposition + fractionalVelocity;
        subposition = unchecked((ushort)fractionalSum);
        if (fractionalSum > ushort.MaxValue)
            position = unchecked((ushort)(position + 1));
        return (position, subposition);
    }

    private AtomicEnemyState RequireAtomicState(RoomEnemySlot slot) =>
        _atomicStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Atomic state.");
}
