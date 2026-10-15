namespace SuperMetroid.Core.Enemies.KiHunters;

/// <summary>The secondary function stored in detached-wing variable zero.</summary>
public enum KiHunterWingFunction : ushort
{
    /// <summary><c>$A8:F7DB Function_KihunterWings_Falling_DriftingLeft</c>: advances a radius-$30 orbit without collision, switching arcs when the angle crosses $C000.</summary>
    Orbit = 0xf7db,
    /// <summary><c>$A8:F8AD Function_KihunterWings_Falling_DriftingRight</c>: advances the opposite radius-$30 arc with vertical collision, deleting the wing on impact or switching back across $C000.</summary>
    FallingCollisionArc = 0xf8ad,
}
