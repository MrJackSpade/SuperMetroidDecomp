using System.Collections.Immutable;
using static SuperMetroid.Core.DefinitionRun;
namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>
/// The physical contour of Mama Turtle's dormant shell (#2044): its surface's height above an actor beside her origin,
/// by the actor's distance from it, on each side. The shell is not symmetric.
/// </summary>
internal static partial class MamaTurtleShellContourDefinitions
{
    /// <summary>The distance from her origin, in pixels, either contour reaches; an actor this far or further is clear of the shell.</summary>
    internal const int HalfWidth = 24;

    /// <summary>The shell's surface above an actor at or left of her origin, by the actor's distance from it, 0 through 23 pixels.</summary>
    internal static ImmutableArray<int> LeftOfOrigin { get; } =
    [
        .. Repeated(-16, 4), .. Repeated(-15, 5), -14, -13, -13, -12, -11, -10, -9,
        -8, -7, -6, -5, -4, -4, 0, 0,
    ];

    /// <summary>
    /// The shell's surface above an actor right of her origin, by the actor's distance from it, 1 through 23 pixels; an
    /// actor at her origin takes <see cref="LeftOfOrigin"/>.
    /// </summary>
    internal static ImmutableArray<int> RightOfOrigin { get; } =
    [
             -16, -16, .. Repeated(-15, 3), -14, -13,
        -12, -11, -10, -9, -8, -7, -6, -5,
        -4, -3, -3, -2, .. Repeated(0, 4),
    ];
}
