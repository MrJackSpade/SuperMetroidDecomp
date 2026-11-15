using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one rainbow-beam attack step asks of the room's earthquake: start <paramref name="Shake"/> for
/// <paramref name="Frames"/> frames, or, with no shake, run the room's own shake for that long (zero stops it); with no
/// length, shake until a producer gives the earthquake one (#2013).
/// </summary>
/// <param name="Shake">The shake to start, or null to keep the room's current shake.</param>
/// <param name="Frames">The frames to shake, or null to shake until a producer gives the earthquake a length.</param>
public readonly record struct MotherBrainEarthquakeRequest(EarthquakeShake? Shake, int? Frames)
{
    /// <summary>Applies the request to the room's earthquake.</summary>
    internal void ApplyTo(RoomEarthquake earthquake)
    {
        if (Frames is not int frames)
            earthquake.ShakeIndefinitely(Shake ?? earthquake.Shake);
        else if (Shake is { } shake)
            earthquake.Start(shake, frames);
        else
            earthquake.RunFor(frames);
    }
}
