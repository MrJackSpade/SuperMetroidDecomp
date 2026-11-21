namespace SuperMetroid.Core.Earthquakes;

/// <summary>
/// How each part of an <see cref="EarthquakeShake"/> displaces the room: the pixels each strength throws, the axes each
/// direction moves and what each reach shakes. Together they give the displacement of every earthquake the game shows.
/// </summary>
public static partial class EarthquakeShakeDefinitions
{
    /// <summary>The pixels each strength throws what it shakes along every axis it moves.</summary>
    public static IReadOnlyDictionary<EarthquakeStrength, int> Pixels { get; } = new Dictionary<EarthquakeStrength, int>
    {
        [EarthquakeStrength.Light] = 1,
        [EarthquakeStrength.Medium] = 2,
        [EarthquakeStrength.Strong] = 3,
    };

    /// <summary>The directions that throw what they shake side to side.</summary>
    public static IReadOnlySet<EarthquakeDirection> HorizontalDirections { get; } = new HashSet<EarthquakeDirection>
    {
        EarthquakeDirection.Horizontal,
        EarthquakeDirection.Diagonal,
    };

    /// <summary>The directions that throw what they shake up and down.</summary>
    public static IReadOnlySet<EarthquakeDirection> VerticalDirections { get; } = new HashSet<EarthquakeDirection>
    {
        EarthquakeDirection.Vertical,
        EarthquakeDirection.Diagonal,
    };

    /// <summary>The reaches that shake the foreground layer.</summary>
    public static IReadOnlySet<EarthquakeReach> RoomBlockReaches { get; } = new HashSet<EarthquakeReach>
    {
        EarthquakeReach.RoomBlocks,
        EarthquakeReach.RoomBlocksAndScenery,
        EarthquakeReach.RoomBlocksSceneryAndEnemies,
    };

    /// <summary>The reaches that shake the background layer.</summary>
    public static IReadOnlySet<EarthquakeReach> SceneryReaches { get; } = new HashSet<EarthquakeReach>
    {
        EarthquakeReach.RoomBlocksAndScenery,
        EarthquakeReach.RoomBlocksSceneryAndEnemies,
        EarthquakeReach.SceneryAndEnemies,
    };

    /// <summary>
    /// The reaches that shake the enemies, each update giving them another
    /// <see cref="SuperMetroid.Core.Fx.RoomEarthquakeDefinitions.EnemyShakeDuration"/> updates of shaking, and draw the
    /// enemies' projectiles displaced with them.
    /// </summary>
    public static IReadOnlySet<EarthquakeReach> EnemyReaches { get; } = new HashSet<EarthquakeReach>
    {
        EarthquakeReach.RoomBlocksSceneryAndEnemies,
        EarthquakeReach.SceneryAndEnemies,
    };
}
