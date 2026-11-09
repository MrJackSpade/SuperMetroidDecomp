using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Areas.WreckedShip;

/// <summary>
/// The Wrecked Ship entrance treadmill (#1554). On its first update, if Phantoon was defeated when it spawned, it turns
/// its row of blank blocks into a running belt pushing in its direction; either way it then leaves the room.
/// </summary>
/// <param name="direction">The belt's push direction.</param>
/// <param name="areaBossDefeated">Whether Phantoon was defeated when the treadmill spawned.</param>
internal sealed class WreckedShipTreadmillObject(WreckedShipTreadmillDirection direction, bool areaBossDefeated) : RoomObjectBehaviour
{
    /// <summary>The belt's push direction.</summary>
    internal WreckedShipTreadmillDirection Direction { get; } = direction;

    /// <summary>Whether Phantoon was defeated when the treadmill spawned.</summary>
    internal bool AreaBossDefeated { get; } = areaBossDefeated;

    /// <inheritdoc/>
    internal override void Update(RoomObjectSlot place, RoomObjectStepContext context)
    {
        Ensure.NotNull(place);
        if (AreaBossDefeated)
            WreckedShipTreadmillObjects.RunBelt(context.Level, place.BlockIndex, Direction, context.RoomObjects.Drawing);
        context.RoomObjects.DeleteObject(place);
    }
}
