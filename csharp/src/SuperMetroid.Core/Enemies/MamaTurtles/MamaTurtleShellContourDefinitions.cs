namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The physical contour of Mama Turtle's dormant shell (#2044): its surface's height above an actor beside her origin,
/// by the actor's distance from it, on each side. The shell is not symmetric.
/// </summary>
internal static class MamaTurtleShellContourDefinitions
{
    /// <summary>The distance from her origin, in pixels, either contour reaches; an actor this far or further is clear of the shell.</summary>
    internal const int HalfWidth = 24;

    /// <summary>The shell's surface above an actor at or left of her origin, by the actor's distance from it, 0 through 23 pixels.</summary>
    internal static ReadOnlySpan<int> LeftOfOrigin =>
    [
        -16, -16, -16, -16, -15, -15, -15, -15,
        -15, -14, -13, -13, -12, -11, -10, -9,
        -8, -7, -6, -5, -4, -4, 0, 0,
    ];

    /// <summary>The shell's surface above an actor right of her origin, by the actor's distance from it, 1 through 23 pixels.</summary>
    internal static ReadOnlySpan<int> RightOfOrigin =>
    [
        -16, -16, -16, -15, -15, -15, -14, -13,
        -12, -11, -10, -9, -8, -7, -6, -5,
        -4, -3, -3, -2, 0, 0, 0, 0,
    ];
}
