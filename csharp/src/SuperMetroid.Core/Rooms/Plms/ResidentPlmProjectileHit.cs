using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// A projectile hit a resident PLM's trigger block, waiting for the PLM's pre-instruction to consume it. Doors, the
/// n00b tube, Draygon's wall cannons and Mother Brain's glass read only the family that struck them; the grapple
/// beam's touch arrives as a beam.
/// </summary>
/// <param name="Family">The family of the projectile that struck the block.</param>
public readonly record struct ResidentPlmProjectileHit(SamusProjectileFamily Family);
