using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Shaktool's room controller PLM and the pre-instruction that follows Shaktool's digging.</summary>
[RoomLifetime]
internal sealed class ShaktoolObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>Physical PLM slots scanned from highest index for the hardcoded spawn.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Connects Shaktool's room controller to the resident PLM slots and room owners.</summary>
    internal ShaktoolObjects(IRoomSamusAccess? samus, IGlobalEventFlags? events, RoomObjectSlotTable slotTable)
    {
        _samus = samus;
        _events = events;
        _slotTable = slotTable;
    }
    /// <summary>Spawns the Shaktool room's controller PLM and runs its scroll setup.</summary>
    public bool TrySpawnShaktoolRoomController(RoomScrollGrid scrolls)
    {
        Ensure.NotNull(scrolls);
        // Hardcoded spawning leaves the scrolls untouched when its pool is full.
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = RoomObjectHeaderId.ShaktoolsRoom;
        slot.BlockIndex = 0;
        slot.StartAnimation(new ScriptedRoomObjectAnimation(ScriptedRoomObjectProgram.ShaktoolRoomController));
        slot.FrameHold = 1;
        for (int cell = 0; cell < ShaktoolRoomDefinitions.ScrollCellCount; cell++)
            scrolls.SetStorage(cell, cell == 0 ? RoomScrollState.Blue : RoomScrollState.RedBoundary);
        return true;
    }

    /// <summary>Opens all scroll cells during a power bomb and retires the controller after Samus passes the cleared-path boundary.</summary>
    /// <remarks>Retiring clears the room object's identity but leaves its instruction words in place.</remarks>
    internal void RunShaktoolRoomFrameBehavior(RoomObjectSlot slot, RoomScrollGrid? scrolls,
        bool powerBombExplosionActive)
    {
        if (scrolls is null || _samus?.Current is not { } samus || _events is null)
            throw new InvalidOperationException("Shaktool room controller requires scroll, Samus, and event owners.");
        if (powerBombExplosionActive)
            for (int cell = 0; cell < ShaktoolRoomDefinitions.ScrollCellCount; cell++)
                scrolls.SetStorage(cell, RoomScrollState.Blue);
        if (samus.Position.X.Whole <= ShaktoolRoomDefinitions.ClearedPathXBoundary) return;
        _events.Set(EventNumber.ShaktoolClearedPath);
        // Retiring clears only the identity; inactive instruction/timer words remain in place.
        slot.Header = 0;
        slot.Delete();
    }
}
