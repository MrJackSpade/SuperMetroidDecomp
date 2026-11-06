using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The tables a population parameter selects a spawn setting from (#1519), for <see cref="EnemySpawnSettingsCodec"/>:
/// each decode turns a selector into the speed, distance or duration it selects, and each encode turns that value back
/// into its selector. The game holds only the selected values. A selector past its table fails loudly: no retail
/// placement selects one, so the game never reads past these tables.
/// </summary>
internal static class SpawnSettingSelectors
{
    /// <summary>The speed shared linear-speed row <paramref name="row"/> moves at, in pixels per update.</summary>
    /// <exception cref="InvalidDataException"><paramref name="row"/> is past the table.</exception>
    public static SignedSixteenSixteen LinearSpeed(int row) => EnemyLinearSpeeds.Row(row).Positive;

    /// <summary>The shared linear-speed row that moves at <paramref name="speed"/>.</summary>
    /// <exception cref="ArgumentException">No row moves at <paramref name="speed"/>.</exception>
    public static int LinearSpeedRow(SignedSixteenSixteen speed)
    {
        int row = speed.RawValue / SignedSixteenSixteen.FromSixteenthsOfPixel(1).RawValue;
        return row is >= 0 and < EnemyLinearSpeedDefinitions.RowCount && LinearSpeed(row) == speed
            ? row
            : throw new ArgumentException($"No linear-speed row moves at {speed}.", nameof(speed));
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

    /// <summary>
    /// The pixels an Owtch walks to either side of its spawn position, by selector: sixteen more for each selector.
    /// Independently reviewed for #1165 against all eight authored words.
    /// </summary>
    private static readonly int[] OwtchTravelDistances = [16, 32, 48, 64, 80, 96, 112, 128];

    /// <summary>
    /// The updates an Owtch stays buried, by selector: thirty-two more for each selector. Independently reviewed for
    /// #1165 against all six NTSC J/U v1.0 words; the PAL table differs.
    /// </summary>
    private static readonly int[] OwtchUndergroundWaits = [32, 64, 96, 128, 160, 192];

    /// <summary>The Owtch travel distance <paramref name="selector"/> selects.</summary>
    /// <exception cref="InvalidDataException">
    /// <paramref name="selector"/> is eight or more. The game reads past its eight distances for such a selector, but
    /// no retail placement selects one: every Owtch placement selects one of the first four.
    /// </exception>
    public static int OwtchTravelDistance(int selector) => Selected(OwtchTravelDistances, selector, "Owtch travel distance");

    /// <summary>The selector of the Owtch travel distance <paramref name="distance"/>.</summary>
    public static int OwtchTravelDistanceSelector(int distance) => Selector(OwtchTravelDistances, distance, "Owtch travel distance");

    /// <summary>The Owtch underground wait <paramref name="selector"/> selects.</summary>
    /// <exception cref="InvalidDataException">
    /// <paramref name="selector"/> is six or more. The game reads past its six waits for such a selector, but no retail
    /// placement selects one: every Owtch placement selects the second, fourth or sixth.
    /// </exception>
    public static int OwtchUndergroundWait(int selector) => Selected(OwtchUndergroundWaits, selector, "Owtch underground wait");

    /// <summary>The selector of the Owtch underground wait <paramref name="wait"/>.</summary>
    public static int OwtchUndergroundWaitSelector(int wait) => Selector(OwtchUndergroundWaits, wait, "Owtch underground wait");

    /// <summary>
    /// The accelerating updates <paramref name="steps"/> selects along the shared quadratic speed curve, which has a
    /// step for each of its rows.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="steps"/> is past the curve.</exception>
    public static int QuadraticSteps(int steps) => steps is >= 0 and < EnemyQuadraticSpeedDefinitions.RowCount
        ? steps
        : throw new InvalidDataException($"Quadratic-speed step {steps} is past the {EnemyQuadraticSpeedDefinitions.RowCount} authored rows.");

    /// <summary>The value of <paramref name="table"/> that <paramref name="selector"/> selects.</summary>
    /// <exception cref="InvalidDataException"><paramref name="selector"/> is past the table.</exception>
    private static int Selected(int[] table, int selector, string setting) => selector >= 0 && selector < table.Length
        ? table[selector]
        : throw new InvalidDataException($"Population words select {setting} {selector}, past its {table.Length} authored values.");

    /// <summary>The selector of <paramref name="value"/> in <paramref name="table"/>.</summary>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not one of the table's values.</exception>
    private static int Selector(int[] table, int value, string setting)
    {
        int selector = Array.IndexOf(table, value);
        return selector >= 0
            ? selector
            : throw new ArgumentException($"No selector gives {setting} {value}.", nameof(value));
    }
}
