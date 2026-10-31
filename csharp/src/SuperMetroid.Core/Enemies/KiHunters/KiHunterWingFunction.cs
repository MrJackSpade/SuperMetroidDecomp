namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The secondary routine a detached wing runs.
/// Its values are arbitrary.
/// </summary>
public enum KiHunterWingFunction
{
    /// <summary>Advances a radius-$30 orbit without collision, switching arcs when the angle crosses $C000.</summary>
    Orbit = 1,
    /// <summary>Advances the opposite radius-$30 arc with vertical collision, deleting the wing on impact or switching back across $C000.</summary>
    FallingCollisionArc,
}
