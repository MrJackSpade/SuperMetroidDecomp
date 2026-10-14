namespace SuperMetroid.Core.Enemies.Alcoon;

/// <summary>
/// Typed debugger view of Alcoon's common enemy-slot aliases and its five words in extra
/// enemy WRAM. The properties deliberately retain native 16-bit fixed-point halves: this
/// makes carry, signed wrap, and the initializer's surviving landing subposition visible.
/// </summary>
public sealed class AlcoonEnemyState
{
    /// <summary>Common enemy-slot words that hold Alcoon's function and motion state.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Whole vertical acceleration words indexed by native enemy slot.</summary>
    private readonly ushort[] _yAccelerations;
    /// <summary>Fractional vertical acceleration words indexed by native enemy slot.</summary>
    private readonly ushort[] _ySubaccelerations;
    /// <summary>Original whole X positions used to bound Alcoon's walking distance.</summary>
    private readonly ushort[] _spawnXPositions;
    /// <summary>Floor Y positions found during Alcoon's initialization jump.</summary>
    private readonly ushort[] _landingYPositions;
    /// <summary>Walking cycles remaining until the next fire volley.</summary>
    private readonly ushort[] _stepCounters;

    /// <summary>Maps one enemy slot onto its parallel extra-WRAM arrays without copying their values.</summary>
    internal AlcoonEnemyState(
        RoomEnemySlot slot,
        ushort[] yAccelerations,
        ushort[] ySubaccelerations,
        ushort[] spawnXPositions,
        ushort[] landingYPositions,
        ushort[] stepCounters)
    {
        _slot = slot;
        _yAccelerations = yAccelerations;
        _ySubaccelerations = ySubaccelerations;
        _spawnXPositions = spawnXPositions;
        _landingYPositions = landingYPositions;
        _stepCounters = stepCounters;
    }

    /// <summary>Bank-$A8 indirect AI target, represented by common variable A here and corresponding to native <c>Alcoon.function</c> at $0FA8,x.</summary>
    public AlcoonEnemyFunction Function
    {
        get => (AlcoonEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Motion per step. Y is the signed 16.16 vertical velocity in native $0FAA,x (whole) and
    /// $0FAC,x (fraction): emergence starts at -12 and retreat at -4, and acceleration carries
    /// from the fraction into the whole word. X is the signed whole-pixel horizontal velocity in
    /// common variable D (retail uses -2, 0 or +2); Alcoon has no horizontal fraction word, so
    /// a velocity with a nonzero X fraction is rejected.
    /// </summary>
    public FixedVector<RoomPixels> Velocity
    {
        get => FixedVector<RoomPixels>.FromWords(_slot.VariableD, 0, _slot.VariableB, _slot.VariableC);
        internal set
        {
            if (value.X.Fraction != 0)
                throw new InvalidOperationException(
                    $"Alcoon has no horizontal subvelocity word; X fraction ${value.X.Fraction:X4} cannot be stored.");
            _slot.VariableB = value.Y.WholeWord;
            _slot.VariableC = value.Y.Fraction;
            _slot.VariableD = value.X.WholeWord;
        }
    }

    /// <summary>
    /// Common variable four is cleared by <c>SetupAlcoonJumpMovement</c> even though this
    /// actor never reads it. Retaining the alias prevents the translated setup from quietly
    /// omitting an observable WRAM side effect.
    /// </summary>
    public ushort ClearedVariable
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>
    /// Original population whole-pixel position. Y (native $0FB2,x, common variable F) is
    /// restored after the initialization floor search and after retreat, leaving the live Y
    /// subposition unchanged; X (native $7E:7804,x) enforces the 112-pixel walking range and is
    /// restored when hiding completes.
    /// </summary>
    public Position16<RoomPixels> SpawnPosition
    {
        get => new(_spawnXPositions[_slot.SlotIndex], _slot.VariableF);
        internal set
        {
            _spawnXPositions[_slot.SlotIndex] = value.X;
            _slot.VariableF = value.Y;
        }
    }

    /// <summary>Whole-pixel component of the vertical velocity increment per acceleration step, paired with <see cref="YSubacceleration"/>; jump setup writes zero. Native $7E:7800,x.</summary>
    public ushort YAcceleration
    {
        get => _yAccelerations[_slot.SlotIndex];
        internal set => _yAccelerations[_slot.SlotIndex] = value;
    }

    /// <summary>Fractional vertical velocity increment in 1/65536 pixel per motion-step squared; jump setup writes $8000, adding half a pixel per step to downward velocity. Native $7E:7802,x.</summary>
    public ushort YSubacceleration
    {
        get => _ySubaccelerations[_slot.SlotIndex];
        internal set => _ySubaccelerations[_slot.SlotIndex] = value;
    }

    /// <summary>Floor-aligned center Y discovered by the complete initialization jump.</summary>
    public ushort LandingYPosition
    {
        get => _landingYPositions[_slot.SlotIndex];
        internal set => _landingYPositions[_slot.SlotIndex] = value;
    }

    /// <summary>Number of four-step walking cycles remaining before another fire volley.</summary>
    public NativeCountdown StepCounter
    {
        get => NativeCountdown.FromRaw(_stepCounters[_slot.SlotIndex]);
        internal set => _stepCounters[_slot.SlotIndex] = value.RawValue;
    }
}
