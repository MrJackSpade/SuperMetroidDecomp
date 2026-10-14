using SuperMetroid.Core.Rooms;

/// <summary>Room-shake writes a PLM fixture observes; an uninstalled write fails at the dependency (#633).</summary>
internal sealed class FixtureRoomEarthquake : IRoomEarthquakeWriter
{
    /// <summary>Observes timer writes.</summary>
    public Action<ushort>? WriteTimer { get; init; }

    /// <summary>Observes type writes.</summary>
    public Action<ushort>? WriteType { get; init; }

    /// <summary>Invokes the installed timer observer or fails at the missing fixture dependency.</summary>
    public void SetTimer(ushort timer) => (WriteTimer ?? throw Missing("timer"))(timer);

    /// <summary>Invokes the installed quake-type observer or fails at the missing fixture dependency.</summary>
    public void SetType(ushort type) => (WriteType ?? throw Missing("type"))(type);

    /// <summary>Identifies the earthquake writer the PLM fixture failed to install.</summary>
    private static InvalidOperationException Missing(string what) =>
        new($"The PLM fixture did not install an earthquake {what} writer.");
}
