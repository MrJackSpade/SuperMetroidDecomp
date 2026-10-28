namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// Compiled physical contour of Mama Turtle's sleeping shell. The first half describes
/// actors left of the parent origin; the second half describes actors to its right.
/// </summary>
internal static class MamaTurtleShellContourDefinitions
{

    /// <summary>Maximum horizontal distance represented by either contour half.</summary>
    internal const int HalfWidth = 24;

    /// <summary>Native vertical shell contour: 24 samples per side of the parent origin.</summary>
    internal static ReadOnlySpan<short> Offsets =>
    [
        -16, -16, -16, -16, -15, -15, -15, -15,
        -15, -14, -13, -13, -12, -11, -10, -9,
        -8, -7, -6, -5, -4, -4, 0, 0,
        -16, -16, -16, -15, -15, -15, -14, -13,
        -12, -11, -10, -9, -8, -7, -6, -5,
        -4, -3, -3, -2, 0, 0, 0, 0,
    ];
}
