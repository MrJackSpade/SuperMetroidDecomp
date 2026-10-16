using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Shaktool's room controller PLM and the pre-instruction that follows Shaktool's digging.</summary>
internal sealed class ShaktoolPlms
{
    /// <summary>Room owners needed by the controller's event and Samus-position checks.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Physical PLM slots scanned from highest index for the hardcoded spawn.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Connects Shaktool's room controller to the resident PLM slots and room owners.</summary>
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
            slot.Clear();
            slot.Active = true;
            slot.HeaderPointer = PlmHeaderId.ShaktoolsRoom;
            slot.BlockIndex = 0;
            slot.InstructionPointer = ShaktoolRoomPlmRomData.InstructionList;
            slot.InstructionTimer = NativeCountdown.Start(1);
            for (int cell = 0; cell < ShaktoolRoomPlmRomData.ScrollCellCount; cell++)
                scrolls.SetStorage(cell, cell == 0 ? RoomScrollState.Blue : RoomScrollState.RedBoundary);
            return true;
        }
        // Native hardcoded spawning leaves the scrolls untouched when its pool is full.
        return false;
    }

    /// <summary>Opens all scroll cells during a power bomb and retires the controller after Samus passes the cleared-path boundary.</summary>
    /// <remarks>The full explosion status word admits a pending or time-frozen blast; retiring clears the PLM ID but leaves its instruction words in RAM.</remarks>
    internal void RunShaktoolRoomPreInstruction(PlmSlot slot, RoomScrollGrid? scrolls,
        ushort powerBombExplosionStatus)
    {
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
