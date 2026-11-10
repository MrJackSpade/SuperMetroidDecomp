using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Verification.Harness;

/// <summary>Room-shake writes a PLM fixture observes; an uninstalled write fails at the dependency (#633).</summary>
internal sealed class FixtureRoomEarthquake : IRoomEarthquakeWriter
{
    /// <summary>Observes earthquake starts.</summary>
    public Action<EarthquakeShake, int>? WriteStart { get; init; }

    /// <summary>Observes earthquake stops.</summary>
    public Action? WriteStop { get; init; }

    /// <summary>Invokes the installed start observer or fails at the missing fixture dependency.</summary>
    public void Start(EarthquakeShake shake, int frames) => (WriteStart ?? throw Missing("start"))(shake, frames);

    /// <summary>Invokes the installed stop observer or fails at the missing fixture dependency.</summary>
    public void Stop() => (WriteStop ?? throw Missing("stop"))();

    /// <summary>Identifies the earthquake writer the PLM fixture failed to install.</summary>
    private static InvalidOperationException Missing(string what) =>
        new($"The PLM fixture did not install an earthquake {what} writer.");
}
