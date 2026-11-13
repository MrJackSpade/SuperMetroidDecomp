using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Verification.Enemies.Projectiles;

/// <summary>
/// Checks the coarse cells a shot and an enemy projectile meet in (#1632): each axis's cell is the coordinate's 32-pixel
/// cell, the coordinate shifted right five, for every room coordinate.
/// </summary>
internal static class EnemyProjectileCoarseContactVerification
{
    /// <summary>Compares every room coordinate's coarse contact cell with the coordinate shifted right five.</summary>
    internal static void VerifyEnemyProjectileCoarseContactCells()
    {
        int mismatches = 0;
        for (int pixel = 0; pixel <= ushort.MaxValue; pixel++)
            mismatches += EnemyProjectileEngine.CoarseContactCell(pixel) == pixel >> 5 ? 0 : 1;
        AssertEqual(0, mismatches, "every room coordinate's coarse contact cell is the coordinate shifted right five");
        AssertThrows<ArgumentOutOfRangeException>(() => EnemyProjectileEngine.CoarseContactCell(-1), "a coordinate below the room's range is refused");
        AssertThrows<ArgumentOutOfRangeException>(() => EnemyProjectileEngine.CoarseContactCell(ushort.MaxValue + 1),
            "a coordinate past the room's range is refused");
        Console.WriteLine("Enemy projectile coarse contact: every room coordinate's 32-pixel cell matches the shifted coordinate.");
    }
}
