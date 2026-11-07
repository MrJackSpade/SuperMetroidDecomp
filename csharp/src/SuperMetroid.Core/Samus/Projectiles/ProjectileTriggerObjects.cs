using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>PLMs triggered by Samus projectiles reaching their block rather than by contact.</summary>
internal sealed class ProjectileTriggerObjects
{
    /// <summary>Physical PLM slots searched from highest index to lowest on a projectile hit.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Binds the active room's PLM population for generic projectile collision dispatch.</summary>
    internal ProjectileTriggerObjects(RoomObjectSlotTable slotTable)
    {
        _slotTable = slotTable;
    }
    /// <summary>
    /// Publishes a projectile's hit on a resident-trigger block to the PLM at <paramref name="blockIndex"/>: the
    /// highest-numbered active slot at that block receives the hit's family, whatever PLM it holds. The grapple
    /// beam's touch (<see cref="SamusProjectileType.None"/>) arrives as a beam.
    /// </summary>
    public bool TryNotifyResidentProjectileHit(
        int blockIndex,
        SamusProjectileType projectileType)
    {
        // The search runs in processing order and accepts the first matching block regardless of its header. Preserve
        // that generality: reaction PLMs and future translated actors must not be omitted from a family dispatcher.
        foreach (RoomObjectSlot slot in _slotTable.InProcessingOrder)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex)
                continue;

            slot.ProjectileHit = new ResidentObjectProjectileHit(projectileType.Family);

            // These actors also keep their own pending flag, which their pre-instructions
            // clear only when they accept the hit.
            if (slot.Family is IShotTriggeredObject triggered)
                triggered.RecordShot();

            return true;
        }

        return false;
    }
}
