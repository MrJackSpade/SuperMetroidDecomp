using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>Checks the blocks a Power Bomb blast's boundary reaches, including clipping and repeated corner visits.</summary>
internal static class PowerBombBoundaryVerification
{
    /// <summary>
    /// Confirms the boundary reaches the expected room blocks in reaction order at two origins and zero radius, each corner
    /// twice, and none for a blast left of and above the room (#2121: the blocks themselves, where the check once counted
    /// an unread reaction list).
    /// </summary>
    internal static void VerifyPowerBombBoundary()
    {
        var level = new RoomLevelData(new Extent16<RoomBlocks>(5, 5), new RoomLevelPlanes(new RoomTerrainBlock[25], new RoomBlockBehavior[25], new RoomBlockAppearance[25]), RoomLevelData.NothingBeyondGrid(), []);
        Check(32, 32, 16, [new(1, 1), new(2, 1), new(3, 1), new(1, 1), new(1, 2), new(1, 2), new(2, 2), new(3, 2), new(3, 1), new(3, 2)]);
        Check(0, 0, 16, [new(0, 0), new(1, 0), new(0, 0), new(0, 0), new(1, 0), new(1, 0)]);
        Check(32, 32, 0, [new(2, 2), new(2, 2), new(2, 2), new(2, 2)]);
        // #1920: a blast both left of and above the room, whose far edges pass 65535, has each far edge held to the
        // room's last block, which leaves every edge before its near one: the blast reaches nothing.
        Check(65530, 65530, 16, []);

        void Check(ushort x, ushort y, int radiusPixels, Position16<RoomBlocks>[] expected)
        {
            IReadOnlyList<Position16<RoomBlocks>> reached = SamusBombProjectileSystem.PowerBombBoundaryBlocks(
                level, new(x, y), PowerBombRadius.FromWholePixels(radiusPixels));
            AssertEqual(string.Join(" ", expected), string.Join(" ", reached),
                $"the boundary of a {radiusPixels}-pixel blast at ({x}, {y}) reaches its blocks in order, corners twice");
        }
        Console.WriteLine("Power Bomb boundary: edge blocks in order, corner revisits, clipping and zero radius agree.");
    }
}
