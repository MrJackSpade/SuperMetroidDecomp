namespace SuperMetroid.Core.Game;

/// <summary>
/// Discrete destruction anchors at $A6:F840-F84F, consumed by $A6:F7FE-F820.
/// The four-column formation and its interleaved traversal, two-pixel spacing,
/// and four vertical placements specify the selected destruction composition.
/// The native producer emits separate effects at these anchors and only then selects
/// their animation through RNG. Generating different anchors would change the chosen
/// impact composition; no trajectory, collision boundary, or physical motion is sampled.
/// </summary>
internal static class CeresDoorRumbleGeometryDefinitions
{
    /// <summary>Number of discrete anchor positions in the native destruction formation.</summary>
    internal const int Count = 4;
    /// <summary>Horizontal pixel spacing between adjacent formation columns.</summary>
    private const int ColumnSpacing = 2;
    /// <summary>Vertical pixel offsets ordered by the native interleaved anchor traversal.</summary>
    private static readonly short[] VerticalPlacements = [-8, 4, 22, 12];

    /// <summary>
    /// Returns the selected anchor's relative displacement from the Ceres door rumble origin.
    /// </summary>
    /// <param name="index">Zero-based position in the native four-anchor traversal.</param>
    /// <returns>Horizontal and vertical pixel offsets for that anchor.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the four defined anchors.</exception>
    internal static (short X, short Y) Offset(int index)
    {
        if ((uint)index >= Count)
            throw new IndexOutOfRangeException();
        int pair = index / 2;
        int side = index % 2;
        return ((short)(ColumnSpacing * (2 * side + pair - 2)), VerticalPlacements[index]);
    }
}
