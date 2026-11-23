namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The secondary behaviour a detached wing runs.
/// Its values are arbitrary.
/// </summary>
public enum KiHunterWingMovement
{
    /// <summary>Advances the detached wings' orbit (<see cref="KiHunterMotionDefinitions.DetachedWingRadius"/>) without collision, switching arcs when the angle crosses three quarters of a turn.</summary>
    Orbit,
    /// <summary>Advances the opposite arc of the same radius with vertical collision, deleting the wing on impact or switching back across three quarters of a turn.</summary>
    FallingCollisionArc,
}
