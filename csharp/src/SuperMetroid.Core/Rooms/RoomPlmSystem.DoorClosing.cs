using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Shared bank-$82 door-transition handoff into the bank-$84 PLM pool.</summary>
public sealed partial class RoomPlmSystem
{
    /// <summary>
    /// Executes <c>$82:E8EB Spawn_Door_Closing_PLM</c> after destination room PLMs and
    /// door/setup code have been constructed.
    /// </summary>
    /// <remarks>
    /// The cartridge first looks for any resident PLM at the door-cap block. If that
    /// actor's nonnegative persistence bit is not already set, its header's second list is
    /// selected in place. Otherwise the twelve-entry direction table may spawn a separate
    /// blue-door or escape-gate closer. Allocation always uses the highest free native
    /// slot, exactly like <c>Spawn_Room_PLM</c>.
    /// </remarks>
    /// <returns>
    /// True when an existing cap was redirected or a fallback closer was spawned; false
    /// for non-closing directions or native pool exhaustion.
    /// </returns>
    public bool TrySpawnDoorClosingPlm(
        ISnesAddressSpace bus,
        RoomLevelData level,
        CartridgeDoorHeader door,
        Bank80SystemState system)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(door);
        ArgumentNullException.ThrowIfNull(system);

        int blockIndex = level.GetBlockIndex(door.PlmX, door.PlmY);
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot resident = _slots[slotIndex];
            if (!resident.Active || resident.BlockIndex != blockIndex)
                continue;

            // `$82:E951-$E963` treats a negative room argument as explicitly
            // nonpersistent. C8CA uses $8000, so the escape entrance always selects its
            // closing list. Ordinary colored caps consult the same opened-door bit array.
            if (unchecked((short)resident.RoomArgument) >= 0 &&
                system.HasOpenedDoorBit(resident.RoomArgument))
            {
                break;
            }

            ushort closingList = ReadBank84Word(
                bus,
                unchecked((ushort)(resident.HeaderPointer + 4)));
            if (closingList < 0x8000)
            {
                throw new InvalidDataException(
                    $"Door-cap PLM $84:{resident.HeaderPointer:X4} at block {blockIndex} " +
                    $"has invalid second instruction list $84:{closingList:X4}.");
            }

            resident.InstructionPointer = closingList;
            resident.InstructionTimer = 1;

            // The native handler has only scalar PLM arrays. These references are C#-only
            // family discriminators; once the shared transition routine replaces the list,
            // the ordinary instruction interpreter must own the actor as it does on cart.
            resident.ColoredDoor = null;
            resident.GreyDoor = null;
            return true;
        }

        ushort fallbackHeader = DoorClosingPlmRomData.GetHeader(door.Orientation);
        if (fallbackHeader == 0)
            return false;

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = fallbackHeader;
            slot.BlockIndex = blockIndex;
            slot.InstructionPointer = ReadBank84Word(
                bus,
                unchecked((ushort)(fallbackHeader + 2)));
            slot.InstructionTimer = 1;
            SetupDoorTransitionDeactivatedSlot(level, slot);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Translates <c>$84:B3C1 Setup_DeactivatePLM</c>: clear collision bits 12-14 while
    /// retaining the block's solid bit and visual index. Despite its historical name, the
    /// routine does not delete or deactivate the PLM slot.
    /// </summary>
    private static void SetupDoorTransitionDeactivatedSlot(RoomLevelData level, PlmSlot slot)
    {
        var levelWord = new RoomLevelWord(
            level.GetCollisionBlockByIndex(slot.BlockIndex).LevelWord);
        RoomCollisionType deactivatedType = levelWord.CollisionTypeValue >=
            (byte)RoomCollisionType.SolidBlock
                ? RoomCollisionType.SolidBlock
                : RoomCollisionType.Air;
        level.SetForegroundEntry(
            slot.BlockIndex,
            (ushort)levelWord.WithCollisionType(deactivatedType));
    }
}
