using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// The state of a Bull. Speed and acceleration are wrapping 8.8 speeds, because a deceleration that overshoots leaves a
/// value past zero that the next update reads as below zero; they change only through <see cref="Accelerate"/> and
/// <see cref="Decelerate"/>. The shot-reaction guard is inherited from the previous Bull in its
/// slot (<see cref="SlotInheritance{TOccupant}"/>).
/// </summary>
public sealed class BullEnemyState
{
    /// <summary>Starts one Bull with the shot-reaction guard the previous Bull in its slot left, or none for the slot's first.</summary>
    /// <remarks>
    /// The Bull's setup writes only the speed and interval settings. The guard flag and its countdown are
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

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<BullSegment> Playback { get; set; }

    /// <summary>The current delay, target, acceleration, or deceleration step.</summary>
    public BullEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// The speed change applied at each interval expiry. It grows or shrinks by <see cref="AccelerationDelta"/> through
    /// <see cref="Accelerate"/> and <see cref="Decelerate"/>, wrapping as the game's arithmetic does, so a change taken
    /// past zero reads as below zero.
    /// </summary>
    public SignedEightEight Acceleration { get; internal set; }

    /// <summary>
    /// The change in <see cref="Acceleration"/> at each interval expiry, 24 256ths of a pixel an update while flying and
    /// none once the flight cycle stops.
    /// </summary>
    public SignedEightEight AccelerationDelta { get; internal set; }

    /// <summary>
    /// The flight speed, projected through the sine table into the X and Y displacements. It changes only by wrapping
    /// steps, so a deceleration that overshoots leaves a speed below zero, which the next decelerating update reads as
    /// stopped (<see cref="HasStopped"/>) before the Bull moves again.
    /// </summary>
    public SignedEightEight Speed { get; internal set; }

    /// <summary>
    /// Whether flight is still below <see cref="MaxSpeed"/>, read from the sign of the wrapping difference as the game
    /// does; an overshooting update is not clamped.
    /// </summary>
    internal bool IsBelowMaximumSpeed => (Speed - MaxSpeed).IsNegative;

    /// <summary>Whether a decelerating update stops the flight: the speed or the acceleration is zero or has gone below zero.</summary>
    internal bool HasStopped =>
        Speed.IsZero || Speed.IsNegative || Acceleration.IsZero || Acceleration.IsNegative;

    /// <summary>
    /// An accelerating interval expiry: the acceleration grows by its delta, then the speed grows by the acceleration,
    /// each sum wrapping as the game's does.
    /// </summary>
    internal void Accelerate()
    {
        Acceleration += AccelerationDelta;
        Speed += Acceleration;
    }

    /// <summary>
    /// A decelerating interval expiry: the acceleration shrinks by its delta, then the speed shrinks by the
    /// acceleration, each difference wrapping as the game's does, so an overshoot leaves a value below zero.
    /// </summary>
    internal void Decelerate()
    {
        Acceleration -= AccelerationDelta;
        Speed -= Acceleration;
    }

    /// <summary>
    /// Movement-delay countdown initialized/reloaded to 16, with targeting selected when its decrement
    /// reaches exactly zero.
    /// </summary>
    public int ActivationTimer { get; internal set; }

    /// <summary>
    /// Shared acceleration/deceleration interval countdown, decremented on eligible speed-update calls and reloaded
    /// from the current phase's reset value on exact-zero expiry.
    /// </summary>
    public int AccelerationIntervalTimer { get; internal set; }

    /// <summary>Maximum-speed threshold, 1023 through 2815 256ths of a pixel an update, selected by the spawn's speed setting; prevents further acceleration when reached but does not clamp an overshooting update.</summary>
    public SignedEightEight MaxSpeed { get; internal set; }

    /// <summary>Latest bearing toward Samus with zero pointing right; compared with the captured flight angle rather than continuously steering it.</summary>
    public RotationAngle AngleToSamus { get; internal set; }

    /// <summary>Captured movement heading, zero right and a quarter turn down, set when targeting Samus or replaced from projectile direction by an immune-shot kick.</summary>
    public TrigonometryAngle Angle { get; internal set; }

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
}
