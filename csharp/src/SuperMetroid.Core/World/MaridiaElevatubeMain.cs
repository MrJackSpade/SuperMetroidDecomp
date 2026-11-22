using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>The Maridia elevatube's main (#2287): the ride its doors start, after gameplay drawing and before the room shakes.</summary>
/// <param name="tube">The tube's ride.</param>
[RoomLifetime]
internal sealed partial class MaridiaElevatubeMain(MaridiaElevatubeRoomMainState tube) : IRoomMain
{
    /// <summary>The tube's ride, which the main steps and checks read.</summary>
    internal readonly MaridiaElevatubeRoomMainState tube = tube;

    /// <summary>
    /// Starts the tube's ride from <paramref name="entry"/>, as the tube's entry door does (#2485): the ride's start and
    /// Samus held still.
    /// </summary>
    /// <param name="entry">The end of the tube Samus enters from.</param>
    /// <param name="samus">Samus, whom the ride holds still.</param>
    internal void StartRide(ElevatubeEntry entry, SamusState samus) => tube.StartRide(entry, samus);

    /// <inheritdoc/>
    public RoomMainOutcome Run(RoomMainUpdate update)
    {
        tube.Step(update.Room.SamusTerrain, update.Player.Samus, update.FrameCounter);
        return RoomMainOutcome.None;
    }
}
