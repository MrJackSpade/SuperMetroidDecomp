using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Bull;

/// <summary>
/// The state of a Bull. Its speed and acceleration change only through <see cref="Accelerate"/> and
/// <see cref="Decelerate"/>. The shot-reaction guard is inherited from the previous Bull in its
/// place (<see cref="PlaceInheritance{TOccupant}"/>).
/// </summary>
public sealed class BullEnemyState
{
    /// <summary>What the enemy's population entry told its species, which it starts from and reads for its life (#2554).</summary>
    internal BullSpawnSettings Settings { get; }

    /// <summary>Starts one Bull with the shot-reaction guard the previous Bull in its place left, or none for the place's first.</summary>
    /// <remarks>
    /// The Bull's setup writes only the speed and interval settings. The guard's countdown is
    /// read by the Bull's first main-AI update, which counts the inherited countdown down: a Bull set up while its
    /// predecessor's guard (<see cref="BullMovementDefinitions.ShotReactionGuardDuration"/>) was still running ignores no-damage kicks until that guard runs out. The angles
    /// are always written by targeting or a kick before they are read, so they are the Bull's own.
    /// </remarks>
    /// <param name="previous">The place's previous Bull, or null for the place's first in the room.</param>
    /// <param name="action">The action the Bull starts in.</param>
    /// <param name="settings">What the enemy's population entry told its species.</param>
    internal BullEnemyState(BullEnemyState? previous, BullAction action, BullSpawnSettings settings)
    {
        Settings = Ensure.NotNull(settings);
        Action = action;
        ShotReactionDisableTimer = UpdateCountdown.For(previous?.ShotReactionDisableTimer.UpdatesLeft ?? 0);
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
    internal SignedEightEight Acceleration { private get; set; }

    /// <summary>
    /// The change in <see cref="Acceleration"/> at each interval expiry, <see cref="BullMovementDefinitions.AccelerationStep"/>
    /// while flying and none once the flight cycle stops.
    /// </summary>
    public SignedEightEight AccelerationDelta { get; internal set; }

    /// <summary>
    /// The flight speed along the heading: never below zero and never more than one acceleration step past the spawn's
    /// top speed. A flight starts from rest and speeds up by one, two, three and more acceleration steps
    /// (<see cref="BullMovementDefinitions.AccelerationStep"/>) each interval until it reaches the spawn's top speed, so
    /// it passes that by at most one step. A braking flight slows by one acceleration step less each interval than it last
    /// sped up by, so its braking runs out before it comes to rest; a kick, slowing by a pixel an interval at first, stops
    /// with its braking spent short of rest, or, when the kick finds the Bull waiting with no braking change, slows a whole
    /// pixel each interval to exactly zero.
    /// </summary>
    public SignedEightEight Speed { get; internal set; }

    /// <summary>
    /// Whether flight is still below <paramref name="maximumSpeed"/>, its spawn settings' top speed, from a 256th of a
    /// pixel short of four to a 256th short of eleven pixels an update; an overshooting update is not clamped.
    /// </summary>
    internal bool IsBelow(SignedEightEight maximumSpeed) => Speed < maximumSpeed;

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
    /// Movement-delay countdown initialized/reloaded to <see cref="BullMovementDefinitions.MovementDelay"/>, with targeting selected when its decrement
    /// reaches exactly zero.
    /// </summary>
    public UpdateCountdown ActivationTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>
    /// Shared acceleration/deceleration interval countdown, decremented on eligible speed updates and reloaded
    /// from the current phase's reset value on exact-zero expiry.
    /// </summary>
    public UpdateCountdown AccelerationIntervalTimer { get; internal set; } = UpdateCountdown.Ended();

    /// <summary>Latest bearing toward Samus with zero pointing right; compared with the captured flight angle rather than continuously steering it.</summary>
    public RotationAngle AngleToSamus { get; internal set; }

    /// <summary>Captured movement heading, zero right and a quarter turn down, set when targeting Samus or replaced from projectile direction by an immune-shot kick.</summary>
    public TrigonometryAngle Angle { get; internal set; }

    /// <summary>
    /// Whether repeated no-damage projectile kicks are suppressed during the shot-reaction cooldown: while its countdown
    /// runs (#2432). It does not disable ordinary health processing.
    /// </summary>
    public bool ShotReactionDisabled => ShotReactionDisableTimer.IsRunning;

    /// <summary>
    /// The no-damage kick's guard, inherited from the place's previous Bull: started for its duration (<see cref="BullMovementDefinitions.ShotReactionGuardDuration"/>) by the kick and
    /// counted each main-AI update, the guard clearing on the update it ends (#2282). Over while no guard counts.
    /// </summary>
    public UpdateCountdown ShotReactionDisableTimer { get; internal set; }
}
