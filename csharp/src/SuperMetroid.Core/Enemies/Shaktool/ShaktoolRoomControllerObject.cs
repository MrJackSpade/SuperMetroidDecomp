using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// Shaktool's room controller (#1554). From its second update on, every update it opens all of the room's scroll cells
/// while a power bomb explodes, and once Samus is past the cleared path's boundary it marks the path cleared and
/// leaves the room.
/// </summary>
/// <param name="samus">Samus as the room reads her.</param>
/// <param name="events">The game's event flags.</param>
internal sealed class ShaktoolRoomControllerObject(IRoomSamusAccess? samus, IGlobalEventFlags? events) : RoomObjectBehaviour
{
    /// <summary>Samus as the room reads her.</summary>
    private readonly IRoomSamusAccess? _samus = samus;

    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events = events;

    /// <summary>Whether the controller has had its first update and now watches the room.</summary>
    internal bool Watching { get; private set; }

    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (!Watching)
        {
            Watching = true;
            return;
        }
        RoomScrollGrid? scrolls = context.Frame.Scrolls;
        if (scrolls is null || _samus?.Current is not { } current || _events is null)
            throw new InvalidOperationException("Shaktool room controller requires scroll, Samus, and event owners.");
        if (context.Frame.PowerBombExplosionActive)
            for (int cell = 0; cell < ShaktoolRoomDefinitions.ScrollCellCount; cell++)
                scrolls.SetCell(cell, RoomScrollState.OpenAlignedToTop);
        if (current.Position.X.Whole <= ShaktoolRoomDefinitions.ClearedPathXBoundary)
            return;
        _events.Set(StoryEvent.ShaktoolClearedPath);
        context.RoomObjects.DeleteObject(place);
    }
}
