using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// Bull's state: its six common slot variables and its per-slot extension words. Speed and acceleration stay the exact
/// words the cartridge keeps, because a deceleration that overshoots leaves a wrapped word that the next update reads by
/// its sign; they change only through <see cref="Accelerate"/> and <see cref="Decelerate"/>.
/// </summary>
public sealed class BullEnemyState
{
    /// <summary>Common slot words carrying Bull's dispatcher, flight speed, acceleration, and timers.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>The slot's words that outlive the occupant.</summary>
    private readonly BullExtensionWords _words;

    /// <summary>Exposes one Bull slot's common words and its extension words without copying state.</summary>
    internal BullEnemyState(RoomEnemySlot slot, BullExtensionWords words)
    {
        _words = words;
        _slot = slot;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<BullProgram> Program { get; set; }

    /// <summary>Native variable A ($0FA8 plus slot byte index): the current bank-$A8 delay, target, acceleration, or deceleration dispatcher pointer.</summary>
    public BullEnemyFunction Function
    {
        get => (BullEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>
    /// Variable B: the speed change applied at each interval expiry, in 256ths of a pixel per update, as the exact word
    /// the cartridge keeps (0 through 65535). It grows or shrinks by <see cref="AccelerationDelta"/> through
    /// <see cref="Accelerate"/> and <see cref="Decelerate"/>; a word with its sign bit set reads as below zero.
    /// </summary>
    public int Acceleration
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = checked((ushort)value);
    }

    /// <summary>Variable C: the change in <see cref="Acceleration"/> at each interval expiry, 24 (a 256th of a pixel per update each) while flying and zero once the flight cycle stops.</summary>
    public int AccelerationDelta
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = checked((ushort)value);
    }

    /// <summary>
    /// Variable D: the flight speed in 256ths of a pixel per update, projected through the sine table into the X and Y
    /// displacements, as the exact word the cartridge keeps (0 through 65535). A deceleration that overshoots leaves a
    /// word with its sign bit set, which the next decelerating update reads as stopped (<see cref="HasStopped"/>).
    /// </summary>
    public int Speed
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = checked((ushort)value);
    }

    /// <summary>
    /// Whether flight is still below <see cref="MaxSpeed"/>, compared by the sign of the word difference as the cartridge
    /// does; an overshooting update is not clamped.
    /// </summary>
    internal bool IsBelowMaximumSpeed => EnemyMath.IsNegative16(Speed - MaxSpeed);

    /// <summary>Whether a decelerating update stops the flight: the speed or the acceleration is zero or has gone below zero.</summary>
    internal bool HasStopped =>
        Speed == 0 || EnemyMath.IsNegative16(Speed) || Acceleration == 0 || EnemyMath.IsNegative16(Acceleration);

    /// <summary>
    /// An accelerating interval expiry: the acceleration grows by its delta, then the speed grows by the acceleration.
    /// Both are word additions that wrap as the cartridge's do.
    /// </summary>
    internal void Accelerate()
    {
        _slot.VariableB = unchecked((ushort)(_slot.VariableB + _slot.VariableC));
        _slot.VariableD = unchecked((ushort)(_slot.VariableD + _slot.VariableB));
    }

    /// <summary>
    /// A decelerating interval expiry: the acceleration shrinks by its delta, then the speed shrinks by the acceleration.
    /// Both are word subtractions that wrap as the cartridge's do, so an overshoot leaves a word that reads as below zero.
    /// </summary>
    internal void Decelerate()
    {
        _slot.VariableB = unchecked((ushort)(_slot.VariableB - _slot.VariableC));
        _slot.VariableD = unchecked((ushort)(_slot.VariableD - _slot.VariableB));
    }

    /// <summary>Native variable E: wrapping movement-delay counter initialized/reloaded to $0010, with targeting selected when its decrement reaches exactly zero.</summary>
    public NativeCountdown ActivationTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableE);
        internal set => _slot.VariableE = value.RawValue;
    }

    /// <summary>Native variable F: shared acceleration/deceleration interval countdown, decremented on eligible speed-update calls and reloaded from the current phase's reset value on exact-zero expiry.</summary>
    public NativeCountdown AccelerationIntervalTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableF);
        internal set => _slot.VariableF = value.RawValue;
    }

    /// <summary>Native extension offset $00: maximum-speed threshold in 256ths of a pixel per update, 1023 through 2815, selected by population parameter two; prevents further acceleration when reached but does not clamp an overshooting update.</summary>
    public int MaxSpeed
    {
        get => _words.MaxSpeed;
        internal set => _words.MaxSpeed = value;
    }

    /// <summary>Native extension offset $02: latest bearing toward Samus with zero pointing right; compared with the captured flight angle rather than continuously steering it.</summary>
    public SnesAngle AngleToSamus
    {
        get => _words.AngleToSamus;
        internal set => _words.AngleToSamus = value;
    }

    /// <summary>Native extension offset $04: captured movement angle in 256ths of a turn (0 through 255 as the game writes it), zero right and 64 down, set when targeting Samus or replaced from projectile direction by an immune-shot kick. Movement steers by its low byte.</summary>
    public int Angle
    {
        get => _words.Angle;
        internal set => _words.Angle = value;
    }

    /// <summary>Native extension offset $06 flag suppressing repeated no-damage projectile kicks during the shot-reaction cooldown; it does not disable ordinary health processing.</summary>
    public bool ShotReactionDisabled
    {
        get => _words.ShotReactionDisableFlag != 0;
        internal set => _words.ShotReactionDisableFlag = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>Native extension offset $0A: acceleration interval in eligible AI calls, 3 through 15, selected by population parameter one and also copied into the initial live interval timer.</summary>
    public int AccelerationIntervalTimerReset
    {
        get => _words.AccelerationTimerReset;
        internal set => _words.AccelerationTimerReset = value;
    }

    /// <summary>Native extension offset $0C: deceleration interval in eligible AI calls, 1 through 7, selected alongside the acceleration interval; used to reload the shared timer after a deceleration expiry.</summary>
    public int DecelerationIntervalTimerReset
    {
        get => _words.DecelerationTimerReset;
        internal set => _words.DecelerationTimerReset = value;
    }

    /// <summary>Native extension offset $0E: wrapping guard timer set to $0030 by an immune-shot kick and decremented each main-AI call; reaching zero clears the flag and pins the timer to one, while an initial zero first wraps to $FFFF.</summary>
    public NativeCountdown ShotReactionDisableTimer
    {
        get => NativeCountdown.FromRaw(_words.ShotReactionDisableTimer);
        internal set => _words.ShotReactionDisableTimer = value.RawValue;
    }

    /// <summary>Native parallel $7E:8800 health snapshot taken before projectile damage, retained so unchanged health can select the actor's immune-shot kick instead of treating it as ordinary damage.</summary>
    public int PreviousHealth
    {
        get => _words.PreviousHealth;
        internal set => _words.PreviousHealth = value;
    }
}
