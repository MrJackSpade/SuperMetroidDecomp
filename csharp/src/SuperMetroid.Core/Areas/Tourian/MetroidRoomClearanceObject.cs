using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// A Metroid room's clearance watch (#1554): it stays in the room for good and, on every room-object update from its
/// first, marks its room's cleared event once the room's enemy deaths reach the room's kill quota.
/// </summary>
/// <param name="cleared">The event the watch marks.</param>
internal sealed partial class MetroidRoomClearanceObject(StoryEvent cleared) : RoomObjectBehaviour
{
    /// <summary>The event the watch marks once the quota is met.</summary>
    internal StoryEvent Cleared { get; } = cleared;

    /// <inheritdoc/>
    internal override void Update(RoomObjectPlace place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (context.Frame.EnemyDeaths < context.Frame.EnemyDeathQuota)
            return;
        // The event is marked again on every update the quota stays met, which changes nothing once it is set.
        context.RoomObjects.Events.Set(Cleared);
    }
}
