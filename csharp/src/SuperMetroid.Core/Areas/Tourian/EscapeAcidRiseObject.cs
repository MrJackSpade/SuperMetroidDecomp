using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// The acid trigger of the escape room before the old Tourian escape shaft (#1800). From its second update on it watches
/// for Samus passing its trigger point; on the update she does it gives the room's acid its rising speed and the delay
/// before it rises, and leaves the room in the same update. The acid's own motion then waits out the delay and rises.
/// </summary>
/// <param name="samus">Samus as the room reads her.</param>
/// <param name="roomFx">The room's liquid motion, which the trigger starts.</param>
internal sealed class EscapeAcidRiseObject(IRoomSamusAccess? samus, IRoomFxMotion? roomFx) : RoomObjectBehaviour
{
    /// <summary>Samus as the room reads her.</summary>
    private readonly IRoomSamusAccess? _samus = samus;

    /// <summary>The room's liquid motion, which the trigger starts.</summary>
    private readonly IRoomFxMotion? _roomFx = roomFx;

    /// <summary>Whether the trigger watches for Samus yet: its first update only starts the watch.</summary>
    private bool _watching;

    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (!_watching)
        {
            _watching = true;
            return;
        }

        IRoomSamus current = _samus?.Current
            ?? throw new InvalidOperationException("Escape acid trigger has no live Samus owner.");
        if (!EscapeAcidRiseObjectDefinitions.TriggerPoint.IsPassedBy(current))
            return;
        (_roomFx ?? throw new InvalidOperationException("Escape acid trigger has no room-FX owner.")).SetLiquidMotion(
            levelSpeed: EscapeAcidRiseObjectDefinitions.RiseSpeed,
            moveDelay: EscapeAcidRiseObjectDefinitions.RiseDelay);
        context.RoomObjects.DeleteObject(place);
    }
}
