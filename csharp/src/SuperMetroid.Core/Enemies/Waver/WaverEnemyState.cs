using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>The state of one Waver: its horizontal velocity, wave phase and animation selection.</summary>
public sealed class WaverEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<WaverSegment> Playback { get; set; }

    /// <summary>Signed 1/65536-pixel horizontal displacement per update: the Waver's speed toward <see cref="Facing"/> (#2278).</summary>
    public SignedSixteenSixteen XVelocity => Facing == HorizontalDirection.Right ? WaverMotionDefinitions.HorizontalSpeed : -WaverMotionDefinitions.HorizontalSpeed;

    /// <summary>The way the Waver faces: its spawn's, turned around by each wall it meets; a spin keeps it (#2145).</summary>
    public HorizontalDirection Facing { get; internal set; }

    /// <summary>
    /// The vertical wave's phase; only its whole step is read, so it wraps after a full turn. It advances two whole
    /// steps per unobstructed update and a vertical collision reflects it by half a turn.
    /// </summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>Set by the spin animation's last cue after the four-frame spin completes.</summary>
    public bool SpinFinished { get; internal set; }

    /// <summary>Advances the wave phase by its step, wrapping after a full turn.</summary>
    internal void AdvanceWave() => Angle = Angle.TurnedBySteps(WaverMotionDefinitions.WaveStep);

    /// <summary>Reflects the wave phase by half a turn, as a vertical collision does.</summary>
    internal void ReflectWave() => Angle += RotationAngle.HalfTurn;
}
