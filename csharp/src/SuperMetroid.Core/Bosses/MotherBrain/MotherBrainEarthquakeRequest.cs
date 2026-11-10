using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one rainbow-beam attack step asks of the room's earthquake: start <paramref name="Shake"/> for
/// <paramref name="Frames"/> frames, or, with no shake, run the room's own shake for that long (zero stops it).
/// </summary>
/// <param name="Shake">The shake to start, or null to keep the room's current shake.</param>
/// <param name="Frames">The frames to shake, 0 through 65535.</param>
public readonly record struct MotherBrainEarthquakeRequest(EarthquakeShake? Shake, int Frames)
{
    /// <summary>Applies the request to the room's earthquake.</summary>
    internal void ApplyTo(RoomEarthquake earthquake)
    {
        if (Shake is { } shake)
            earthquake.Start(shake, Frames);
        else
            earthquake.RunFor(Frames);
    }
}
