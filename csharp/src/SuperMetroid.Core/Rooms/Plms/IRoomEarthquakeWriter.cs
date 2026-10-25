namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The room-shake writes PLMs make (#633).</summary>
public interface IRoomEarthquakeWriter
{
    /// <summary>Writes the earthquake timer, 0 through 65535.</summary>
    void SetTimer(int timer);

    /// <summary>Writes the earthquake type.</summary>
    void SetType(int type);
}
