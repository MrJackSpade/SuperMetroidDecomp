using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>How the room shakes as Kraid rises through its floor.</summary>
internal static class KraidRiseDefinitions
{
    /// <summary>The medium diagonal foreground shake Kraid's setup gives the room for his rise.</summary>
    internal static readonly EarthquakeShake RiseShake =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Diagonal, EarthquakeReach.RoomBlocks);
}
