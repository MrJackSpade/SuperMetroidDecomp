using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Escape;

/// <summary>
/// The Zebes escape rooms' shaking and explosions (#1415): the earthquake each escape room shakes with, how often a
/// horizontal shake turns diagonal and for how long, where the explosions go off and which one, and the blocks outdoor
/// explosions skip.
/// </summary>
public static partial class ZebesEscapeDefinitions
{
    /// <summary>The updates a temporary diagonal shake lasts before the room returns to its horizontal shake.</summary>
    public const int DiagonalFrames = 42;

    /// <summary>Light horizontal shaking of both layers, enemies included.</summary>
    public static readonly EarthquakeShake LightHorizontal =
        new(EarthquakeStrength.Light, EarthquakeDirection.Horizontal, EarthquakeReach.RoomBlocksSceneryAndEnemies);

    /// <summary>Medium horizontal shaking of both layers, enemies included.</summary>
    public static readonly EarthquakeShake MediumHorizontal =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Horizontal, EarthquakeReach.RoomBlocksSceneryAndEnemies);

    /// <summary>The medium diagonal shake, enemies included, a lightly shaking room turns to.</summary>
    internal static readonly EarthquakeShake MediumDiagonal =
        new(EarthquakeStrength.Medium, EarthquakeDirection.Diagonal, EarthquakeReach.RoomBlocksSceneryAndEnemies);

    /// <summary>The strong diagonal shake, enemies included, a medium shaking room turns to.</summary>
    internal static readonly EarthquakeShake StrongDiagonal =
        new(EarthquakeStrength.Strong, EarthquakeDirection.Diagonal, EarthquakeReach.RoomBlocksSceneryAndEnemies);

    /// <summary>
    /// The chance a lightly shaking room turns diagonal on an update (#1619): one update in 128.
    /// </summary>
    internal static readonly RandomChance LightShakeTurnsDiagonal = RandomChance.Lowest(1, RandomSampleField.RangePart(128));

    /// <summary>
    /// The chance a medium shaking room turns diagonal on an update (#1619): three updates in 512.
    /// </summary>
    internal static readonly RandomChance MediumShakeTurnsDiagonal = RandomChance.Lowest(3, RandomSampleField.RangePart(512));

    /// <summary>
    /// The updates on which an outdoor escape room sets off an explosion (#1619), counted on the frame clock's counter at
    /// the period's even place.
    /// </summary>
    internal static readonly UpdateCadence OutdoorExplosion = new(2, 0);

    /// <summary>
    /// The updates on which an indoor escape room sets off an explosion (#1619), counted on the frame clock's counter at
    /// the period's first place.
    /// </summary>
    internal static readonly UpdateCadence IndoorExplosion = new(4, 0);

    /// <summary>
    /// The random selection that places an explosion across the screen: its pixels right of the camera's left edge, 256
    /// positions.
    /// </summary>
    internal static readonly RandomSampleField ExplosionScreenX = RandomSampleField.OneOf(256);

    /// <summary>
    /// The random selection that places an explosion down the screen: its pixels below the camera's top edge, 256
    /// positions, drawn from the same sample as <see cref="ExplosionScreenX"/>.
    /// </summary>
    internal static readonly RandomSampleField ExplosionScreenY = RandomSampleField.RangePart(256);

    /// <summary>
    /// The random selection, from the explosion's own sample, of which explosion goes off and whether it sounds: 16
    /// choices, which <see cref="ZebesEscapeExplosionDefinitions.Draw"/> names.
    /// </summary>
    internal static readonly RandomSampleField ExplosionChoice = RandomSampleField.OneOf(16);


    /// <summary>The strong horizontal shake of both layers, enemies included, Mainstreet's escape room shakes with.</summary>
    public static readonly EarthquakeShake MainstreetQuake =
        new(EarthquakeStrength.Strong, EarthquakeDirection.Horizontal, EarthquakeReach.RoomBlocksSceneryAndEnemies);

    /// <summary>The strong horizontal foreground shake the Landing Site shakes with during the escape.</summary>
    public static readonly EarthquakeShake LandingQuake =
        new(EarthquakeStrength.Strong, EarthquakeDirection.Horizontal, EarthquakeReach.RoomBlocks);

    /// <summary>A lightly shaking escape room's shaking, which turns medium diagonal one update in 128.</summary>
    public static readonly EscapeDiagonalShaking LightShakingTurnsMediumDiagonal =
        new(LightHorizontal, MediumDiagonal, LightShakeTurnsDiagonal);

    /// <summary>A medium shaking escape room's shaking, which turns strong diagonal three updates in 512.</summary>
    public static readonly EscapeDiagonalShaking MediumShakingTurnsStrongDiagonal =
        new(MediumHorizontal, StrongDiagonal, MediumShakeTurnsDiagonal);
}
