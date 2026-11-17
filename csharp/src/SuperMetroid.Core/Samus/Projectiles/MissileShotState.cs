namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A missile's ignition (#2346), created when the missile is fired or reflected; a Super Missile and its link keep theirs
/// with the link they name (<see cref="SuperMissileShotState"/>).
/// </summary>
/// <param name="ignition">Where the missile starts in its ignition: pending when fired, pending after a reflection when reflected.</param>
internal class MissileShotState(SamusMissileIgnition ignition) : SamusShotKindState
{
    /// <summary>Where the missile is in its ignition.</summary>
    internal SamusMissileIgnition Ignition { get; set; } = ignition;
}
