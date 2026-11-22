using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// What one rainbow-beam attack step asks of the room's earthquake (#2013): to shake with <paramref name="Shake"/>, or with
/// the room's own shake when it names none, for a number of frames, until a producer gives the earthquake a length, or to
/// stop, the shake staying for a later run.
/// </summary>
/// <param name="Shake">The shake to start, or null to keep the room's current shake.</param>
/// <param name="Length">How long the room shakes.</param>
public readonly partial record struct MotherBrainEarthquakeRequest(EarthquakeShake? Shake, MotherBrainEarthquakeLength Length)
{
    /// <summary>Applies the request to the room's earthquake.</summary>
    internal void ApplyTo(RoomEarthquake earthquake)
    {
        switch (Length)
        {
            case MotherBrainEarthquakeLength.Lasting lasting when Shake is { } shake:
                earthquake.Start(shake, lasting.Frames);
                break;
            case MotherBrainEarthquakeLength.Lasting lasting:
                earthquake.RunFor(lasting.Frames);
                break;
            case var indefinite when indefinite == MotherBrainEarthquakeLength.Indefinite:
                earthquake.ShakeIndefinitely(Shake ?? earthquake.Shake);
                break;
            case var stopped when stopped == MotherBrainEarthquakeLength.Stopped:
                if (Shake is { } kept)
                    earthquake.ChangeShake(kept);
                earthquake.Stop();
                break;
            default:
                throw new InvalidOperationException($"Undefined earthquake length {Length}.");
        }
    }
}
