using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Turns a population entry's selector into the value it selects, and back (#1519), for
/// <see cref="EnemySpawnSettingsCodec"/>: an index into a species' authored table, or a row of the shared linear speed
/// table. A selector past its table is rejected here, where the entry is decoded, so a species never reads one.
/// </summary>
internal static class EnemySpawnSelectors
{
    /// <summary>The crawl-speed selector that keeps a crawler's velocity, so it takes the default speed after its first landing.</summary>
    public const int KeepsVelocitySelector = 255;

    /// <summary>The value <paramref name="selector"/> selects from <paramref name="table"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="selector"/> lies past the table.</exception>
    public static T Select<T>(string setting, T[] table, int selector) =>
        selector >= 0 && selector < Ensure.NotNull(table).Length
            ? table[selector]
            : throw new InvalidDataException(
                $"Population words select {setting} {selector}, past the {table.Length} the species authors.");

    /// <summary>The selector that selects <paramref name="value"/> from <paramref name="table"/>.</summary>
    /// <exception cref="ArgumentException">No selector selects the value.</exception>
    public static int SelectorOf<T>(string setting, T[] table, T value)
    {
        for (int selector = 0; selector < Ensure.NotNull(table).Length; selector++)
        {
            if (EqualityComparer<T>.Default.Equals(table[selector], value))
                return selector;
        }
        throw new ArgumentException($"No population selector gives {setting} {value}.", nameof(value));
    }

    /// <summary>The positive speed of linear-speed row <paramref name="row"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="row"/> lies past the shared table.</exception>
    public static SignedSixteenSixteen LinearSpeed(string setting, int row) =>
        row is >= 0 and < EnemyLinearSpeedTable.RowCount
            ? EnemyLinearSpeedTable.Speed(row)
            : throw new InvalidDataException(
                $"Population words select {setting} row {row}, past the {EnemyLinearSpeedTable.RowCount} linear speeds.");

    /// <summary>The linear-speed row whose positive speed is <paramref name="speed"/>.</summary>
    /// <exception cref="ArgumentException">No row moves at the speed.</exception>
    public static int LinearSpeedRow(string setting, SignedSixteenSixteen speed)
    {
        for (int row = 0; row < EnemyLinearSpeedTable.RowCount; row++)
        {
            if (EnemyLinearSpeedTable.Speed(row) == speed)
                return row;
        }
        throw new ArgumentException($"No linear-speed row gives {setting} {speed.RawValue}.", nameof(speed));
    }

    /// <summary>
    /// The crawl speed a crawler's speed selector gives: a crawl speed from the authored records, or none for the
    /// selector that keeps the crawler's velocity.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="selector"/> lies past the authored records.</exception>
    public static SignedEightEight? CrawlSpeed(int selector) => selector == KeepsVelocitySelector
        ? null
        : selector is >= 0 and < CrawlerSpeedDefinitions.Count
            ? new SignedEightEight(CrawlerSpeeds.ForParameter(selector))
            : throw new InvalidDataException(
                $"Population words select crawl speed {selector}, past the {CrawlerSpeedDefinitions.Count} authored crawl speeds.");

    /// <summary>The crawl speed an orange Zoomer's speed selector gives; its initializer has no use for the selector that keeps a velocity.</summary>
    /// <exception cref="InvalidDataException"><paramref name="selector"/> keeps the velocity or lies past the authored records.</exception>
    public static SignedEightEight? HZoomerCrawlSpeed(int selector) => CrawlSpeed(selector) ??
        throw new InvalidDataException("Population words give an orange Zoomer the selector that keeps its velocity, which its initializer has no speed for.");

    /// <summary>
    /// The first speed selector that gives <paramref name="crawlSpeed"/>; none gives the selector that keeps the velocity.
    /// Two records author eight pixels per update, and the earlier one is chosen.
    /// </summary>
    /// <exception cref="ArgumentException">No authored record crawls at the speed.</exception>
    public static int CrawlSpeedSelector(SignedEightEight? crawlSpeed)
    {
        if (crawlSpeed is not SignedEightEight speed)
            return KeepsVelocitySelector;
        for (int selector = 0; selector < CrawlerSpeedDefinitions.Count; selector++)
        {
            if (CrawlerSpeeds.ForParameter(selector) == speed.RawValue)
                return selector;
        }
        throw new ArgumentException($"No crawl-speed selector gives {speed.RawValue}.", nameof(crawlSpeed));
    }

