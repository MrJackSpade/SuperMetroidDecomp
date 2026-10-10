using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

public sealed partial class RoomPlmSystem
{
    /// <summary>Runs $8F:C8D3's hardcoded spawn and $84:B8DC's scroll setup.</summary>
    public bool TrySpawnShaktoolRoomController(RoomScrollGrid scrolls)
    {
        ArgumentNullException.ThrowIfNull(scrolls);
        for (int index = _slots.Length - 1; index >= 0; index--)
        {
            PlmSlot slot = _slots[index];
            if (slot.Active) continue;
            ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = ShaktoolRoomPlmRomData.Header;
            slot.BlockIndex = 0;
            slot.InstructionPointer = ShaktoolRoomPlmRomData.InstructionList;
            slot.InstructionTimer = 1;
            for (int cell = 0; cell < ShaktoolRoomPlmRomData.ScrollCellCount; cell++)
                scrolls.SetStorage(cell, cell == 0 ? RoomScrollState.Blue : RoomScrollState.RedBoundary);
            return true;
        }
        // Native hardcoded spawning leaves the scrolls untouched when its pool is full.
        return false;
    }

    /// <summary>
    /// Applies Shaktool-room scroll updates and, after Samus crosses the cleared-path boundary, records the event and removes the controller PLM.
    /// </summary>
    /// <param name="slot">The PLM slot whose Shaktool pre-instruction is being dispatched.</param>
    /// <param name="scrolls">Room scroll storage used to restore blue scroll settings while a power-bomb explosion is active.</param>
    /// <param name="powerBombExplosionStatus">The complete native explosion status word; any nonzero value triggers the scroll update.</param>
    /// <exception cref="InvalidOperationException">The matching controller has no scroll grid, Samus state, or event callback.</exception>
    private void RunShaktoolRoomPreInstruction(PlmSlot slot, RoomScrollGrid? scrolls,
        ushort powerBombExplosionStatus)
    {
        if (slot.PreInstruction != ShaktoolRoomPlmRomData.PreInstruction) return;
        if (scrolls is null || _collectibleSamus?.Invoke() is not { } samus || _setEvent is null)
            throw new InvalidOperationException("Shaktool room controller requires scroll, Samus, and event owners.");
        // The cartridge tests the entire status word, not just its active high bit.
        // This also admits the pending/time-frozen status value.
        if (powerBombExplosionStatus != 0)
            for (int cell = 0; cell < ShaktoolRoomPlmRomData.ScrollCellCount; cell++)
                scrolls.SetStorage(cell, RoomScrollState.Blue);
        if (samus.XPosition <= ShaktoolRoomPlmRomData.ClearedPathXBoundary) return;
        _setEvent(EventNumber.ShaktoolClearedPath);
        // B8D2 clears only the ID; inactive instruction/timer words remain in RAM.
        slot.HeaderPointer = 0;
        slot.Active = false;
    }
}
