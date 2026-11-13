namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where one of Samus's shots meets an enemy projectile (#1620, was <c>EnemyProjectileEngine.CoarseContactCell</c>): only
/// when both stand in the same 32-pixel square of the room, the squares counted from the room's top left corner, whatever
/// the sizes of the shot and the projectile. A shot crossing a projectile just over a square's edge passes it by, and one
/// far from it but inside its square destroys it; nothing about the two pictures' overlap is tested.
/// </summary>
/// <remarks>
/// Consulted for each live shot that can strike against each live projectile that blocks shots, in the projectile pass's
/// shot test, after both have moved this update. Evidence: the enemy projectile and shot collision handling
/// (<c>$A0:996C</c>) masks both X positions and then both Y positions to their 32-pixel square (<c>$A0:99BA</c>-<c>$A0:99DC</c>)
/// and handles the hit only when both pairs match, for every projectile and shot it tests; Kago bugs, Nuclear Waffles and
/// every other shootable projectile are reached in retail play. Positions are whole room coordinates, so the squares cover
/// the coordinate space in 2048 columns and rows. The shot and projectile squares check compares every coordinate's
/// square with that mask.
/// </remarks>
[CompatibilityService("a shot meets an enemy projectile only when both stand in the same 32-pixel square of the room, whatever their sizes")]
internal static class ShotAndProjectileSquares
{
    /// <summary>The pixels along each side of a square a shot and an enemy projectile meet in.</summary>
    private const int SquarePixels = 32;

    /// <summary>Whether a shot at <paramref name="shot"/> meets an enemy projectile at <paramref name="projectile"/>.</summary>
    /// <param name="shot">The shot's whole room position.</param>
    /// <param name="projectile">The enemy projectile's whole room position.</param>
    internal static bool Meet(Position16<RoomPixels> shot, Position16<RoomPixels> projectile) =>
        Square(shot.X) == Square(projectile.X) && Square(shot.Y) == Square(projectile.Y);

    /// <summary>The square, along one axis, that holds the whole room coordinate <paramref name="pixel"/>.</summary>
    /// <param name="pixel">The coordinate along the axis.</param>
    internal static int Square(int pixel) => Ensure.BetweenInclusive(0, ushort.MaxValue, pixel) / SquarePixels;
}
