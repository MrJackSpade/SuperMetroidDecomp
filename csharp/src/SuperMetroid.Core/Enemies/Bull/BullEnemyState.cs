using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// The state of a Bull. Speed and acceleration stay the exact values the cartridge keeps, because a deceleration that
/// overshoots leaves a wrapped value that the next update reads by its sign; they change only through
/// <see cref="Accelerate"/> and <see cref="Decelerate"/>. The values the cartridge keeps in the enemy's common record are
/// emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>). The shot-reaction guard is inherited from
/// the previous Bull in its slot (<see cref="SlotInheritance{TOccupant}"/>).
/// </summary>
public sealed class BullEnemyState
{
    /// <summary>Starts one Bull with the shot-reaction guard the previous Bull in its slot left, or none for the slot's first.</summary>
    /// <remarks>
    /// Setup (<c>InitAI_Bull</c>) writes only the speed and interval settings. The guard flag and its countdown are
    /// read by the Bull's first main-AI update, which counts the inherited countdown down: a Bull set up while its
    /// predecessor's 48-update guard was still running ignores no-damage kicks until that guard runs out. The angles
    /// are always written by targeting or a kick before they are read, so they are the Bull's own.
    /// </remarks>
    /// <param name="previous">The slot's previous Bull, or null for the slot's first in the room.</param>
    internal BullEnemyState(BullEnemyState? previous)
    {
        ShotReactionDisabled = previous?.ShotReactionDisabled ?? false;
        ShotReactionDisableTimer = previous?.ShotReactionDisableTimer ?? WrappingCountdown.Start(0);
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
    public WrappingCountdown ActivationTimer { get; internal set; }

    /// <summary>
    /// Shared acceleration/deceleration interval countdown, decremented on eligible speed-update calls and reloaded
    /// from the current phase's reset value on exact-zero expiry.
    /// </summary>
    public WrappingCountdown AccelerationIntervalTimer { get; internal set; }

    /// <summary>Maximum-speed threshold in 256ths of a pixel per update, 1023 through 2815, selected by the spawn's speed setting; prevents further acceleration when reached but does not clamp an overshooting update.</summary>
    public int MaxSpeed { get; internal set; }

    /// <summary>Latest bearing toward Samus with zero pointing right; compared with the captured flight angle rather than continuously steering it.</summary>
    public RotationAngle AngleToSamus { get; internal set; }

    /// <summary>Captured movement angle in 256ths of a turn (0 through 255 as the game writes it), zero right and 64 down, set when targeting Samus or replaced from projectile direction by an immune-shot kick. Movement steers by its low eight bits.</summary>
    public int Angle { get; internal set; }

    /// <summary>
    /// Whether repeated no-damage projectile kicks are suppressed during the shot-reaction cooldown, inherited from the
    /// slot's previous Bull; it does not disable ordinary health processing.
    /// </summary>
    public bool ShotReactionDisabled { get; internal set; }

    /// <summary>Acceleration interval in eligible AI calls, 3 through 15, selected by the spawn's interval setting and also copied into the initial live interval timer.</summary>
    public int AccelerationIntervalTimerReset { get; internal set; }

    /// <summary>Deceleration interval in eligible AI calls, 1 through 7, selected alongside the acceleration interval; used to reload the shared timer after a deceleration expiry.</summary>
    public int DecelerationIntervalTimerReset { get; internal set; }

    /// <summary>
    /// Wrapping guard countdown, inherited from the slot's previous Bull, set to 48 by an immune-shot kick and
    /// decremented each main-AI call; reaching zero clears the guard and pins the countdown at one, while a countdown
    /// starting at zero first wraps to its largest count.
    /// </summary>
    public WrappingCountdown ShotReactionDisableTimer { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        Acceleration = 0;
        AccelerationDelta = 0;
        Speed = 0;
        ActivationTimer = WrappingCountdown.Start(0);
        AccelerationIntervalTimer = WrappingCountdown.Start(0);
    }
}
