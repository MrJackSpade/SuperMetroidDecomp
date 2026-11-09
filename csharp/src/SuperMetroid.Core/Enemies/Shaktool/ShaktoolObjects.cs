using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Shaktool's room controller: its spawn and the room's scroll setup.</summary>
[RoomLifetime]
internal sealed class ShaktoolObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>Physical room-object slots scanned from highest index for the hardcoded spawn.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Connects Shaktool's room controller to the resident room-object slots and room owners.</summary>
    internal ShaktoolObjects(IRoomSamusAccess? samus, IGlobalEventFlags? events, RoomObjectSlotTable slotTable)
    {
        _samus = samus;
        _events = events;
        _slotTable = slotTable;
    }
    /// <summary>Spawns the Shaktool room's controller and runs its scroll setup.</summary>
    public bool TrySpawnShaktoolRoomController(RoomScrollGrid scrolls)
    {
        Ensure.NotNull(scrolls);
        // Hardcoded spawning leaves the scrolls untouched when its pool is full.
        if (_slotTable.SpawnHardcoded() is not { } slot)
            return false;

        slot.Header = RoomObjectHeaderId.ShaktoolRoomController;
        slot.BlockIndex = 0;
        slot.Become(new ShaktoolRoomControllerObject(_samus, _events));
        for (int cell = 0; cell < ShaktoolRoomDefinitions.ScrollCellCount; cell++)
            scrolls.SetCell(cell, cell == 0 ? RoomScrollState.Blue : RoomScrollState.RedBoundary);
        return true;
    }
}
