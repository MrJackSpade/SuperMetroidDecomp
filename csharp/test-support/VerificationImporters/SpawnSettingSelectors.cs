using SuperMetroid.Core;
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

    /// <summary>
    /// The accelerating updates <paramref name="steps"/> selects along the shared quadratic speed curve, which has a
    /// step for each of its rows.
    /// </summary>
    /// <exception cref="InvalidDataException"><paramref name="steps"/> is past the curve.</exception>
    public static int QuadraticSteps(int steps) => steps is >= 0 and < EnemyQuadraticSpeedDefinitions.RowCount
        ? steps
        : throw new InvalidDataException($"Quadratic-speed step {steps} is past the {EnemyQuadraticSpeedDefinitions.RowCount} authored rows.");
}
