namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The phase of one of the Spazer special beam attack's particles. Values are arbitrary.
/// </summary>
public enum SpazerComboPhase
{
    /// <summary>Circling Samus; also the phase of a slot no particle has used.</summary>
    Circling,

    /// <summary>Flying up towards the convergence point.</summary>
    FlyingUpTowardsPoint,

    /// <summary>Flying up away from the convergence point.</summary>
    FlyingUpAwayFromPoint,
}
