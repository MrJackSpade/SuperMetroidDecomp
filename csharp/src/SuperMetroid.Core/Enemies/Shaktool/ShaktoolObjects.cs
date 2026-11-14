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
    /// <summary>The room's room objects, which the controller joins.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Connects Shaktool's room controller to the resident room-object slots and room owners.</summary>
    internal ShaktoolObjects(IRoomSamusAccess? samus, IGlobalEventFlags? events, RoomObjectSlotTable slotTable)
    {
        _samus = samus;
        _events = events;
        _slotTable = slotTable;
    }
    /// <summary>
    /// Spawns the Shaktool room's controller and runs its scroll setup: the room's first screen open with the camera
    /// aligned to its top, and every other screen closed.
    /// </summary>
    /// <returns>False when the room is full; the scrolls are then left untouched (<see cref="Compatibility.RoomObjectPlaces"/>).</returns>
    public bool TrySpawnShaktoolRoomController(RoomScrollGrid scrolls)
    {
        Ensure.NotNull(scrolls);
        if (_slotTable.Spawn() is not { } slot)
            return false;

        slot.Block = Position16<RoomBlocks>.Zero;
        slot.Become(new ShaktoolRoomControllerObject(_samus, _events));
        foreach (Position16<RoomScreens> screen in scrolls.Screens)
            scrolls.SetLogicalState(screen, screen == Position16<RoomScreens>.Zero ? RoomScrollState.OpenAlignedToTop : RoomScrollState.Closed);
        return true;
    }
}
