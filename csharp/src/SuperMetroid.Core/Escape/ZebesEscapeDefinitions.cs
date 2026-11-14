using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// The Zebes escape rooms' shaking and explosions (#1415): the earthquake each escape room shakes with, how often a
/// horizontal shake turns diagonal and for how long, where the explosions go off and which one, and the blocks outdoor
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

    /// <summary>
    /// The chance a lightly shaking room turns diagonal on an update (#1619): the lowest 512 of the 65536 random samples,
    /// one update in 128.
    /// </summary>
    public static readonly RandomChance LightShakeTurnsDiagonal = RandomChance.Lowest(512, RandomSampleField.Whole);

    /// <summary>
    /// The chance a medium shaking room turns diagonal on an update (#1619): the lowest 384 of the 65536 random samples,
    /// three updates in 512.
    /// </summary>
    public static readonly RandomChance MediumShakeTurnsDiagonal = RandomChance.Lowest(384, RandomSampleField.Whole);

    /// <summary>
    /// The updates on which an outdoor escape room sets off an explosion (#1619): every second update of the frame clock's
    /// counter, on its even place.
    /// </summary>
    internal static readonly UpdateCadence OutdoorExplosionEverySecondUpdate = new(2, 0);

    /// <summary>
    /// The updates on which an indoor escape room sets off an explosion (#1619): every fourth update of the frame clock's
    /// counter, on the period's first place.
    /// </summary>
    internal static readonly UpdateCadence IndoorExplosionEveryFourthUpdate = new(4, 0);

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
    /// choices, of which the first eight name an explosion and the rest leave its kind to its place.
    /// </summary>
    internal static readonly RandomSampleField ExplosionChoice = new(0, 4);


    /// <summary>The strong horizontal shake of both layers, enemies included, Mainstreet's escape room shakes with.</summary>
    public static readonly EarthquakeShake MainstreetQuake =
        new(EarthquakeStrength.Strong, EarthquakeDirection.Horizontal, EarthquakeReach.ForegroundBackgroundAndEnemies);

    /// <summary>The strong horizontal foreground shake the Landing Site shakes with during the escape.</summary>
    public static readonly EarthquakeShake LandingQuake =
        new(EarthquakeStrength.Strong, EarthquakeDirection.Horizontal, EarthquakeReach.Foreground);

    /// <summary>
    /// The room mains of the escape rooms that set off random explosions: the outdoor rooms, whose explosions skip blank
    /// blocks, the two rooms whose shaking turns diagonal, and the rooms exploding every fourth update. The escape room
    /// effects run their explosions in exactly these rooms, and the import pictures the explosions only for them.
    /// </summary>
    public static IReadOnlySet<RoomMainBehavior> ExplosionRoomMains { get; } = new HashSet<RoomMainBehavior>
    {
        RoomMainBehavior.ScrollingSkyLandZebesTimebombSet,
        RoomMainBehavior.SetScreenShakingAndGenerateRandomExplosions,
        RoomMainBehavior.ShakeScreenLightHorizontalAndMediumDiagonal,
        RoomMainBehavior.ShakeScreenMediumHorizontalAndStrongDiagonal,
        RoomMainBehavior.GenerateRandomExplosionEveryFourthFrame,
    };
}
