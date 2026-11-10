using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// The Zebes escape rooms' shaking and explosions (#1415): the earthquake each escape room shakes with, how often a
/// horizontal shake turns diagonal and for how long, where the explosions go off and which one, and the tile outdoor
/// explosions skip.
/// </summary>
public static class ZebesEscapeDefinitions
{
    /// <summary>The updates a temporary diagonal shake lasts before the room returns to its horizontal shake.</summary>
    public const int DiagonalFrames = 42;

    /// <summary>Light horizontal shaking of both layers, enemies included.</summary>
    public static readonly EarthquakeShake LightHorizontal =
        new(EarthquakeStrength.Light, EarthquakeDirection.Horizontal, EarthquakeReach.ForegroundBackgroundAndEnemies);

    /// <summary>Medium horizontal shaking of both layers, enemies included.</summary>
    public static readonly EarthquakeShake MediumHorizontal =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Horizontal, EarthquakeReach.ForegroundBackgroundAndEnemies);

    /// <summary>The medium diagonal shake, enemies included, a lightly shaking room turns to.</summary>
    public static readonly EarthquakeShake MediumDiagonal =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Diagonal, EarthquakeReach.ForegroundBackgroundAndEnemies);

    /// <summary>The strong diagonal shake, enemies included, a medium shaking room turns to.</summary>
    public static readonly EarthquakeShake StrongDiagonal =
        new(EarthquakeStrength.Strong, EarthquakeDirection.Diagonal, EarthquakeReach.ForegroundBackgroundAndEnemies);

    /// <summary>A lightly shaking room turns diagonal on an update whose random number is below this.</summary>
    public const int LightChance = 512;

    /// <summary>A medium shaking room turns diagonal on an update whose random number is below this.</summary>
    public const int MediumChance = 384;

    /// <summary>
    /// The random selection that places an explosion across the screen: its pixels right of the camera's left edge, 256
    /// positions.
    /// </summary>
    internal static readonly RandomSampleField ExplosionScreenX = new(0, 8);

    /// <summary>
    /// The random selection that places an explosion down the screen: its pixels below the camera's top edge, 256
    /// positions, drawn from the same sample as <see cref="ExplosionScreenX"/>.
    /// </summary>
    internal static readonly RandomSampleField ExplosionScreenY = new(8, 8);

    /// <summary>
    /// The random selection, from the explosion's own sample, of which explosion goes off and whether it sounds: 16
    /// choices, of which the first eight name an explosion and the rest keep the inherited one.
    /// </summary>
    internal static readonly RandomSampleField ExplosionChoice = new(0, 4);

    /// <summary>The blank foreground tile outdoor explosions skip.</summary>
    public const int BlankTile = 255;

    /// <summary>The strong horizontal shake of both layers, enemies included, Mainstreet's escape room shakes with.</summary>
    public static readonly EarthquakeShake MainstreetQuake =
        new(EarthquakeStrength.Strong, EarthquakeDirection.Horizontal, EarthquakeReach.ForegroundBackgroundAndEnemies);

    /// <summary>The strong horizontal foreground shake the Landing Site shakes with during the escape.</summary>
    public static readonly EarthquakeShake LandingQuake =
        new(EarthquakeStrength.Strong, EarthquakeDirection.Horizontal, EarthquakeReach.Foreground);

    /// <summary>
    /// The room mains of the escape rooms that set off random explosions: the outdoor rooms, whose explosions skip blank
    /// tiles, the two rooms whose shaking turns diagonal, and the rooms exploding every fourth update. The escape room
    /// effects run their explosions in exactly these rooms, and the import pictures the explosions only for them.
    /// </summary>
    public static IReadOnlySet<RoomMainCallback> ExplosionRoomMains { get; } = new HashSet<RoomMainCallback>
    {
        RoomMainCallback.ScrollingSkyLandZebesTimebombSet,
        RoomMainCallback.SetScreenShakingAndGenerateRandomExplosions,
        RoomMainCallback.ShakeScreenLightHorizontalAndMediumDiagonal,
        RoomMainCallback.ShakeScreenMediumHorizontalAndStrongDiagonal,
        RoomMainCallback.GenerateRandomExplosionEveryFourthFrame,
    };
}
