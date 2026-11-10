using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The room's earthquake as room objects start and stop it (#633).</summary>
public interface IRoomEarthquakeWriter
{
    /// <summary>Starts the room's earthquake: <paramref name="shake"/> for <paramref name="frames"/> frames, 0 through 65535.</summary>
    void Start(EarthquakeShake shake, int frames);

    /// <summary>Stops the room's earthquake.</summary>
    void Stop();
}
