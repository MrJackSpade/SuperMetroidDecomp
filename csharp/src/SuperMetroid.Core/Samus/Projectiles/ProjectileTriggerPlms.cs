using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>PLMs triggered by Samus projectiles reaching their block rather than by contact.</summary>
internal sealed class ProjectileTriggerPlms
{
    /// <summary>Physical PLM slots searched from highest index to lowest on a projectile hit.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Binds the active room's PLM population for generic projectile collision dispatch.</summary>
    internal ProjectileTriggerPlms(PlmSlotTable slotTable)
    {
        _slotTable = slotTable;
    }
    /// <summary>
    /// Models setup <c>$84:C7E2</c> for the generic BTS-$44 collision trigger. The
    /// temporary trigger searches for the resident PLM at <paramref name="blockIndex"/>
    /// and publishes the projectile family to that actor; both type-$8 special collision
    /// and type-$C shootable collision use this same cartridge route.
    /// </summary>
    public bool TryNotifyResidentProjectileHit(
        int blockIndex,
        SamusProjectileTypeWord projectileType)
    {
        // Native searches physical slots from $4E down to zero and accepts the first
        // matching block regardless of its header. Preserve that generality: reaction
        // PLMs and future translated actors must not be omitted from a family dispatcher.
        for (int slotIndex = _slotTable._slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slotTable._slots[slotIndex];
            if (!slot.Active || slot.BlockIndex != blockIndex)
                continue;

            SamusProjectileTypeWord publishedType = new(
                projectileType.AsResidentPlmTriggerWord());
            slot.LoopTimer = publishedType.Raw;

            // These translated actors mirror native PLM_Timers with explicit pending
            // state so their semantic pre-instructions cannot mistake stale timer data
            // for a fresh collision on a later frame.
            if (slot.Family is DraygonCannonPlms.DraygonCannonPlmState cannon)
                cannon.HasPendingHit = true;
            if (slot.Family is EyeDoorPlms.EyeDoorPlmState { Component: EyeDoorComponent.Eye } eye)
                eye.HasPendingHit = true;

            return true;
        }

        return false;
    }
}
