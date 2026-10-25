namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>
/// The secondary routine a detached wing runs.
/// Its values are arbitrary; verification's native view names each member's cartridge routine (#1412).
/// </summary>
public enum KiHunterWingFunction : ushort
{
    /// <summary><c>$A8:F7DB Function_KihunterWings_Falling_DriftingLeft</c>: advances a radius-$30 orbit without collision, switching arcs when the angle crosses $C000.</summary>
    Orbit = 1,
    /// <summary><c>$A8:F8AD Function_KihunterWings_Falling_DriftingRight</c>: advances the opposite radius-$30 arc with vertical collision, deleting the wing on impact or switching back across $C000.</summary>
    FallingCollisionArc,
}
