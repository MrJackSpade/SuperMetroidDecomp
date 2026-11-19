using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>The Ceres pre-elevator hall's main (#2287): its falling debris once Ridley has escaped.</summary>
/// <param name="debris">The countdown to the next falling tile.</param>
[RoomLifetime]
internal sealed class CeresFallingDebrisMain(CeresFallingDebrisRoomMainState debris) : IRoomMain
{
    /// <inheritdoc/>
    public RoomMainOutcome Run(RoomMainUpdate update)
    {
        update.Mains.RunCeresFallingDebrisRoomMain(update.Room, debris);
        return RoomMainOutcome.None;
    }
}
