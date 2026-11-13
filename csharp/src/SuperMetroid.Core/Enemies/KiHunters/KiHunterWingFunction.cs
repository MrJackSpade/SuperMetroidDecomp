namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The secondary behaviour a detached wing runs.
/// Its values are arbitrary.
/// </summary>
public enum KiHunterWingFunction
{
    /// <summary>Advances a radius-48 orbit without collision, switching arcs when the angle crosses three quarters of a turn.</summary>
    Orbit = 1,
    /// <summary>Advances the opposite radius-48 arc with vertical collision, deleting the wing on impact or switching back across three quarters of a turn.</summary>
    FallingCollisionArc,
}
