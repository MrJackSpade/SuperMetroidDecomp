namespace SuperMetroid.Core.Progression;

/// <summary>
/// The beam projectiles that are neither a fired shot nor a special-attack particle: the trail and the echo a special
/// attack or a drained Shinespark crash leaves, each drawn and damaging as the beam it names.
/// </summary>
public enum SamusBeamEcho
{
    /// <summary>The trailing pair of the Spazer special attack; it damages as a Spazer beam.</summary>
    SpazerComboTrail,
    /// <summary>An echo a drained Shinespark crash leaves; it damages as a Plasma-Wave beam.</summary>
    ShinesparkEcho,
}
