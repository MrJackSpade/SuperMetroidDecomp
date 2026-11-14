using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Wall-impact request made when Ridley slams into a wall.</summary>
internal static class RidleyWallImpactDefinitions
{
    /// <summary>The wall impact compares the larger absolute 1/256-pixel velocity against 2.5 pixels/frame.</summary>
    public static readonly SignedEightEight MinimumSpeed = SignedEightEight.FromTwoHundredFiftySixthsOfPixel(640);
    /// <summary>The wall impact's strong side-to-side earthquake, outside Norfair, shaking the background and enemies only.</summary>
    public static readonly EarthquakeShake Earthquake =
        new EarthquakeShake(EarthquakeStrength.Strong, EarthquakeDirection.Horizontal, EarthquakeReach.BackgroundAndEnemies);
    /// <summary>The wall impact initializes the quake to twelve frames.</summary>
    public const int Duration = 12;
}
