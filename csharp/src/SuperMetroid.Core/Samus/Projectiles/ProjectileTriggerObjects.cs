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
    /// active place at that block in update order takes the family the strike lands as, whatever room object it holds; the
    /// grapple beam's touch (<see cref="BlockStrike.GrappleTouch"/>) lands as a beam.
    /// </summary>
    /// <remarks>
    /// A hit on a block no active place lies at changes nothing (#2553): a resident-trigger block can outlive its object, as
    /// the n00b tube's setup writes its origin before the already-broken tube deletes itself. The game's generic trigger
    /// ($84:C7E2) then finds no object and leaves only the carry of its last compare, against the first room-object slot's
    /// stored block, and every caller discards that carry: the shot, bomb and grapple reactions ($94:9E55-9EA5,
    /// $94:9D71-9DA3) set the shootable block's collision and clear shootable air's after the spawn whatever the setup
    /// returned, and the trigger deletes itself ($84:C840). The block's own collision decides whether the strike stops, as
    /// it does with an object there.
    /// </remarks>
    public void NotifyResidentProjectileHit(
        Position16<RoomBlocks> block,
        BlockStrike strike)
    {
        Ensure.NotNull(strike);
        foreach (RoomObjectPlace place in _places.InUpdateOrder)
        {
            if (!place.Active || place.Block != block)
                continue;

            // An object a shot can strike keeps the strike until its next update; the hit means nothing to any other
            // object, which takes it all the same, so a place later in the order never sees it.
            if (place.Behaviour is IShootableRoomObject shootable)
                shootable.Strike(new ResidentObjectProjectileHit(strike.Family));
            return;
        }
    }
}
