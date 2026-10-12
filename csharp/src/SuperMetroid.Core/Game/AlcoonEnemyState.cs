using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The literal bank-$A8 function pointer stored in Alcoon's common variable zero.</summary>
public enum AlcoonEnemyFunction : ushort
{
    /// <summary>$A8:DD71, <c>Function_Alcoon_WaitForSamusToGetNear</c>: starts the emergence jump when Samus is strictly within 80 horizontal pixels and 32 pixels of the previously discovered landing Y.</summary>
    WaitingForSamus = 0xdd71,
    /// <summary>$A8:DDC6, <c>Function_Alcoon_Emerging_Rising</c>: integrates upward vertical motion with half-pixel acceleration until the whole velocity becomes nonnegative, then selects the falling pose.</summary>
    EmergingRising = 0xddc6,
    /// <summary>$A8:DE05, <c>Function_Alcoon_Emerging_Falling</c>: descends through the terrain collision mover, then faces Samus and begins walking with one walking cycle remaining.</summary>
    EmergingFalling = 0xde05,
    /// <summary>$A8:DE4B, <c>Function_Alcoon_MoveHorizontally_SpitFireballsAtSamus</c>: follows the floor, hides after travelling 112 pixels from spawn, or requests a volley when the step counter is zero and Samus is ahead; animation commands perform horizontal movement.</summary>
    WalkingAndFiring = 0xde4b,
    /// <summary>$A8:DECC, <c>RTL_A8DECC</c>: performs no main-AI movement while the fire-volley animation runs, until its command restores walking.</summary>
    WaitingForFireAnimation = 0xdecc,
    /// <summary>$A8:DECD, <c>Function_Alcoon_Hiding_Rising</c>: integrates the retreat jump's upward motion until acceleration changes its whole velocity to nonnegative.</summary>
    HidingRising = 0xdecd,
    /// <summary>$A8:DEEC, <c>Function_Alcoon_Hiding_Falling</c>: falls without terrain collision until reaching spawn Y, then restores the whole spawn coordinates and proximity-wait state while preserving subpositions.</summary>
    HidingFalling = 0xdeec,
}

/// <summary>
/// Typed debugger view of Alcoon's common enemy-slot aliases and its five words in extra
/// enemy WRAM. The properties deliberately retain native 16-bit fixed-point halves: this
/// makes carry, signed wrap, and the initializer's surviving landing subposition visible.
/// </summary>
public sealed class AlcoonEnemyState
{
    /// <summary>Stores slot.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>Stores y accelerations.</summary>
    private readonly ushort[] _yAccelerations;
    /// <summary>Stores y subaccelerations.</summary>
    private readonly ushort[] _ySubaccelerations;
    /// <summary>Stores spawn xpositions.</summary>
    private readonly ushort[] _spawnXPositions;
    /// <summary>Stores landing ypositions.</summary>
    private readonly ushort[] _landingYPositions;
    /// <summary>Stores step counters.</summary>
    private readonly ushort[] _stepCounters;

    /// <summary>Initializes a new alcoon enemy state instance.</summary>
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
    public ushort StepCounter
    {
        get => _stepCounters[_slot.SlotIndex];
        internal set => _stepCounters[_slot.SlotIndex] = value;
    }
}
