namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Discrete destruction anchors, consumed by the rumble's explosion placement.
/// The four-column formation and its interleaved traversal, two-pixel spacing,
/// and four vertical placements specify the selected destruction composition.
/// The producer emits separate effects at these anchors and only then selects
/// their animation through RNG. Generating different anchors would change the chosen
/// impact composition; no trajectory, collision boundary, or physical motion is sampled.
/// </summary>
internal static class CeresDoorRumbleGeometryDefinitions
{
    /// <summary>The rumble emits four separate impact effects around the door.</summary>
    internal const int Count = 4;
    /// <summary>Each interleaved horizontal column is two pixels from the next.</summary>
    internal const int ColumnSpacing = 2;
    /// <summary>Vertical offsets in the emission order, distinct from horizontal column order.</summary>
    internal static readonly short[] VerticalPlacements = [-8, 4, 22, 12];
}
