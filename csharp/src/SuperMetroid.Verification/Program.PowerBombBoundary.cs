using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyPowerBombBoundary()
    {
        var level = new RoomLevelData(5, 5, new ushort[25], new byte[25], new ushort[25], new byte[8]);
        Check(32, 32, 0x1000, [(1,1), (2,1), (3,1), (1,1), (1,2),
            (1,2), (2,2), (3,2), (3,1), (3,2)]);
        Check(0, 0, 0x1000, [(0,0), (1,0), (0,0), (0,0), (1,0), (1,0)]);
        Check(32, 32, 0, [(2,2), (2,2), (2,2), (2,2)]);

        void Check(ushort x, ushort y, ushort radius, (int X, int Y)[] expected)
        {
            // Both a normal Power Bomb and the Chainsaw word pass through the same
            // boundary walker. This checks traversal only, not their PLM damage class.
            foreach (ushort type in new ushort[] { 0x0300, 0x800d })
            {
                var visited = new List<BombBlockReaction>();
                SamusBombProjectileSystem.CollectPowerBombBoundaryReactions(level, x, y, radius,
                    visited, null, AreaId.Crateria, type);
                AssertEqual(expected.Length, visited.Count, "inclusive Power Bomb edge visit count");
                for (int index = 0; index < expected.Length; index++)
                {
                    AssertEqual(expected[index].X, visited[index].BlockX, $"boundary visit {index} X");
                    AssertEqual(expected[index].Y, visited[index].BlockY, $"boundary visit {index} Y");
                }
            }
        }
        Console.WriteLine("Power Bomb boundary: ordered edges, corner revisits, clipping and zero radius agree for both caller words.");
    }
}
