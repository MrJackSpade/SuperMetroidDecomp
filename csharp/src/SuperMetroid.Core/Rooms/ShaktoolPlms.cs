using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Rooms;

/// <summary>Shaktool's room controller PLM and the pre-instruction that follows Shaktool's digging.</summary>
internal sealed class ShaktoolPlms
{
    /// <summary>Stores bindings.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Stores slot table.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Initializes a new shaktool plms instance.</summary>
    internal ShaktoolPlms(PlmRoomBindings bindings, PlmSlotTable slotTable)
    {
        _bindings = bindings;
        _slotTable = slotTable;
    }
    /// <summary>Runs $8F:C8D3's hardcoded spawn and $84:B8DC's scroll setup.</summary>
    public bool TrySpawnShaktoolRoomController(RoomScrollGrid scrolls)
    {
        Ensure.NotNull(scrolls);
        for (int index = _slotTable._slots.Length - 1; index >= 0; index--)
        {
            PlmSlot slot = _slotTable._slots[index];
            if (slot.Active) continue;
            PlmPopulationLoader.ClearSlot(slot);
            slot.Active = true;
            slot.HeaderPointer = PlmHeaderId.ShaktoolsRoom;
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

    /// <summary>run shaktool room pre instruction.</summary>
    internal void RunShaktoolRoomPreInstruction(PlmSlot slot, RoomScrollGrid? scrolls,
        ushort powerBombExplosionStatus)
    {
        if (slot.PreInstruction != ShaktoolRoomPlmRomData.PreInstruction) return;
        if (scrolls is null || _bindings.Samus?.Current is not { } samus || _bindings.Events is null)
            throw new InvalidOperationException("Shaktool room controller requires scroll, Samus, and event owners.");
        // The cartridge tests the entire status word, not just its active high bit.
        // This also admits the pending/time-frozen status value.
        if (powerBombExplosionStatus != 0)
            for (int cell = 0; cell < ShaktoolRoomPlmRomData.ScrollCellCount; cell++)
                scrolls.SetStorage(cell, RoomScrollState.Blue);
        if (samus.Position.X.Whole <= ShaktoolRoomPlmRomData.ClearedPathXBoundary) return;
        _bindings.Events.Set(EventNumber.ShaktoolClearedPath);
        // B8D2 clears only the ID; inactive instruction/timer words remain in RAM.
        slot.HeaderPointer = 0;
        slot.Active = false;
    }
}
