using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The cartridge's shared linear enemy speed table, whose rows population entries select (#1821): row <c>n</c> moves an
/// enemy <c>n</c> sixteenths of a pixel per frame, stored with its exact negation. The game holds the selected speed;
/// the table's extent is the import's knowledge.
/// </summary>
/// <remarks>
/// Reviewed for #1165 against the Japanese and North American v1.0 release; the European release authors a different
/// table.
/// </remarks>
internal static class EnemyLinearSpeedTable
{
    /// <summary>The number of rows, zero through 64: from standing still to four pixels per frame.</summary>
    public const int RowCount = 65;

    /// <summary>The speed toward increasing coordinates that row <paramref name="row"/> stores.</summary>
    /// <exception cref="InvalidDataException"><paramref name="row"/> is not one of the authored rows.</exception>
    public static SignedSixteenSixteen Speed(int row) => row is >= 0 and < RowCount
        ? SignedSixteenSixteen.FromSixteenthsOfPixel(row)
        : throw new InvalidDataException($"Enemy linear-speed row {row} is outside the {RowCount} authored rows.");
}
