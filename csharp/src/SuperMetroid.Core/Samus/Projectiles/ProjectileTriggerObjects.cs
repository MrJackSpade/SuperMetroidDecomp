using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Room objects triggered by Samus projectiles reaching their block rather than by contact.</summary>
[RoomLifetime]
internal sealed class ProjectileTriggerObjects
{
    /// <summary>The room-object places, searched from the last in update order back to the first on a projectile hit.</summary>
    private readonly RoomObjectPlaceList _places;

    /// <summary>Binds the active room's room-object population for generic projectile collision reactions.</summary>
    internal ProjectileTriggerObjects(RoomObjectPlaceList places)
    {
        _places = places;
    }

    /// <summary>
    /// Publishes a projectile's hit on a resident-trigger block to the room object at <paramref name="block"/>: the
    /// highest-numbered active place at that block receives the hit's family, whatever room object it holds. The grapple
    /// beam's touch (<see cref="SamusProjectileType.None"/>) arrives as a beam.
    /// </summary>
    public bool TryNotifyResidentProjectileHit(
        Position16<RoomBlocks> block,
        SamusProjectileType projectileType)
    {
        // The first active place at the block the search reaches takes the hit, whatever room object it holds.
        foreach (RoomObjectPlace place in _places.InUpdateOrder)
        {
            if (!place.Active || place.Block != block)
                continue;

            // An object a shot strikes directly keeps the strike itself; every other place records the hit on its block,
            // which the doors take on their next update.
            if (place.Behaviour is IShootableRoomObject shootable)
                shootable.Strike(new ResidentObjectProjectileHit(projectileType.Family));
            else
                place.ProjectileHit = new ResidentObjectProjectileHit(projectileType.Family);
            return true;
        }

        return false;
    }
}
