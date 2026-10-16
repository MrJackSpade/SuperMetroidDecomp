using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
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
    /// <summary>The slot's Alcoon extension words, kept for the room across occupants.</summary>
    private readonly AlcoonExtensionWords _words;

    /// <summary>Maps one enemy slot onto its parallel extra-WRAM arrays without copying their values.</summary>
    internal AlcoonEnemyState(
        RoomEnemySlot slot, AlcoonExtensionWords words)
    {
        _slot = slot;
        _words = words;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<AlcoonProgram> Program { get; set; }

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
        get => new(_words.SpawnXPosition, _slot.VariableF);
        internal set
        {
            _words.SpawnXPosition = value.X;
            _slot.VariableF = value.Y;
        }
    }

    /// <summary>Whole-pixel component of the vertical velocity increment per acceleration step, paired with <see cref="YSubacceleration"/>; jump setup writes zero. Native $7E:7800,x.</summary>
    public ushort YAcceleration
    {
        get => _words.YAcceleration;
        internal set => _words.YAcceleration = value;
    }

    /// <summary>Fractional vertical velocity increment in 1/65536 pixel per motion-step squared; jump setup writes $8000, adding half a pixel per step to downward velocity. Native $7E:7802,x.</summary>
    public ushort YSubacceleration
    {
        get => _words.YSubacceleration;
        internal set => _words.YSubacceleration = value;
    }

    /// <summary>Floor-aligned center Y discovered by the complete initialization jump.</summary>
    public ushort LandingYPosition
    {
        get => _words.LandingYPosition;
        internal set => _words.LandingYPosition = value;
    }

    /// <summary>Number of four-step walking cycles remaining before another fire volley.</summary>
    public NativeCountdown StepCounter
    {
        get => NativeCountdown.FromRaw(_words.StepCounter);
        internal set => _words.StepCounter = value.RawValue;
    }
}
