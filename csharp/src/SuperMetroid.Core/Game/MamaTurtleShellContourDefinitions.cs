namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled physical contour of Mama Turtle's sleeping shell. The first half describes
/// actors left of the parent origin; the second half describes actors to its right.
/// </summary>
internal static class MamaTurtleShellContourDefinitions
{

    /// <summary>Maximum horizontal distance represented by either contour half.</summary>
    internal const int HalfWidth = 24;

    /// <summary>Signed vertical contour offsets ordered by horizontal distance, first for the shell's left side and then its right side.</summary>
    private static ReadOnlySpan<short> Offsets =>
    [
        -16, -16, -16, -16, -15, -15, -15, -15,
        -15, -14, -13, -13, -12, -11, -10, -9,
        -8, -7, -6, -5, -4, -4, 0, 0,
        -16, -16, -16, -15, -15, -15, -14, -13,
        -12, -11, -10, -9, -8, -7, -6, -5,
        -4, -3, -3, -2, 0, 0, 0, 0,
    ];

    /// <summary>
    /// Returns the native signed vertical contour offset for a parent-minus-actor X
    /// difference whose magnitude is below twenty-four pixels.
    /// </summary>
    internal static short GetOffset(short parentMinusActorX)
    {
        int distance = Math.Abs((int)parentMinusActorX);
        if (distance >= HalfWidth)
            throw new ArgumentOutOfRangeException(nameof(parentMinusActorX));
        int index = parentMinusActorX < 0 ? distance + HalfWidth : distance;
        return Offsets[index];
    }
}
