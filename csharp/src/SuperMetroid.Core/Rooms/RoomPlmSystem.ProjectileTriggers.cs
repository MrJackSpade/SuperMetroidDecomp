using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Cartridge-generic projectile handoff to resident bank-$84 PLMs.</summary>
public sealed partial class RoomPlmSystem
{
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
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (!slot.Active || slot.BlockIndex != blockIndex)
                continue;

            SamusProjectileTypeWord publishedType = new(
                projectileType.AsResidentPlmTriggerWord());
            slot.LoopTimer = publishedType.Raw;

            // These translated actors mirror native PLM_Timers with explicit pending
            // state so their semantic pre-instructions cannot mistake stale timer data
            // for a fresh collision on a later frame.
            if (slot.DraygonCannon is not null)
                slot.DraygonCannon.HasPendingHit = true;
            if (slot.EyeDoor?.Component == EyeDoorComponent.Eye)
                slot.EyeDoor.HasPendingHit = true;
            if (slot.ColoredDoor is not null && slot.ColoredDoor.Phase is not (
                    ColoredDoorPhase.Opening or ColoredDoorPhase.Closing))
            {
                slot.ColoredDoor.PendingProjectileType = publishedType;
                slot.ColoredDoor.HasPendingHit = true;
            }
            if (slot.GreyDoor is not null && slot.GreyDoor.Phase is not (
                    GreyDoorPhase.Opening or
                    GreyDoorPhase.ConvertToBlue or
                    GreyDoorPhase.Closing))
            {
                slot.GreyDoor.PendingProjectileType = publishedType;
                slot.GreyDoor.HasPendingHit = true;
            }

            return true;
        }

        return false;
    }
}
