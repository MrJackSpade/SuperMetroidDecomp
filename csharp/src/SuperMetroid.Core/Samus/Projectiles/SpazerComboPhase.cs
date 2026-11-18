namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The phase of one of the Spazer special beam attack's particles. Values are arbitrary.
/// </summary>
public enum SpazerComboPhase
{
    /// <summary>Circling Samus.</summary>
    Circling,

    /// <summary>Flying up towards the convergence point.</summary>
    FlyingUpTowardsPoint,

    /// <summary>Flying up away from the convergence point.</summary>
    FlyingUpAwayFromPoint,

    /// <summary>Falling beside Samus until it leaves the screen; a spent falling particle ends without its partner.</summary>
    Falling,
}
