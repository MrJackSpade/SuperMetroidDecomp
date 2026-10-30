namespace SuperMetroid.Core.Progression;

/// <summary>
/// The beam projectiles that are neither a fired shot nor a special-attack particle, but echoes drawn and damaged from
/// the echo-and-trail projectile table.
/// </summary>
public enum SamusBeamEcho
{
    /// <summary>The trailing pair of the Spazer special attack; it damages as a Spazer beam.</summary>
    SpazerComboTrail,
    /// <summary>An echo a drained Shinespark crash leaves; it damages as a Plasma-Wave beam.</summary>
    ShinesparkEcho,
}
