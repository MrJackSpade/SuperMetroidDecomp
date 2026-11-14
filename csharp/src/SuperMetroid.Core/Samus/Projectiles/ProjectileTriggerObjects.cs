using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Room objects triggered by Samus projectiles reaching their block rather than by contact.</summary>
[RoomLifetime]
internal sealed class ProjectileTriggerObjects
{
    /// <summary>The room-object slots searched from highest index to lowest on a projectile hit.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Binds the active room's room-object population for generic projectile collision reactions.</summary>
    internal ProjectileTriggerObjects(RoomObjectSlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>
    /// Publishes a projectile's hit on a resident-trigger block to the room object at <paramref name="block"/>: the
    /// highest-numbered active slot at that block receives the hit's family, whatever room object it holds. The grapple
    /// beam's touch (<see cref="SamusProjectileType.None"/>) arrives as a beam.
    /// </summary>
    public bool TryNotifyResidentProjectileHit(
        Position16<RoomBlocks> block,
        SamusProjectileType projectileType)
    {
        // The search runs in processing order and accepts the first matching block regardless of its definition. Preserve
        // that generality: reaction room objects and future modelled actors must not be omitted from a family step.
        foreach (RoomObjectSlot slot in _slotTable.InProcessingOrder)
        {
            if (!slot.Active || slot.Block != block)
                continue;

            // An object a shot strikes directly keeps the strike itself; every other place records the hit on its block,
            // which the doors take on their next update.
            if (slot.Behaviour is IShootableRoomObject shootable)
                shootable.Strike(new ResidentObjectProjectileHit(projectileType.Family));
            else
                slot.ProjectileHit = new ResidentObjectProjectileHit(projectileType.Family);
            return true;
        }

        return false;
    }
}
