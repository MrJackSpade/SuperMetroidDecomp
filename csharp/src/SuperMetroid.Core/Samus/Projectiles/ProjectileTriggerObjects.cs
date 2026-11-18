using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Room objects triggered by Samus projectiles reaching their block rather than by contact.</summary>
[RoomLifetime]
internal sealed class ProjectileTriggerObjects
{
    /// <summary>The room-object places, searched in update order on a projectile hit.</summary>
    private readonly RoomObjectPlaceList _places;

    /// <summary>Binds the active room's room-object population for generic projectile collision reactions.</summary>
    internal ProjectileTriggerObjects(RoomObjectPlaceList places)
    {
        _places = places;
    }

    /// <summary>
    /// Publishes a projectile's hit on a resident-trigger block to the room object at <paramref name="block"/>: the first
    /// active place at that block in update order takes the hit's family, whatever room object it holds. The grapple
    /// beam's touch (<see cref="SamusProjectileType.None"/>) arrives as a beam.
    /// </summary>
    /// <returns>Whether an active place lies at <paramref name="block"/>.</returns>
    public bool TryNotifyResidentProjectileHit(
        Position16<RoomBlocks> block,
        SamusProjectileType projectileType)
    {
        foreach (RoomObjectPlace place in _places.InUpdateOrder)
        {
            if (!place.Active || place.Block != block)
                continue;

            // An object a shot can strike keeps the strike until its next update; the hit means nothing to any other
            // object, which takes it all the same, so a place later in the order never sees it.
            if (place.Behaviour is IShootableRoomObject shootable)
                shootable.Strike(new ResidentObjectProjectileHit(projectileType.Family));
            return true;
        }

        return false;
    }
}
