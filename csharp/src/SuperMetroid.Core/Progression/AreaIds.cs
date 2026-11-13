namespace SuperMetroid.Core.Progression;

/// <summary>
/// The seven areas and checks on them. Imported data and save files number the areas; that numbering is the import's
/// and the save files' own, and the area identities' own values are never read.
/// </summary>
public static class AreaIds
{
    /// <summary>The number of areas.</summary>
    public const int RetailCount = 7;

    /// <summary>Every area: the six Zebes areas, then Ceres, the prologue station.</summary>
    public static IReadOnlyList<AreaId> All { get; } =
    [
        AreaId.Crateria, AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia, AreaId.Tourian, AreaId.Ceres,
    ];

    /// <summary>The six Zebes areas; Ceres, the prologue station, is not one of them.</summary>
    public static IReadOnlyList<AreaId> Zebes { get; } = [.. All.Where(area => area != AreaId.Ceres)];

    /// <summary><paramref name="area"/>, checked to be one of the seven areas.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is not an area.</exception>
    public static AreaId Require(AreaId area) =>
        All.Contains(area) ? area : throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown retail area ID.");

    /// <summary>
    /// The area at <paramref name="index"/> of <see cref="All"/>, for the file-select world map's and the area-map
    /// presentation's per-area tables, which list the areas in that order.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> names no area.</exception>
    public static AreaId At(int index) =>
        (uint)index < RetailCount ? All[index] : throw new ArgumentOutOfRangeException(nameof(index), index, "Unknown retail area index.");

    /// <summary>
    /// The position of <paramref name="area"/> in <see cref="All"/>, for the file-select world map's and the area-map
    /// presentation's per-area tables, which list the areas in that order.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is not an area.</exception>
    public static int ToIndex(AreaId area)
    {
        for (int index = 0; index < All.Count; index++)
            if (All[index] == area)
                return index;
        throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown retail area ID.");
    }

    /// <summary>Whether <paramref name="area"/> is one of the six Zebes areas.</summary>
    public static bool IsZebes(AreaId area) => area != AreaId.Ceres && All.Contains(area);
}
