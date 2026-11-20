using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.World;

/// <summary>The Maridia elevatube's main (#2287): the ride its doors start, after gameplay drawing and before the room shakes.</summary>
/// <param name="tube">The tube's ride.</param>
[RoomLifetime]
internal sealed class MaridiaElevatubeMain(MaridiaElevatubeRoomMainState tube) : IRoomMain
{
    /// <inheritdoc/>
    public RoomMainOutcome Run(RoomMainUpdate update)
    {
        tube.Step(update.Room.LevelData, update.Player.Samus, update.FrameCounter, update.Room.RoomObjects);
        return RoomMainOutcome.None;
    }
}
