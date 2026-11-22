using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// A projectile hit a resident room object's trigger block. Doors, the n00b tube, Draygon's wall cannons and Mother Brain's
/// glass read only the family that struck them; the grapple beam's touch arrives as a beam.
/// </summary>
/// <param name="Family">The family of the projectile that struck the block.</param>
public readonly partial record struct ResidentObjectProjectileHit(SamusProjectileFamily Family);
