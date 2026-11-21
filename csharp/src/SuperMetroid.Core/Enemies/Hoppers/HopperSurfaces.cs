namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Which way a hopper's surface sends its hops (#2473).</summary>
internal static partial class HopperSurfaces
{
    /// <summary>The way a hop leaves <paramref name="surface"/>: up from the floor, down from the ceiling.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="surface"/> is neither the floor nor the ceiling.</exception>
    internal static VerticalDirection AwayFrom(HopperSurface surface) => surface switch
    {
        HopperSurface.Floor => VerticalDirection.Up,
        HopperSurface.Ceiling => VerticalDirection.Down,
        _ => throw new ArgumentOutOfRangeException(nameof(surface), surface, "Undefined hopper surface."),
    };
}
