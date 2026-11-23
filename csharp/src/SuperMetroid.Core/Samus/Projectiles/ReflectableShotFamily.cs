namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The shot families an enemy reflects (#2553): a beam, a missile or a Super Missile. A reflection carries one, so no
/// bomb or explosion reaches a reflection.
/// </summary>
public enum ReflectableShotFamily
{
    /// <summary>A beam shot.</summary>
    Beam,

    /// <summary>A missile.</summary>
    Missile,

    /// <summary>A Super Missile.</summary>
    SuperMissile,
}
