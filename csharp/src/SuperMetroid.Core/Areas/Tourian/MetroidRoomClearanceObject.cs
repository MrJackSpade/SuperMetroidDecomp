using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// A Metroid room's clearance watch (#1554): it stays in the room for good and, on every room-object update from its
/// first, marks its room's cleared event once the room's enemy deaths reach the room's kill quota. A placement that names
/// no event (<see cref="MetroidRoomArgument.ObservesNothing"/>) watches nothing.
/// </summary>
/// <param name="cleared">The event the watch marks, or null for a watch that marks nothing.</param>
internal sealed class MetroidRoomClearanceObject(StoryEvent? cleared) : RoomObjectBehaviour
{
    /// <summary>The event the watch marks once the quota is met, or null for one that marks nothing.</summary>
    internal StoryEvent? Cleared { get; } = cleared;

    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (Cleared is not { } cleared || context.Frame.EnemyDeaths < context.Frame.EnemyDeathQuota)
            return;
        // The event is marked again on every update the quota stays met, which changes nothing once it is set.
        (context.RoomObjects.Events ?? throw new InvalidOperationException(
            "A Metroid room's clearance watch met its quota without a room event writer.")).Set(cleared);
    }
}
