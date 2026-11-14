using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>The state of one Waver: its horizontal velocity, wave phase and animation selection.</summary>
public sealed class WaverEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<WaverSegment> Playback { get; set; }

    /// <summary>Signed 1/65536-pixel horizontal displacement per update, one and a half pixels either way; a wall negates it.</summary>
    public SignedSixteenSixteen XVelocity { get; internal set; }

    /// <summary>The animation currently installed; a request for it installs nothing.</summary>
    internal WaverSegment InstalledAnimation { get; set; }

    /// <summary>
    /// The vertical wave's phase; only its whole step is read, so it wraps after a full turn. It advances two whole
    /// steps per unobstructed update and a vertical collision reflects it by half a turn.
    /// </summary>
    public RotationAngle Angle { get; internal set; }

    /// <summary>Set by the spin animation's last cue after the four-frame spin completes.</summary>
    public bool SpinFinished { get; internal set; }

    /// <summary>The animation main AI asks for: steady or spinning, facing left or right.</summary>
    internal WaverSegment RequestedAnimation { get; set; }

    /// <summary>Advances the wave phase two table units, wrapping after a full turn.</summary>
    internal void AdvanceWave() => Angle = Angle.TurnedBySteps(2);

    /// <summary>Reflects the wave phase by half a turn, as a vertical collision does.</summary>
    internal void ReflectWave() => Angle = Angle.TurnedBySteps(EnemyTrigonometryAngleDefinitions.HalfTurn);
}
