using static SuperMetroid.Core.Enemies.EnemyQuadraticSpeedDefinitions;

namespace SuperMetroid.Core.Enemies;

/// <summary>The shared quadratic enemy speed table, whose speeds grow with the triangle numbers.</summary>
public static class EnemyQuadraticSpeeds
{
    /// <summary>Every row, by row number.</summary>
    private static readonly EnemyQuadraticSpeedRow[] Rows = [.. Enumerable.Range(0, RowCount).Select(Build)];

    /// <summary>The speeds of row <paramref name="row"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="row"/> is not one of the authored rows.</exception>
    public static EnemyQuadraticSpeedRow Row(int row) => (uint)row < (uint)Rows.Length
        ? Rows[row]
        : throw new InvalidDataException($"Enemy quadratic-speed row {row} is outside the {RowCount} authored rows.");

    /// <summary>
    /// Row <paramref name="row"/>: the triangle number in 256ths of a pixel, its fraction widened by
    /// <see cref="FractionMultiplier"/> with the carry past the fraction dropped, and that speed negated.
    /// </summary>
    private static EnemyQuadraticSpeedRow Build(int row)
    {
        int triangle = row * (row + 1) / 2;
        int speed = SixteenSixteen.FromWhole(EightEight.WholeOf(triangle)) |
            SixteenSixteen.FractionOf(EightEight.FractionOf(triangle) * FractionMultiplier);
        return new EnemyQuadraticSpeedRow(speed, -speed);
    }
}