    /// <summary>The Fune and Namihe launch speeds, each a quarter pixel per update faster than the last.</summary>
    private const int FuneNamiheLaunchSpeedCount = 8;

    /// <summary>The quarter pixel per update, in 256ths of a pixel, by which each Fune and Namihe launch speed is faster.</summary>
    private const int FuneNamiheLaunchSpeedStep = 64;

    /// <summary>The speed at which Fune and Namihe launch selector <paramref name="selector"/> launches fireballs.</summary>
    /// <exception cref="InvalidDataException">
    /// <paramref name="selector"/> is eight or more. The game reads past its eight launch velocity pairs for such a
    /// selector, but no retail placement selects one: every Fune and Namihe placement selects the sixth or eighth.
    /// </exception>
    public static SignedEightEight FuneNamiheLaunchSpeed(int selector) => selector is >= 0 and < FuneNamiheLaunchSpeedCount
        ? new SignedEightEight((selector + 1) * FuneNamiheLaunchSpeedStep)
        : throw new InvalidDataException($"Population words select Fune/Namihe launch speed {selector}, past its {FuneNamiheLaunchSpeedCount} authored values.");

    /// <summary>The Fune and Namihe launch selector that launches fireballs at <paramref name="speed"/>.</summary>
    /// <exception cref="ArgumentException">No selector launches at <paramref name="speed"/>.</exception>
    public static int FuneNamiheLaunchSpeedSelector(SignedEightEight speed)
    {
        int selector = speed.RawValue / FuneNamiheLaunchSpeedStep - 1;
        return selector is >= 0 and < FuneNamiheLaunchSpeedCount && FuneNamiheLaunchSpeed(selector) == speed
            ? selector
            : throw new ArgumentException($"No Fune/Namihe launch selector launches at {speed}.", nameof(speed));
    }

    /// <summary>The Owtch travel distance <paramref name="selector"/> selects.</summary>
    /// <exception cref="InvalidDataException">
    /// <paramref name="selector"/> is eight or more. The game reads past its eight distances for such a selector, but
    /// no retail placement selects one: every Owtch placement selects one of the first four.
    /// </exception>
    public static int OwtchTravelDistance(int selector) =>
        Select("Owtch travel distance", EnemySpawnSelectorDefinitions.OwtchTravelDistances, selector);

    /// <summary>The selector of the Owtch travel distance <paramref name="distance"/>.</summary>
    public static int OwtchTravelDistanceSelector(int distance) =>
        SelectorOf("Owtch travel distance", EnemySpawnSelectorDefinitions.OwtchTravelDistances, distance);

    /// <summary>The Owtch underground wait <paramref name="selector"/> selects.</summary>
    /// <exception cref="InvalidDataException">
    /// <paramref name="selector"/> is six or more. The game reads past its six waits for such a selector, but no retail
    /// placement selects one: every Owtch placement selects the second, fourth or sixth.
    /// </exception>
    public static int OwtchUndergroundWait(int selector) =>
        Select("Owtch underground wait", EnemySpawnSelectorDefinitions.OwtchUndergroundWaits, selector);

    /// <summary>The selector of the Owtch underground wait <paramref name="wait"/>.</summary>
    public static int OwtchUndergroundWaitSelector(int wait) =>
        SelectorOf("Owtch underground wait", EnemySpawnSelectorDefinitions.OwtchUndergroundWaits, wait);

    /// <summary>
    /// The accelerating updates <paramref name="steps"/> selects along the shared quadratic speed curve, which has a
    /// step for each of its rows.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="steps"/> is past the curve.</exception>
    public static int QuadraticSteps(int steps) => steps is >= 0 and < QuadraticSpeedCurveDefinitions.MarkCount
        ? steps
        : throw new InvalidDataException($"Quadratic-speed step {steps} is past the {QuadraticSpeedCurveDefinitions.MarkCount} authored rows.");
}
