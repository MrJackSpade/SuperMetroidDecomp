using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// The state of a Bull. Its speed and acceleration change only through <see cref="Accelerate"/> and
/// <see cref="Decelerate"/>. The shot-reaction guard is inherited from the previous Bull in its
/// place (<see cref="PlaceInheritance{TOccupant}"/>).
/// </summary>
public sealed class BullEnemyState
{
    /// <summary>Starts one Bull with the shot-reaction guard the previous Bull in its place left, or none for the place's first.</summary>
    /// <remarks>
    /// The Bull's setup writes only the speed and interval settings. The guard flag and its countdown are
    /// read by the Bull's first main-AI update, which counts the inherited countdown down: a Bull set up while its
    /// predecessor's 48-update guard was still running ignores no-damage kicks until that guard runs out. The angles
    /// are always written by targeting or a kick before they are read, so they are the Bull's own.
    /// </remarks>
    /// <param name="previous">The place's previous Bull, or null for the place's first in the room.</param>
    /// <param name="action">The action the Bull starts in.</param>
    internal BullEnemyState(BullEnemyState? previous, BullAction action)
    {
        Action = action;
        ShotReactionDisabled = previous?.ShotReactionDisabled ?? false;
        ShotReactionDisableTimer = previous?.ShotReactionDisableTimer ?? 0;
    }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<BullSegment> Playback { get; set; }

    /// <summary>The current delay, target, acceleration, or deceleration step.</summary>
    public BullAction Action { get; internal set; }

    /// <summary>
    /// The speed change applied at each interval expiry. It grows or shrinks by <see cref="AccelerationDelta"/> through
    /// <see cref="Accelerate"/> and <see cref="Decelerate"/>; braking after a kick can take it below zero, which stops the
    /// flight (<see cref="HasStopped"/>).
    /// </summary>
    public SignedEightEight Acceleration { get; internal set; }

    /// <summary>
    /// The change in <see cref="Acceleration"/> at each interval expiry, 24 256ths of a pixel an update while flying and
    /// none once the flight cycle stops.
    /// </summary>
    public SignedEightEight AccelerationDelta { get; internal set; }

    /// <summary>
    /// The flight speed along the heading, in 256ths of a pixel an update: never below zero and never above 2880. A
    /// flight starts from rest and speeds up by 24, 48, 72 ... each interval until it reaches the spawn's top speed, at
    /// most 2815, so it passes that by at most one step, from 2520 to 2880. A braking flight slows by 24 less each interval
    /// than it last sped up by, so its braking runs out before it comes to rest; a kick, six pixels an update slowing by a
    /// pixel an interval at first, stops with its braking spent at 304, or, when the kick finds the Bull waiting with no
    /// braking change, slows a whole pixel each interval to exactly zero.
    /// </summary>
    public SignedEightEight Speed { get; internal set; }

    /// <summary>Whether flight is still below <see cref="MaxSpeed"/>; an overshooting update is not clamped.</summary>
    internal bool IsBelowMaximumSpeed => Speed < MaxSpeed;

    /// <summary>Whether a decelerating update stops the flight: the speed is zero or the acceleration has run out.</summary>
    internal bool HasStopped => Speed.IsZero || Acceleration <= SignedEightEight.Zero;

    /// <summary>An accelerating interval expiry: the acceleration grows by its delta, then the speed grows by the acceleration.</summary>
    internal void Accelerate()
    {
        Acceleration += AccelerationDelta;
        Speed += Acceleration;
    }

    /// <summary>A decelerating interval expiry: the acceleration shrinks by its delta, then the speed shrinks by the acceleration.</summary>
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
    /// Shared acceleration/deceleration interval countdown, decremented on eligible speed updates and reloaded
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
    /// place's previous Bull; it does not disable ordinary health processing.
    /// </summary>
    public bool ShotReactionDisabled { get; internal set; }

    /// <summary>
    /// The main-AI updates left before a no-damage kick's guard runs out, inherited from the place's previous Bull: set to
    /// 48 by the kick and counted down each main-AI update, clearing the guard on the update it reaches zero (#2282). Zero
    /// while no guard counts; the guard is set only with its countdown.
    /// </summary>
    public int ShotReactionDisableTimer
    {
        get;
        internal set => field = Ensure.AtLeastZero(value);
    }
}
