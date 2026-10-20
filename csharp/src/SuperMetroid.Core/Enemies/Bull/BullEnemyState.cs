using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// The state of a Bull, with its per-slot extension words. Speed and acceleration stay the exact words the cartridge
/// keeps, because a deceleration that overshoots leaves a wrapped word that the next update reads by its sign; they
/// change only through <see cref="Accelerate"/> and <see cref="Decelerate"/>. The values the cartridge keeps in the
/// enemy's common record are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class BullEnemyState
{
    /// <summary>The slot's words that outlive the occupant.</summary>
    private readonly BullExtensionWords _words;

    /// <summary>Creates the state over one Bull slot's extension words.</summary>
    internal BullEnemyState(BullExtensionWords words)
    {
        _words = words;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<BullProgram> Program { get; set; }

    /// <summary>
    /// The current bank-$A8 delay, target, acceleration, or deceleration dispatcher pointer; none once a kill has
    /// emptied the record.
    /// </summary>
    public BullEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// The speed change applied at each interval expiry, in 256ths of a pixel per update, as the exact word the
    /// cartridge keeps (0 through 65535). It grows or shrinks by <see cref="AccelerationDelta"/> through <see
    /// cref="Accelerate"/> and <see cref="Decelerate"/>; a word with its sign bit set reads as below zero.
    /// </summary>
    public int Acceleration
    {
        get;
        internal set => field = checked((ushort)value);
    }

    /// <summary>
    /// The change in <see cref="Acceleration"/> at each interval expiry, 24 (a 256th of a pixel per update each) while
    /// flying and zero once the flight cycle stops.
    /// </summary>
    public int AccelerationDelta
    {
        get;
        internal set => field = checked((ushort)value);
    }

    /// <summary>
    /// The flight speed in 256ths of a pixel per update, projected through the sine table into the X and Y
    /// displacements, as the exact word the cartridge keeps (0 through 65535). A deceleration that overshoots leaves a
    /// word with its sign bit set, which the next decelerating update reads as stopped (<see cref="HasStopped"/>).
    /// </summary>
    public int Speed
    {
        get;
        internal set => field = checked((ushort)value);
    }

    /// <summary>
    /// Whether flight is still below <see cref="MaxSpeed"/>, compared by the sign of the word difference as the
    /// cartridge does; an overshooting update is not clamped.
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
        Acceleration = unchecked((ushort)(Acceleration + AccelerationDelta));
        Speed = unchecked((ushort)(Speed + Acceleration));
    }

    /// <summary>
    /// A decelerating interval expiry: the acceleration shrinks by its delta, then the speed shrinks by the
    /// acceleration. Both are word subtractions that wrap as the cartridge's do, so an overshoot leaves a word that
    /// reads as below zero.
    /// </summary>
    internal void Decelerate()
    {
        Acceleration = unchecked((ushort)(Acceleration - AccelerationDelta));
        Speed = unchecked((ushort)(Speed - Acceleration));
    }

    /// <summary>
    /// Wrapping movement-delay counter initialized/reloaded to $0010, with targeting selected when its decrement
    /// reaches exactly zero.
    /// </summary>
    public NativeCountdown ActivationTimer { get; internal set; }

    /// <summary>
    /// Shared acceleration/deceleration interval countdown, decremented on eligible speed-update calls and reloaded
    /// from the current phase's reset value on exact-zero expiry.
    /// </summary>
    public NativeCountdown AccelerationIntervalTimer { get; internal set; }

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

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        Acceleration = 0;
        AccelerationDelta = 0;
        Speed = 0;
        ActivationTimer = NativeCountdown.Start(0);
        AccelerationIntervalTimer = NativeCountdown.Start(0);
    }
}
