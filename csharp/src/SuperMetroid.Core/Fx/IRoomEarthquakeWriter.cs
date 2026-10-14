namespace SuperMetroid.Core.Fx;

/// <summary>The room-shake writes PLMs make (#633).</summary>
public interface IRoomEarthquakeWriter
{
    /// <summary>Writes the earthquake timer.</summary>
    void SetTimer(ushort timer);

    /// <summary>Writes the earthquake type.</summary>
    void SetType(ushort type);
}
