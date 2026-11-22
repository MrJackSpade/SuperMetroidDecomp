using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>
/// A scrolling-sky room's main (#2287): it moves the sky's drifting bands and its vertical scroll, and in the escape's
/// Landing Site then sets off the escape's outdoor explosions.
/// </summary>
/// <param name="sky">The room's sky.</param>
/// <param name="explosions">Where the room's escape explosions go off, or null for a room that sets off none.</param>
[RoomLifetime]
internal sealed partial class ScrollingSkyMain(ScrollingSkyState sky, EscapeExplosionSite? explosions) : IRoomMain
{
    /// <inheritdoc/>
    public RoomMainOutcome Run(RoomMainUpdate update)
    {
        sky.ProcessFrame(update.Room.Camera.PixelPosition, timeIsFrozen: update.TimeIsFrozen);
        if (explosions is { } site)
            update.Escape.StepEscapeRoom(update.Room, site, null);
        return RoomMainOutcome.None;
    }
}
