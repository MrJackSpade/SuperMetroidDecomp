namespace SuperMetroid.Core.Progression;

/// <summary>
/// Checked conversions between areas and their zero-based indices in room and save data. The indices are the areas'
/// positions in <see cref="Retail"/>; the area identities' own values are never read.
/// </summary>
public static class AreaIds
{
    /// <summary>The number of area indices accepted by retail room and save data.</summary>
    public const int RetailCount = 7;

    /// <summary>Every retail area in index order: the six Zebes areas, then Ceres.</summary>
    private static readonly AreaId[] Retail =
    [
        AreaId.Crateria, AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia, AreaId.Tourian, AreaId.Ceres,
    ];

    /// <summary>The six Zebes areas in index order; Ceres, the prologue station, is not one of them.</summary>
    public static IReadOnlyList<AreaId> Zebes { get; } = Retail[..^1];

    /// <summary>Validates and names one area index read from room or save data.</summary>
    public static AreaId FromIndex(byte value, string source)
    {
        if (value >= RetailCount)
        {
            throw new InvalidDataException(
                $"{source} contains area ${value:X2}, outside the seven retail areas.");
        }

        return Retail[value];
    }

    /// <summary>The area at zero-based index <paramref name="index"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> names no retail area.</exception>
    public static AreaId At(int index) =>
        (uint)index < RetailCount ? Retail[index] : throw new ArgumentOutOfRangeException(nameof(index), index, "Unknown retail area index.");

    /// <summary>Returns a safe zero-based index for a typed area value.</summary>
    public static int ToIndex(AreaId area)
    {
        int index = Array.IndexOf(Retail, area);
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown retail area ID.");
        return index;
    }

    /// <summary>Whether <paramref name="area"/> is one of the six Zebes areas.</summary>
    public static bool IsZebes(AreaId area) => Array.IndexOf(Retail, area) is >= 0 and < RetailCount - 1;
}
