namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Which way a hopper's surface sends its hops (#2473).</summary>
internal static partial class HopperSurfaces
{
    /// <summary>The away from of each surface.</summary>
    private static IdentityTable<HopperSurface, VerticalDirection> AwayFromBySurface => field ??= new(new Dictionary<HopperSurface, VerticalDirection>
    {
        [HopperSurface.Floor] = VerticalDirection.Up,
        [HopperSurface.Ceiling] = VerticalDirection.Down,
    });

    /// <summary>The way a hop leaves <paramref name="surface"/>: up from the floor, down from the ceiling.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="surface"/> is neither the floor nor the ceiling.</exception>
    internal static VerticalDirection AwayFrom(HopperSurface surface) => AwayFromBySurface[surface];
}
