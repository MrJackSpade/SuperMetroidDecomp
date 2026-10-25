using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Shaktool's room controller PLM and the pre-instruction that follows Shaktool's digging.</summary>
[RoomLifetime]
internal sealed class ShaktoolPlms
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>Physical PLM slots scanned from highest index for the hardcoded spawn.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Connects Shaktool's room controller to the resident PLM slots and room owners.</summary>
    internal ShaktoolPlms(IRoomSamusAccess? samus, IGlobalEventFlags? events, PlmSlotTable slotTable)
    {
        _samus = samus;
        _events = events;
        _slotTable = slotTable;
    }
    /// <summary>Runs $8F:C8D3's hardcoded spawn and $84:B8DC's scroll setup.</summary>
    public bool TrySpawnShaktoolRoomController(RoomScrollGrid scrolls)
    {
        Ensure.NotNull(scrolls);
        // Native hardcoded spawning leaves the scrolls untouched when its pool is full.
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = PlmHeaderId.ShaktoolsRoom;
        slot.BlockIndex = 0;
        slot.StartAnimation(new ScriptedRoomPlmAnimation(ScriptedRoomPlmProgram.ShaktoolRoomController));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        for (int cell = 0; cell < ShaktoolRoomDefinitions.ScrollCellCount; cell++)
            scrolls.SetStorage(cell, cell == 0 ? RoomScrollState.Blue : RoomScrollState.RedBoundary);
        return true;
    }

    /// <summary>Opens all scroll cells during a power bomb and retires the controller after Samus passes the cleared-path boundary.</summary>
    /// <remarks>The full explosion status word admits a pending or time-frozen blast; retiring clears the PLM ID but leaves its instruction words in RAM.</remarks>
    internal void RunShaktoolRoomPreInstruction(PlmSlot slot, RoomScrollGrid? scrolls,
        int powerBombExplosionStatus)
    {
        if (scrolls is null || _samus?.Current is not { } samus || _events is null)
            throw new InvalidOperationException("Shaktool room controller requires scroll, Samus, and event owners.");
        // The cartridge tests the entire status word, not just its active high bit.
        // This also admits the pending/time-frozen status value.
        if (powerBombExplosionStatus != 0)
            for (int cell = 0; cell < ShaktoolRoomDefinitions.ScrollCellCount; cell++)
                scrolls.SetStorage(cell, RoomScrollState.Blue);
        if (samus.Position.X.Whole <= ShaktoolRoomDefinitions.ClearedPathXBoundary) return;
        _events.Set(EventNumber.ShaktoolClearedPath);
        // B8D2 clears only the ID; inactive instruction/timer words remain in RAM.
        slot.Header = 0;
        slot.Delete();
    }
}
