namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>
/// Checks the squares a shot and an enemy projectile meet in (#1620, was the coarse contact cell of #1632) against the
/// original collision handling's mask of each coordinate to its 32-pixel square.
/// </summary>
internal static class ShotAndProjectileSquaresVerification
{
    /// <summary>The values a sixteen-bit coordinate takes.</summary>
    private const int CoordinateRange = 65536;

    /// <summary>The original's mask of a coordinate to the start of its 32-pixel square.</summary>
    private const int SquareMask = 0xffe0;

    /// <summary>
    /// Every room coordinate's square orders as the masked coordinate does, so two coordinates share a square exactly when
    /// their masks match. Every shot coordinate is then met, along X and then along Y with the other axis shared, against
    /// projectile coordinates at both edges of a square, either side of each edge and at both ends of the coordinate space;
    /// <see cref="ShotAndProjectileSquares.Meet"/> agrees with both masks matching. A coordinate outside the space is refused.
    /// </summary>
    internal static void VerifyShotAndProjectileSquares()
    {
        for (int pixel = 0; pixel < CoordinateRange; pixel++)
        {
            if (ShotAndProjectileSquares.Square(pixel) * 32 != (pixel & SquareMask))
                throw new InvalidOperationException($"Coordinate {pixel} lies in square {ShotAndProjectileSquares.Square(pixel)}, not the masked {pixel & SquareMask}.");
        }

        int[] projectiles = [0, 31, 32, 33, 4095, 4096, 4097, 32767, 32768, 65503, 65504, 65535];
        for (int shot = 0; shot < CoordinateRange; shot++)
        foreach (int projectile in projectiles)
        {
            bool expected = (shot & SquareMask) == (projectile & SquareMask);
            Check(new Position16<RoomPixels>(shot, 200), new Position16<RoomPixels>(projectile, 200), expected);
            Check(new Position16<RoomPixels>(200, shot), new Position16<RoomPixels>(200, projectile), expected);
            Check(new Position16<RoomPixels>(shot, shot), new Position16<RoomPixels>(projectile, 200),
                expected && (shot & SquareMask) == (200 & SquareMask));
        }

        AssertThrows<ArgumentOutOfRangeException>(() => ShotAndProjectileSquares.Square(-1), "a coordinate below the room's range is refused");
        AssertThrows<ArgumentOutOfRangeException>(() => ShotAndProjectileSquares.Square(CoordinateRange), "a coordinate past the room's range is refused");
        Console.WriteLine("  Shot and projectile squares: every coordinate's square and every tested meeting match the original's 32-pixel mask.");
    }

    /// <summary>Checks whether a shot at <paramref name="shot"/> meets a projectile at <paramref name="projectile"/>.</summary>
    private static void Check(Position16<RoomPixels> shot, Position16<RoomPixels> projectile, bool expected)
    {
        if (ShotAndProjectileSquares.Meet(shot, projectile) != expected)
            throw new InvalidOperationException($"A shot at {shot} {(expected ? "misses" : "meets")} a projectile at {projectile}.");
    }
}
