using static SuperMetroid.Core.Enemies.EnemyLinearSpeedDefinitions;

namespace SuperMetroid.Core.Enemies;

/// <summary>The shared linear enemy speed table: row <c>n</c> moves <c>n</c> sixteenths of a pixel per frame.</summary>
public static class EnemyLinearSpeeds
{
    /// <summary>Every row, by row number.</summary>
    private static readonly EnemyLinearSpeedRow[] Rows =
    [
        .. Enumerable.Range(0, RowCount).Select(row => new EnemyLinearSpeedRow(
            SignedSixteenSixteen.FromSixteenthsOfPixel(row), SignedSixteenSixteen.FromSixteenthsOfPixel(-row))),
    ];

    /// <summary>The speeds of row <paramref name="row"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="row"/> is not one of the authored rows.</exception>
    public static EnemyLinearSpeedRow Row(int row) => (uint)row < (uint)Rows.Length
        ? Rows[row]
        : throw new InvalidDataException($"Enemy linear-speed row {row} is outside the {RowCount} authored rows.");
}
